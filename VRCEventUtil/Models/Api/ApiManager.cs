using io.github.vrchatapi.Api;
using io.github.vrchatapi.Client;
using io.github.vrchatapi.Model;
using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Concurrent;
using VRCEventUtil.Models.UserList;
using System.ComponentModel;
using VRCEventUtil.Converters;
using VRCEventUtil.Models.Setting;
using VRCEventUtil.Properties;
using static VRCEventUtil.Models.Api.ApiUtil;

namespace VRCEventUtil.Models.Api
{
    public class ApiManager
    {
        #region 定数
        //private const string API_BASE_ADDRESS = "https://api.vrchat.cloud/v1";
        private const string API_DOMAIN = "api.vrchat.cloud";
        #endregion 定数

        #region メンバ変数
        private readonly BlockingCollection<CountdownEvent> _apiCallRequests = new();
        private readonly CancellationTokenSource _tokenSource = new();
        private AuthenticationApi _authApi = new();
        private DateTime _lastApiCallTime;
        #endregion メンバ変数

        #region コンストラクタ・破棄処理
        private ApiManager()
        {
            _lastApiCallTime = DateTime.Now;

            var token = _tokenSource.Token;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_apiCallRequests.Any())
                    {
                        var interval = DateTime.Now - _lastApiCallTime;
                        Logger.Log($"スレッド{Environment.CurrentManagedThreadId}:前回のAPI呼び出しからの経過時間は{interval}です．");

                        TimeSpan MIN_INTERVAL = TimeSpan.FromSeconds(SettingManager.Settings.ApiCallIntervalSec);
                        if (interval < MIN_INTERVAL)
                        {
                            await Task.Delay(MIN_INTERVAL - interval);
                        }

                        _apiCallRequests.Take().Signal();
                        _lastApiCallTime = DateTime.Now;
                        Logger.Log($"スレッド{Environment.CurrentManagedThreadId}:{_lastApiCallTime}にAPI呼び出しを許可しました．");
                    }

