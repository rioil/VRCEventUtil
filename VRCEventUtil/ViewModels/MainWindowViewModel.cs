using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VRCEventUtil.Models;
using System.Threading.Tasks;
using VRCEventUtil.Properties;
using VRCEventUtil.Models.UserList;
using System.Windows;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using VRCEventUtil.Models.Api;
using VRCEventUtil.Models.Setting;

namespace VRCEventUtil.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        // Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
        public async void Initialize()
        {
            IsLoading = true;
            if (!SettingManager.LoadSetting())
            {
                SettingManager.CreateDefaultSetting();
            }

            Username = Settings.Default.Username;
            ApiManager.Instance.ApiLog += msg => DispatcherHelper.UIDispatcher.Invoke(() => Log(msg));
            if (SettingManager.Settings.SaveAuthCookies)
            {
                IsLoggedIn = await Task.Run(() => ApiManager.Instance.Login(Username, null));
                if (IsLoggedIn)
                {
                    Log(string.Format(Resources.Success_AutoLogIn, Username));
                }
                else
                {
                    Log(Resources.Fail_AutoLogin);
                }
            }
            IsLoading = false;
        }

        #region メンバ変数
        /// <summary>
        /// Invite中断用キャンセルトークンソース
        /// </summary>
        CancellationTokenSource _inviteCancellationTokenSrc;
        #endregion メンバ変数

        #region プロパティ
        /// <summary>
        /// インスタンス管理
        /// </summary>
        public WorldInstanceManager WorldInstanceManager { get; } = new WorldInstanceManager();

        /// <summary>
        /// インスタンス公開範囲列挙値リスト
        /// </summary>
        public List<EDisclosureRange> DisclosureRanges { get; } = Enum.GetValues(typeof(EDisclosureRange)).Cast<EDisclosureRange>().ToList();
        #endregion プロパティ

        #region 変更通知プロパティ
        /// <summary>
        /// ユーザー名
        /// </summary>
        public string Username
        {
            get => _username;
            set => RaisePropertyChangedIfSet(ref _username, value);
        }
        private string _username;

        /// <summary>
        /// パスワード
        /// </summary>
        public string Password
        {
            get => _password;
            set => RaisePropertyChangedIfSet(ref _password, value);
        }
        private string _password;

        /// <summary>
        /// 二要素認証コード
        /// </summary>
        public string MFACode
        {
            get => _MFACode;
            set => RaisePropertyChangedIfSet(ref _MFACode, value);
        }
        private string _MFACode;

        /// <summary>
        /// ロケーションID/URL
        /// </summary>
        public string LocationIdOrUrl
        {
            get => _locationIdOrUrl;
            set
            {
                if (RaisePropertyChangedIfSet(ref _locationIdOrUrl, value))
                {
                    InviteCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _locationIdOrUrl;

        /// <summary>
        /// ユーザーID
        /// </summary>
        public string UserId
        {
            get => _userId;
            set => RaisePropertyChangedIfSet(ref _userId, value);
        }
        private string _userId;

        /// <summary>
        /// ワールドID/URL
        /// </summary>
        public string WorldIdOrUrl
        {
            get => _worldIdOrUrl;
            set
            {
                if (RaisePropertyChangedIfSet(ref _worldIdOrUrl, value))
                {
                    CreateWorldInstanceCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _worldIdOrUrl;

        /// <summary>
        /// ログ
        /// </summary>
        public ObservableCollection<UserLog> Logs
        {
            get => _logs;
            set => RaisePropertyChangedIfSet(ref _logs, value);
        }
        private ObservableCollection<UserLog> _logs = new ObservableCollection<UserLog>();

        /// <summary>
        /// ログイン済みか
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => RaisePropertyChangedIfSet(ref _isLoggedIn, value);
        }
        private bool _isLoggedIn;

        /// <summary>
        /// 読み込み中か
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => RaisePropertyChangedIfSet(ref _isLoading, value);
        }
        private bool _isLoading = true;

        /// <summary>
        /// ユーザーリストファイルパス
        /// </summary>
        public string UserListFilePath
        {
            get => _userListFilePath;
            set
            {
                if (RaisePropertyChangedIfSet(ref _userListFilePath, value))
                {
                    Task.Run(() =>
                    {
                        var inviteUsers = UserListReader.ReadInviteUser(value);
                        DispatcherHelper.UIDispatcher.Invoke(() =>
                        {
                            Users = new ObservableCollection<InviteUser>(inviteUsers);
                        });
                    });
                }
            }
        }
        private string _userListFilePath;

        /// <summary>
        /// ユーザーリスト
        /// </summary>
        public ObservableCollection<InviteUser> Users
        {
            get => _users;
            set
            {
                if (RaisePropertyChangedIfSet(ref _users, value))
                {
                    InviteCommand.RaiseCanExecuteChanged();
                    _users.CollectionChanged += (sender, args) => InviteCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private ObservableCollection<InviteUser> _users;

        /// <summary>
        /// インスタンスの公開範囲
        /// </summary>
        public EDisclosureRange InstanceDisclosureRange
        {
            get => _instanceDisclosureRange;
            set => RaisePropertyChangedIfSet(ref _instanceDisclosureRange, value);
        }
        private EDisclosureRange _instanceDisclosureRange = EDisclosureRange.Invite;

        /// <summary>
        /// インスタンスのサーバー地域
        /// </summary>
        public ERegion InstanceRegion
        {
            get => _instanceRegion;
            set => RaisePropertyChangedIfSet(ref _instanceRegion, value);
        }
        private ERegion _instanceRegion = ERegion.JP;

        /// <summary>
        /// Invite実行中か
        /// </summary>
        public bool IsInviting
        {
            get => _isInviting;
            set
            {
                if (RaisePropertyChangedIfSet(ref _isInviting, value))
                {
                    InviteCommand.RaiseCanExecuteChanged();
                    AbortInviteCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool _isInviting;

        /// <summary>
        /// Inviteをキャンセル中か
        /// </summary>
        public bool IsInviteAborting
        {
            get => _IsInviteCancelling;
            set
            {
                if (RaisePropertyChangedIfSet(ref _IsInviteCancelling, value))
                {
                    AbortInviteCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool _IsInviteCancelling;

        /// <summary>
        /// Inviteの進捗度
        /// </summary>
        public int InviteProgress
        {
            get => _inviteProgress;
            set => RaisePropertyChangedIfSet(ref _inviteProgress, value);
        }
        private int _inviteProgress;
        #endregion 変更通知プロパティ

        #region コマンド
        /// <summary>
        /// ログインを行います．
        /// </summary>
        public async void Login()
        {
            if (await ApiManager.Instance.Login(Username, Password, MFACode))
            {
                Log(string.Format(Resources.Success_Login, Username));
                Settings.Default.Username = Username;
                Settings.Default.Save();
                IsLoggedIn = true;
            }
            else
            {
                Messenger.Raise(new InformationMessage(Resources.Fail_LogIn,
                    Resources.Title_Error, MessageBoxImage.Warning, "InformationMessage"));
            }
        }
        private ViewModelCommand _loginCommand;
        public ViewModelCommand LoginCommand => _loginCommand ??= new ViewModelCommand(Login);

        /// <summary>
        /// ログアウトを行います．
        /// </summary>
        public void Logout()
        {
            IsLoggedIn = false;
            ApiManager.Instance.Logout();
            Log(Resources.Success_Logout);
        }
        private ViewModelCommand _logoutCommand;
        public ViewModelCommand LogoutCommand => _logoutCommand ??= new ViewModelCommand(Logout);

        /// <summary>
        /// Invite送信処理を行います．
        /// </summary>
        public async void Invite()
        {
            _inviteCancellationTokenSrc = new CancellationTokenSource();

            InviteProgress = 0;
            IsInviteAborting = false;
            IsInviting = true;

            var progress = new Progress<double>();
            progress.ProgressChanged += (_, val) => DispatcherHelper.UIDispatcher.Invoke(() => InviteProgress = (int)val);
            try
            {
                var result = await ApiManager.Instance.Invite(LocationIdOrUrl, Users, progress, _inviteCancellationTokenSrc.Token);
                if (result)
                {
                    Log(Resources.Success_Invite);
                }
                else
                {
                    Log(Resources.Fail_Invite);
                }
            }
            catch (TaskCanceledException)
            {
                Log(Resources.Abort_Invite);
            }

            IsInviting = false;
        }

        public bool CanInvite() => !IsInviting && !string.IsNullOrWhiteSpace(LocationIdOrUrl) && Users is object && Users.Any();
        private ViewModelCommand _inviteCommand;
        public ViewModelCommand InviteCommand => _inviteCommand ??= new ViewModelCommand(Invite, CanInvite);

        /// <summary>
        /// Invite中断処理を行います．
        /// </summary>
        public void AbortInvite()
        {
            IsInviteAborting = true;
            _inviteCancellationTokenSrc.Cancel();
            Log(Resources.Aborting_Invite);
        }
        private bool CanAbortInvite() => IsInviting && !IsInviteAborting;
        private ViewModelCommand _abortInviteCommand;

        public ViewModelCommand AbortInviteCommand => _abortInviteCommand ??= new ViewModelCommand(AbortInvite, CanAbortInvite);

        /// <summary>
        /// ワールドインスタンス作成処理を行います．
        /// </summary>
        public async void CreateWorldInstance()
        {
            try
            {
                LocationIdOrUrl = await ApiManager.Instance.CreateWorldInstance(WorldIdOrUrl, InstanceRegion, InstanceDisclosureRange);
            }
            catch (FormatException)
            {
                Log(Resources.Error_InvalidWorldIdOrUrl);
                Messenger.Raise(new InformationMessage(Resources.Error_InvalidWorldIdOrUrl, Resources.Title_Error,
                    MessageBoxImage.Warning, "InformationMessage"));
                return;
            }

            var instanceId = ApiUtil.ResolveLocationIdOrUrl(LocationIdOrUrl).InstanceId;
            Log(string.Format(Resources.Success_CreateInstance, InstanceRegion, InstanceDisclosureRange, WorldIdOrUrl, instanceId));
        }
        public bool CanCreateWorldInstance() => !string.IsNullOrWhiteSpace(WorldIdOrUrl);
        public ViewModelCommand CreateWorldInstanceCommand => _createWorldInstanceCommand ??= new ViewModelCommand(CreateWorldInstance, CanCreateWorldInstance);
        private ViewModelCommand _createWorldInstanceCommand;

        /// <summary>
        /// ユーザーリストファイル選択ダイアログを表示します．
        /// </summary>
        public void SelectUserListFile()
        {
            var dialog = new OpeningFileSelectionMessage("OpenFileDialog")
            {
                Title = Resources.Title_SelectUserListFile,
                Filter = Resources.FileFilter_CSV
            };
            Messenger.Raise(dialog);

            if (dialog.Response is object && dialog.Response.Any())
            {
                UserListFilePath = dialog.Response[0];
            }
        }
        private ViewModelCommand _selectUserListFileCommand;
        public ViewModelCommand SelectUserListFileCommand => _selectUserListFileCommand ??= new ViewModelCommand(SelectUserListFile);

        /// <summary>
        /// ログをクリアします．
        /// </summary>
        public void ClaerLog()
        {
            Logs.Clear();
        }
        private ViewModelCommand _claerLogCommand;
        public ViewModelCommand ClaerLogCommand => _claerLogCommand ??= new ViewModelCommand(ClaerLog);



        public void OpenInVRChat()
        {
            if (!ApiUtil.TryParseLocationIdOrUrl(LocationIdOrUrl, out var locationId))
            {
                Log(Resources.Error_InvalidLocationIdOrUrl);
                Messenger.Raise(new InformationMessage(Resources.Error_InvalidLocationIdOrUrl, Resources.Title_Error,
                    MessageBoxImage.Warning, "InformationMessage"));
                return;
            }

            if (string.IsNullOrWhiteSpace(SettingManager.Settings.SteamExePath))
            {
                Log(Resources.Error_EmptySteamExePath);
                Messenger.Raise(new InformationMessage(Resources.Error_EmptySteamExePath, Resources.Title_Error,
                    MessageBoxImage.Warning, "InformationMessage"));
                return;
            }

            //var processInfo = new ProcessStartInfo("cmd.exe",
            //    $"/c start=vrchat://launch?id={locationId}{(Settings.Default.UseVRMode ? string.Empty : " --no-vr")}")
            var processInfo = new ProcessStartInfo(SettingManager.Settings.SteamExePath,
                $"-applaunch 438100 vrchat://launch?id={locationId}{(SettingManager.Settings.UseVRMode ? string.Empty : " --no-vr")}")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Log(Resources.Fail_LauchVRChat);
            }
        }
        private ViewModelCommand _openInVRChatCommand;
        public ViewModelCommand OpenInVRChatCommand => _openInVRChatCommand ??= new ViewModelCommand(OpenInVRChat);

        /// <summary>
        /// インスタンスURLをクリップボードにコピーします．
        /// </summary>
        public void CopyInstanceLink()
        {
            if (ApiUtil.TryResolveLocationIdOrUrl(LocationIdOrUrl, out var worldId, out var instanceId))
            {
                try
                {
                    Clipboard.SetText($"https://vrchat.com/home/launch?worldId={worldId}&instanceId={instanceId}");
                }
                catch (System.Runtime.InteropServices.COMException ex) when (ex.ErrorCode == -2147221040)
                {
                    // クリップボード例外は握りつぶす
                }
            }
        }
        private ViewModelCommand _copyInstanceLinkCommand;
        public ViewModelCommand CopyInstanceLinkCommand => _copyInstanceLinkCommand ??= new ViewModelCommand(CopyInstanceLink);

        #region メニュー
        /// <summary>
        /// 設定画面を開きます．
        /// </summary>
        public void OpenSettingWindow()
        {
            Messenger.Raise(new TransitionMessage(typeof(SettingWindow), new SettingWindowViewModel(), TransitionMode.Modal, "WindowInteraction"));
        }
        private ViewModelCommand _openSettingWindowCommand;
        public ViewModelCommand OpenSettingWindowCommand => _openSettingWindowCommand ??= new ViewModelCommand(OpenSettingWindow);

        #endregion メニュー
        #endregion コマンド

        #region メソッド
        #endregion メソッド

        #region 内部関数
        private void Log(string msg)
        {
            Logs.Add(new UserLog(msg));
        }

        private void Log(object msg)
        {
            Log(msg?.ToString());
        }
        #endregion 内部関数
    }
}
