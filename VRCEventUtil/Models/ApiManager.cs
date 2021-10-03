using io.github.vrchatapi.Api;
using io.github.vrchatapi.Client;
using io.github.vrchatapi.Model;
using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCEventUtil.Properties;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Concurrent;

namespace VRCEventUtil.Models
{
    public class ApiManager
    {
        #region 定数
        //private const string API_BASE_ADDRESS = "https://api.vrchat.cloud/v1";
        private const string API_DOMAIN = "api.vrchat.cloud";
        #endregion 定数

        #region コンストラクタ
        private ApiManager()
        {
            _lastApiCallTime = DateTime.Now;

            TimeSpan MIN_INTERVAL = TimeSpan.FromSeconds(5);
            var token = _tokenSource.Token;
            _apiCallRequestTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_apiCallRequests.Any())
                    {
                        var interval = DateTime.Now - _lastApiCallTime;
                        Logger.Log($"スレッド{Thread.CurrentThread.ManagedThreadId}:前回のAPI呼び出しからの経過時間は{interval}です．");

                        if (interval < MIN_INTERVAL)
                        {
                            await Task.Delay(MIN_INTERVAL - interval);
                        }

                        _apiCallRequests.Take().Signal();
                        _lastApiCallTime = DateTime.Now;
                        Logger.Log($"スレッド{Thread.CurrentThread.ManagedThreadId}:{_lastApiCallTime}にAPI呼び出しを許可しました．");
                    }