                    await Task.Delay(1);
                }
            }, token);
        }

        ~ApiManager()
        {
            _tokenSource?.Cancel();
        }
        #endregion コンストラクタ・破棄処理

        #region プロパティ
        /// <summary>
        /// インスタンス
        /// </summary>
        public static ApiManager Instance => _instance ??= new ApiManager();
        private static ApiManager? _instance;

        /// <summary>
        /// 現在のユーザー
        /// </summary>
        public CurrentUser? CurrentUser { get; private set; }
        #endregion プロパティ

        #region イベント
        /// <summary>
        /// 
        /// </summary>
        public event Action<string>? ApiLog;
        #endregion イベント

        #region メソッド
        public async Task<bool> Login(string username, string? password, string? mfaCode = null)
        {
            Configuration.Default.Username = username;

            if (SettingManager.Settings.SaveAuthCookies && await LoadAuthCookies())
            {
                return true;
            }

            if (password is null) { return false; }

            AuthCookieManager.ClearCookies();
            Configuration.Default.Password = password;
            _authApi.Configuration.Username = username;
            _authApi.Configuration.Password = password;

            try
            {
                await _authApi.GetCurrentUserAsync();
                if (!string.IsNullOrWhiteSpace(mfaCode))
                {
                    await _authApi.Verify2FAAsync(new InlineObject(mfaCode));
                }
            }
            catch (ApiException ex)
            {
                Logger.Log(ex);
                return false;
            }

            // Cookieの保存
            if (SettingManager.Settings.SaveAuthCookies)
            {
                SaveAuthCookie();
            }

            CurrentUser = await _authApi.GetCurrentUserAsync();
            return true;
        }

        public void Logout()
        {
            _authApi?.Logout();
            AuthCookieManager.ClearCookies();
            Configuration.Default.Password = string.Empty;
        }

        /// <summary>
        /// Inviteを実行します．
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="users"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<bool> Invite(string locationId, IEnumerable<InviteUser> users, IProgress<double> progress, CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {
                var apiInstance = new InviteApi(_authApi.Configuration);
                var inviteRequest = new InviteRequest(locationId);

                // Invite実行
                int userCount = 0;
                foreach (var user in users)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested) { return false; }

                        // 状態をリセット
                        user.IsInInstance = false;
                        user.HasInvited = false;

                        // ユーザーのいるインスタンスを確認
                        if (!await UpdateUserStatus(user, locationId, cancellationToken) || !user.CanInvite)
                        {
                            continue;
                        }

                        if (cancellationToken.IsCancellationRequested) { return false; }
                        if (!await Invite(user.Id, apiInstance, inviteRequest, cancellationToken))
                        {
                            // TODO
                            return false;
                        }

                        ApiLog?.Invoke(string.Format(Resources.Success_InviteSentTo, user.Name));
                        user.HasInvited = true;
                    }
                    finally
                    {
                        progress.Report((double)++userCount / users.Count() * 100);
                    }
                }

                return true;
            }, cancellationToken);
        }

        /// <summary>
        /// ユーザー情報を取得します．
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<User?> GetUserInfo(string userId, CancellationToken cancellationToken = default)
        {
            var apiInstance = new UsersApi(_authApi.Configuration);

            try
            {
                WaitApiCallInterval(cancellationToken);
                return await apiInstance.GetUserAsync(userId, cancellationToken);
            }
            catch (ApiException ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        /// <summary>
        /// ユーザーの状態を更新します．
        /// </summary>
        /// <param name="user"></param>
        /// <param name="locationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserStatus(InviteUser user, string? locationId, CancellationToken cancellationToken = default)
        {
            var userInfo = await GetUserInfo(user.Id, cancellationToken);
            if (userInfo is null) { return false; }
            user.Name = userInfo.DisplayName;

            user.IsOnline = userInfo.State == UserState.Online;
            if (!user.IsOnline)
            {
                return true;
            }

            var loc = userInfo.Location;
            user.IsInInstance = loc == locationId;

            return true;
        }

        /// <summary>
        /// ワールド情報を取得します．
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        public World? GetWorldInfo(string worldId)
        {
            var apiInstance = new WorldsApi(_authApi.Configuration);

            try
            {
                WaitApiCallInterval();
                return apiInstance.GetWorld(worldId);
            }
            catch (ApiException ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        /// <summary>
        /// ワールドのインスタンスを作成します．
        /// </summary>
        /// <param name="worldId"></param>
        /// <param name="region"></param>
        /// <param name="disclosureRange"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<string?> CreateWorldInstance(string worldId, ERegion region = ERegion.JP,
            EDisclosureRange disclosureRange = EDisclosureRange.Invite, CancellationToken? cancellationToken = null)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    WaitApiCallInterval(cancellationToken);
                    var user = await new UsersApi(_authApi.Configuration).GetUserByNameAsync(_authApi.Configuration.Username);
                    var userId = user.Id;
                    var instanceId = CreateNewLocationId(worldId, userId, region, disclosureRange);

                    return instanceId;
                }
                catch (ApiException ex)
                {
                    Logger.Log(ex);
                    return null;
                }
            });
        }

        /// <summary>
        /// ワールドのインスタンスの情報を取得します．
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public Instance? GetWorldInstance(string locationId)
        {
            string worldId;
            string instanceId;

            try
            {
                (worldId, instanceId) = ResolveLocationIdOrUrl(locationId);
            }
            catch (FormatException ex)
            {
                Logger.Log(ex);
                return null;
            }

            return GetWorldInstance(worldId, instanceId);
        }

        /// <summary>
        /// ワールドのインスタンスの情報を取得します．
        /// </summary>
        /// <param name="worldId"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public Instance? GetWorldInstance(string worldId, string instanceId)
        {
            var apiInstance = new WorldsApi(_authApi.Configuration);

            try
            {
                WaitApiCallInterval();
                return apiInstance.GetWorldInstance(worldId, instanceId);
            }
            catch (ApiException ex)
            {
                Logger.Log(ex);
                return null;
            }
        }
        #endregion メソッド

        #region 内部関数
        /// <summary>
        /// API呼び出し間隔制限を守るように処理を待ちます．
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <exception cref="OperationCanceledException"></exception>
        private void WaitApiCallInterval(CancellationToken? cancellationToken = null)
        {
            var e = new CountdownEvent(1);
            _apiCallRequests.Add(e);
            if (cancellationToken is null)
            {
                e.Wait();
            }
            else
            {
                e.Wait(cancellationToken.Value);
            }
        }

        /// <summary>
        /// 保存された認証Cookieを読み込んで，接続を試行します．
        /// </summary>
        /// <returns>接続に成功すればtrue</returns>
        private async Task<bool> LoadAuthCookies()
        {
            _authApi = new AuthenticationApi();

            var authCookieVal = AuthCookieManager.AuthCookie;
            var mfaCookieVal = AuthCookieManager.MFACookie;
            if (string.IsNullOrWhiteSpace(authCookieVal))
            {
                return false;
            }

            try
            {
                var authCookie = new Cookie("auth", authCookieVal, "/", API_DOMAIN);
                _authApi.Configuration.ApiClient.RestClient.CookieContainer.Add(authCookie);

                if (!string.IsNullOrWhiteSpace(mfaCookieVal))
                {
                    var mfaCookie = new Cookie("twoFactorAuth", mfaCookieVal, "/", API_DOMAIN);
                    _authApi.Configuration.ApiClient.RestClient.CookieContainer.Add(mfaCookie);
                }

                CurrentUser = await _authApi.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 認証用Cookieを保存します．
        /// </summary>
        private void SaveAuthCookie()
        {
            var cookies = _authApi.Configuration.ApiClient.RestClient.CookieContainer.GetCookies(new Uri("https://api.vrchat.cloud"));
            var authCookie = cookies["auth"];
            var mfaCookie = cookies["twoFactorAuth"];

            AuthCookieManager.AuthCookie = authCookie?.Value ?? string.Empty;
            AuthCookieManager.MFACookie = mfaCookie?.Value ?? string.Empty;
        }

        /// <summary>
        /// Inviteを送信します．
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="apiInstance"></param>
        /// <param name="inviteRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private async Task<bool> Invite(string userId, InviteApi apiInstance, InviteRequest inviteRequest, CancellationToken cancellationToken)
        {
            try
            {
                WaitApiCallInterval(cancellationToken);
                var result = await apiInstance.InviteUserAsync(userId, inviteRequest, cancellationToken);
                return true;
            }
            catch (ApiException ex) when (ex.ErrorCode == 500)  // WARNING:Inviteが成功してもレスポンスの解析で500番エラーが発生するのでログだけ出して無視する
            {
                Logger.Log(ex);
                return true;
            }
            catch (ApiException ex)
            {
                Logger.Log(ex);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                throw new OperationCanceledException("Inviteの送信をキャンセルしました.", ex, cancellationToken);    // キャンセルによる例外はOperationCanceledExceptionにまとめる
            }
        }
        #endregion 内部関数
    }

    /// <summary>
    /// インスタンスのサーバー地域
    /// </summary>
    public enum ERegion
    {
        US,
        EU,
        JP,
    }

    /// <summary>
    /// インスタンスの公開範囲
    /// </summary>
    [TypeConverter(typeof(EnumDisplayNameConverter))]
    public enum EDisclosureRange
    {
        [EnumDisplayName("Public")]
        Public,
        [EnumDisplayName("Friends+")]
        FriendsPlus,
        [EnumDisplayName("Friends Only")]
        Friends,
        [EnumDisplayName("Invite+")]
        InvitePlus,
        [EnumDisplayName("Invite Only")]
        Invite,
    }
}