                    await Task.Delay(1);
                }
            }, token);
        }
        #endregion コンストラクタ

        #region メンバ変数
        private AuthenticationApi _authApi;
        private DateTime _lastApiCallTime;
        private readonly BlockingCollection<CountdownEvent> _apiCallRequests = new BlockingCollection<CountdownEvent>();
        private readonly Task _apiCallRequestTask;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        #endregion メンバ変数

        #region プロパティ
        /// <summary>
        /// インスタンス
        /// </summary>
        public static ApiManager Instance => _instance ??= new ApiManager();
        private static ApiManager _instance;

        /// <summary>
        /// 現在のユーザー
        /// </summary>
        public CurrentUser CurrentUser { get; private set; }
        #endregion プロパティ

        #region メソッド
        /// <summary>
        /// 保存された認証Cookieを読み込んで，接続を試行します．
        /// </summary>
        /// <returns>接続に成功すればtrue</returns>
        public async Task<bool> LoadAuthCookies()
        {
            _authApi = new AuthenticationApi();

            try
            {
                var authCookie = new Cookie("auth", Settings.Default.AuthCookie, "/", API_DOMAIN);
                var mfaCookie = new Cookie("twoFactorAuth", Settings.Default.MFACookie, "/", API_DOMAIN);
                _authApi.Configuration.ApiClient.RestClient.CookieContainer.Add(authCookie);
                _authApi.Configuration.ApiClient.RestClient.CookieContainer.Add(mfaCookie);
                CurrentUser = await _authApi.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

            return true;
        }

        public async Task<bool> Login(string username, string password, string mfaCode = null)
        {
            if (await LoadAuthCookies())
            {
                return true;
            }

            Configuration.Default.Username = username;
            Configuration.Default.Password = password;
            _authApi = new AuthenticationApi();

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
            SaveAuthCookie();

            CurrentUser = await _authApi.GetCurrentUserAsync();
            return true;
        }

        /// <summary>
        /// Inviteを実行します．
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async Task<bool> Invite(string instanceId, IEnumerable<string> userIds, IProgress<string> progress)
        {
            return await Task.Run(async () =>
            {
                // インスタンスID解析
                string formattedInstanceId;
                try
                {
                    formattedInstanceId = ApiUtil.ConvertToLocationId(instanceId);
                }
                catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
                {
                    Logger.Log(ex);
                    return false;
                }

                var apiInstance = new InviteApi(_authApi.Configuration);
                var inviteRequest = new InviteRequest(formattedInstanceId);

                // Invite実行
                for (int i = 0; i < userIds.Count(); i++)
                {
                    var userId = userIds.ElementAt(i);
                    if (!await Invite(userId, apiInstance, inviteRequest))
                    {
                        // TODO
                        return false;
                    }
                    progress.Report($"{userId}にInviteを送信しました．");
                }

                return true;
            });
        }

        public User GetUserInfo(string userId)
        {
            var apiInstance = new UsersApi(Configuration.Default);

            try
            {
                WaitApiCallInterval();
                return apiInstance.GetUser(userId);
            }
            catch (ApiException ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        public World GetWorldInfo(string worldId)
        {
            var apiInstance = new WorldsApi(Configuration.Default);

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

        public async Task<string> CreateWorldInstance(string worldId)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    WaitApiCallInterval();
                    var user = await new UsersApi(Configuration.Default).GetUserByNameAsync(Configuration.Default.Username);
                    var userId = user.Id;
                    var instanceId = CreateNewLocationId(worldId, userId, ERegion.JP, EDisclosureRange.FriendPlus);

                    return instanceId;
                }
                catch (ApiException ex)
                {
                    Logger.Log(ex);
                    return null;
                }
            });
        }

        public Instance GetWorldInstance(string locationId)
        {
            string worldId;
            string instanceId;

            try
            {
                (worldId, instanceId) = ApiUtil.ParseLocationId(locationId);
            }
            catch (FormatException ex)
            {
                Logger.Log(ex);
                return null;
            }

            return GetWorldInstance(worldId, instanceId);
        }

        public Instance GetWorldInstance(string worldId, string instanceId)
        {
            var apiInstance = new WorldsApi();

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
        private void WaitApiCallInterval()
        {
            var e = new CountdownEvent(1);
            _apiCallRequests.Add(e);
            e.Wait();
        }

        /// <summary>
        /// 認証用Cookieを保存します．
        /// </summary>
        private void SaveAuthCookie()
        {
            var cookies = _authApi.Configuration.ApiClient.RestClient.CookieContainer.GetCookies(new Uri("https://api.vrchat.cloud"));
            var authCookie = cookies["auth"];
            var mfaCookie = cookies["twoFactorAuth"];

            Settings.Default.AuthCookie = authCookie.Value;
            Settings.Default.MFACookie = mfaCookie.Value;
            Settings.Default.Save();
        }

        private string CreateNewLocationId(string worldId, string userId, ERegion region, EDisclosureRange disclosureRange)
        {
            // :(\d+)(~region\(([\w]+)\))?(~([\w]+)\(usr_([\w-]+)\)((\~canRequestInvite)?)(~region\(([\w].+)\))?~nonce\((.+)\))?
            var random = new Random();
            var instanceId = random.Next(10000, 99999);
            string disclosureRangeStr = string.Empty;
            switch (disclosureRange)
            {
                case EDisclosureRange.FriendPlus:
                    disclosureRangeStr = $"~hidden({userId})";
                    break;
                case EDisclosureRange.Friend:
                    disclosureRangeStr = $"~friends({userId})";
                    break;
                case EDisclosureRange.InvitePlus:
                    disclosureRangeStr = $"~private({userId})~canRequestInvite";
                    break;
                case EDisclosureRange.Invite:
                    disclosureRangeStr = $"~private({userId})";
                    break;
            }

            string regionStr = $"~region({region.ToString().ToLower()})";
            string nonce = $"~nonce({Guid.NewGuid()})";

            var location = $"{worldId}:{instanceId}{disclosureRangeStr}{regionStr}{nonce}";

            return location;
        }

        private async Task<bool> Invite(string userId, InviteApi apiInstance, InviteRequest inviteRequest)
        {
            try
            {
                WaitApiCallInterval();
                var result = await apiInstance.InviteUserAsync(userId, inviteRequest);
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
        }
        #endregion 内部関数
    }

    public enum ERegion
    {
        US,
        EU,
        JP,
    }

    public enum EDisclosureRange
    {
        Public,
        FriendPlus,
        Friend,
        InvitePlus,
        Invite,
    }
}
