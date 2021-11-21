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
using System.Collections.Specialized;
using VRCEventUtil.Views;
using System.IO;
using VRCEventUtil.Models.Util;

namespace VRCEventUtil.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        // Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
        public async void Initialize()
        {
            IsLoading = true;
            SettingManager.LoadSetting();

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

            var checker = new UpdateChecker();
            if (await checker.Check())
            {
                ShowInformation($"更新があります．\n現在のバージョン：{checker.Current}\n新しいバージョン：{checker.Latest}");
            }
        }

        #region メンバ変数
        /// <summary>
        /// Invite中断用キャンセルトークンソース
        /// </summary>
        CancellationTokenSource? _inviteCancellationTokenSrc;
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
        private string _username = string.Empty;

        /// <summary>
        /// パスワード
        /// </summary>
        public string? Password
        {
            get => _password;
            set => RaisePropertyChangedIfSet(ref _password, value);
        }
        private string? _password;

        /// <summary>
        /// 二要素認証コード
        /// </summary>
        public string? MFACode
        {
            get => _MFACode;
            set => RaisePropertyChangedIfSet(ref _MFACode, value);
        }
        private string? _MFACode;

        /// <summary>
        /// ロケーションID/URL
        /// </summary>
        public string? LocationIdOrUrl
        {
            get => _locationIdOrUrl;
            set
            {
                if (RaisePropertyChangedIfSet(ref _locationIdOrUrl, value))
                {
                    InviteCommandRaiseCanExecuteChanged();
                }
            }
        }
        private string? _locationIdOrUrl;

        /// <summary>
        /// ユーザーID
        /// </summary>
        public string? UserId
        {
            get => _userId;
            set => RaisePropertyChangedIfSet(ref _userId, value);
        }
        private string? _userId;

        /// <summary>
        /// ワールドID/URL
        /// </summary>
        public string? WorldIdOrUrl
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
        private string? _worldIdOrUrl;

        /// <summary>
        /// ログ
        /// </summary>
        public ObservableCollection<UserLog> Logs
        {
            get => _logs;
            set => RaisePropertyChangedIfSet(ref _logs, value);
        }
        private ObservableCollection<UserLog> _logs = new();

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
        public string? UserListFilePath
        {
            get => _userListFilePath;
            set => RaisePropertyChangedIfSet(ref _userListFilePath, value);
        }
        private string? _userListFilePath;

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
                    InviteCommandRaiseCanExecuteChanged();
                    UpdateCommandRaiseCanExecuteChanged();
                    UpdateGruop();

                    _users.CollectionChanged += (sender, args) =>
                    {
                        switch (args.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                if (args.NewItems is null) { break; }
                                foreach (var user in args.NewItems.Cast<InviteUser>())
                                {
                                    var group = Groups.FirstOrDefault(g => g.GroupName == user.GroupName);
                                    if (group is null)
                                    {
                                        group = new UserGroup(user.GroupName, user);
                                        Groups.Add(group);
                                    }
                                    else
                                    {
                                        group.Users.Add(user);
                                    }
                                }
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                if (args.OldItems is null) { break; }
                                foreach (var user in args.OldItems.Cast<InviteUser>())
                                {
                                    var group = Groups.FirstOrDefault(g => g.GroupName == user.GroupName);
                                    if (group is not null)
                                    {
                                        group.Users.Remove(user);
                                    }
                                }
                                break;
                            case NotifyCollectionChangedAction.Reset:
                                UpdateGruop();
                                break;
                            case NotifyCollectionChangedAction.Replace:
                                var oldUser = args.OldItems?.Cast<InviteUser>().FirstOrDefault();
                                var newUser = args.NewItems?.Cast<InviteUser>().FirstOrDefault();
                                if (oldUser is null || newUser is null || oldUser == newUser) { break; }
                                Groups.FirstOrDefault(g => g.GroupName == oldUser.GroupName)?.Users.Remove(oldUser);
                                Groups.FirstOrDefault(g => g.GroupName == newUser.GroupName)?.Users.Add(newUser);
                                break;
                        }

                        InviteCommandRaiseCanExecuteChanged();
                        UpdateCommandRaiseCanExecuteChanged();
                    };
                }
            }
        }
        private ObservableCollection<InviteUser> _users = new();

        /// <summary>
        /// グループリスト
        /// </summary>
        public ObservableCollection<UserGroup> Groups
        {
            get => _groups;
            set => RaisePropertyChangedIfSet(ref _groups, value);
        }
        private ObservableCollection<UserGroup> _groups = new();

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
                    InviteCommandRaiseCanExecuteChanged();
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
                ShowWarning(Resources.Fail_LogIn);
            }

            Password = string.Empty;
            MFACode = string.Empty;
        }
        private ViewModelCommand? _loginCommand;
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
        private ViewModelCommand? _logoutCommand;
        public ViewModelCommand LogoutCommand => _logoutCommand ??= new ViewModelCommand(Logout);

        /// <summary>
        /// Invite送信処理を行います．
        /// </summary>
        public async void Invite(IEnumerable<InviteUser>? users)
        {
            if (users is null) { return; }

            _inviteCancellationTokenSrc = new CancellationTokenSource();

            InviteProgress = 0;
            IsInviteAborting = false;
            IsInviting = true;

            var progress = new Progress<double>();
            progress.ProgressChanged += (_, val) => DispatcherHelper.UIDispatcher.Invoke(() => InviteProgress = (int)val);

            if (!ApiUtil.TryParseLocationIdOrUrl(LocationIdOrUrl, out var locationId))
            {
                Log(Resources.Fail_Invite);
                return;
            }

            try
            {
                var result = await ApiManager.Instance.Invite(locationId, users, progress, _inviteCancellationTokenSrc.Token);
                if (result)
                {
                    Log(Resources.Success_Invite);
                }
                else
                {
                    Log(Resources.Fail_Invite);
                }
            }
            catch (OperationCanceledException)
            {
                Log(Resources.Abort_Invite);
            }

            IsInviting = false;
        }

        public void Invite(InviteUser user)
        {
            Invite(new InviteUser[] { user });
        }

        public bool CanInvite() => !IsInviting && !string.IsNullOrWhiteSpace(LocationIdOrUrl) && Users is not null && Users.Any();

        private ViewModelCommand? _inviteAllCommand;
        public ViewModelCommand InviteAllCommand => _inviteAllCommand ??= new ViewModelCommand(() => Invite(Users), CanInvite);

        private ListenerCommand<UserGroup>? _inviteGroupCommand;
        public ListenerCommand<UserGroup> InviteGroupCommand => _inviteGroupCommand ??= new ListenerCommand<UserGroup>(group => Invite(group.Users), CanInvite);

        private ListenerCommand<InviteUser>? _inviteUserCommand;
        public ListenerCommand<InviteUser> InviteUserCommand => _inviteUserCommand ??= new ListenerCommand<InviteUser>(user => Invite(user), CanInvite);

        /// <summary>
        /// Invite中断処理を行います．
        /// </summary>
        public void AbortInvite()
        {
            IsInviteAborting = true;
            _inviteCancellationTokenSrc?.Cancel();
            Log(Resources.Aborting_Invite);
        }
        private bool CanAbortInvite() => IsInviting && !IsInviteAborting;
        private ViewModelCommand? _abortInviteCommand;
        public ViewModelCommand AbortInviteCommand => _abortInviteCommand ??= new ViewModelCommand(AbortInvite, CanAbortInvite);

        #region 情報更新コマンド
        public async void Update(InviteUser user)
        {
            if (!ApiUtil.TryParseLocationIdOrUrl(LocationIdOrUrl, out var locationId))
            {
                return;
            }

            await ApiManager.Instance.UpdateUserStatus(user, locationId);
        }

        public async void Update(UserGroup group)
        {
            if (!ApiUtil.TryParseLocationIdOrUrl(LocationIdOrUrl, out var locationId))
            {
                return;
            }

            foreach (var user in group.Users)
            {
                await ApiManager.Instance.UpdateUserStatus(user, locationId);
            }
        }

        public async void Update()
        {
            if (!ApiUtil.TryParseLocationIdOrUrl(LocationIdOrUrl, out var locationId))
            {
                return;
            }

            foreach (var user in Users)
            {
                await ApiManager.Instance.UpdateUserStatus(user, locationId);
            }
        }

        public bool CanUpdate() => Users is not null && Users.Any();  // TODO

        private ViewModelCommand? _updateAllCommand;
        public ViewModelCommand UpdateAllCommand => _updateAllCommand ??= new ViewModelCommand(Update, CanUpdate);

        private ListenerCommand<UserGroup>? _updateGroupCommand;
        public ListenerCommand<UserGroup> UpdateGroupCommand => _updateGroupCommand ??= new ListenerCommand<UserGroup>(Update, CanUpdate);

        private ListenerCommand<InviteUser>? _updateUserCommand;
        public ListenerCommand<InviteUser> UpdateUserCommand => _updateUserCommand ??= new ListenerCommand<InviteUser>(Update, CanUpdate);
        #endregion 情報更新コマンド

        /// <summary>
        /// ワールドインスタンス作成処理を行います．
        /// </summary>
        public async void CreateWorldInstance()
        {
            if (!ApiUtil.TryParseWorldId(WorldIdOrUrl, out var worldId))
            {
                Log(Resources.Error_InvalidWorldIdOrUrl);
                ShowWarning(Resources.Error_InvalidWorldIdOrUrl);
                return;
            }

            try
            {
                LocationIdOrUrl = await ApiManager.Instance.CreateWorldInstance(worldId, InstanceRegion, InstanceDisclosureRange);
            }
            catch (FormatException)
            {
                Log(Resources.Error_InvalidWorldIdOrUrl);
                ShowWarning(Resources.Error_InvalidWorldIdOrUrl);
                return;
            }
            catch (OperationCanceledException)
            {
                Log(Resources.Abort_CreateInstance);
                return;
            }

            if (LocationIdOrUrl is null)
            {
                Log(Resources.Fail_CreateInstance);
                return;
            }

            var instanceId = ApiUtil.ResolveLocationIdOrUrl(LocationIdOrUrl).InstanceId;
            Log(string.Format(Resources.Success_CreateInstance, InstanceRegion, InstanceDisclosureRange, WorldIdOrUrl, instanceId));
        }
        public bool CanCreateWorldInstance() => !string.IsNullOrWhiteSpace(WorldIdOrUrl);
        public ViewModelCommand CreateWorldInstanceCommand => _createWorldInstanceCommand ??= new ViewModelCommand(CreateWorldInstance, CanCreateWorldInstance);
        private ViewModelCommand? _createWorldInstanceCommand;

        /// <summary>
        /// ユーザーリストファイル選択ダイアログを表示します．
        /// </summary>
        public void SelectUserListFile()
        {
            var dialog = new OpeningFileSelectionMessage("OpenFileDialog")
            {
                Title = Resources.Title_SelectUserListFile,
                Filter = Resources.FileFilter_JSON,
                InitialDirectory = Path.GetDirectoryName(UserListFilePath),
                FileName = Path.GetFileName(UserListFilePath)
            };
            Messenger.Raise(dialog);

            if (dialog.Response is not null && dialog.Response.Any())
            {
                UserListFilePath = dialog.Response[0];

                if (UserListFilePath is not null)
                {
                    try
                    {
                        var inviteUsers = UserListReader.ReadInviteUser(UserListFilePath);
                        Users = new ObservableCollection<InviteUser>(inviteUsers);
                    }
                    catch (Exception ex)
                    {
                        ShowWarning(ex.Message);
                    }
                }
            }
        }
        private ViewModelCommand? _selectUserListFileCommand;
        public ViewModelCommand SelectUserListFileCommand => _selectUserListFileCommand ??= new ViewModelCommand(SelectUserListFile);

        /// <summary>
        /// ログをクリアします．
        /// </summary>
        public void ClaerLog()
        {
            Logs.Clear();
        }
        private ViewModelCommand? _claerLogCommand;
        public ViewModelCommand ClaerLogCommand => _claerLogCommand ??= new ViewModelCommand(ClaerLog);

        public void OpenInVRChat()
        {
            if (!ApiUtil.TryParseLocationIdOrUrl(LocationIdOrUrl, out var locationId))
            {
                Log(Resources.Error_InvalidLocationIdOrUrl);
                ShowWarning(Resources.Error_InvalidLocationIdOrUrl);
                return;
            }

            if (string.IsNullOrWhiteSpace(SettingManager.Settings.SteamExePath))
            {
                Log(Resources.Error_EmptySteamExePath);
                ShowWarning(Resources.Error_EmptySteamExePath);
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
        private ViewModelCommand? _openInVRChatCommand;
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
        private ViewModelCommand? _copyInstanceLinkCommand;
        public ViewModelCommand CopyInstanceLinkCommand => _copyInstanceLinkCommand ??= new ViewModelCommand(CopyInstanceLink);

        #region メニュー
        /// <summary>
        /// 設定画面を開きます．
        /// </summary>
        public void OpenSettingWindow()
        {
            Messenger.Raise(new TransitionMessage(typeof(SettingWindow), new SettingWindowViewModel(), TransitionMode.Modal, "WindowInteraction"));
        }
        private ViewModelCommand? _openSettingWindowCommand;
        public ViewModelCommand OpenSettingWindowCommand => _openSettingWindowCommand ??= new ViewModelCommand(OpenSettingWindow);

        /// <summary>
        /// About画面を開きます．
        /// </summary>
        public void OpenAboutWindow()
        {
            Messenger.Raise(new TransitionMessage(typeof(AboutWindow), new AboutWindowViewModel(), TransitionMode.Modal, "WindowInteraction"));
        }
        public ViewModelCommand OpenAboutWindowCommand => _openAboutWindowCommand ??= new ViewModelCommand(OpenAboutWindow);
        private ViewModelCommand? _openAboutWindowCommand;
        #endregion メニュー
        #endregion コマンド

        #region メソッド
        #endregion メソッド

        #region 内部関数
        /// <summary>
        /// グループをすべて更新します．
        /// </summary>
        private void UpdateGruop()
        {
            var groupList = Users.GroupBy(user => user.GroupName).Select(grouping => new UserGroup(grouping.Key, grouping));
            Groups = new ObservableCollection<UserGroup>(groupList);
        }

        /// <summary>
        /// Invite関連コマンドの実行可能状態更新処理を行います．
        /// </summary>
        private void InviteCommandRaiseCanExecuteChanged()
        {
            InviteAllCommand.RaiseCanExecuteChanged();
            InviteGroupCommand.RaiseCanExecuteChanged();
            InviteUserCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Update関連コマンドの実行可能状態更新処理を行います．
        /// </summary>
        private void UpdateCommandRaiseCanExecuteChanged()
        {
            UpdateAllCommand.RaiseCanExecuteChanged();
            UpdateGroupCommand.RaiseCanExecuteChanged();
            UpdateUserCommand.RaiseCanExecuteChanged();
        }

        private void Log(string msg)
        {
            Logs.Add(new UserLog(msg));
        }

        private void Log(object msg)
        {
            var tmp = msg?.ToString();
            if (tmp is null) { return; }
            Log(tmp);
        }

        /// <summary>
        /// 警告ダイアログを表示します．
        /// </summary>
        /// <param name="message">メッセージ内容</param>
        private void ShowWarning(string message)
        {
            Messenger.Raise(new InformationMessage(message, Resources.Title_Error, MessageBoxImage.Warning, "InformationMessage"));
        }

        /// <summary>
        /// 確認ダイアログを表示します．
        /// </summary>
        /// <param name="message">メッセージ内容</param>
        private void ShowInformation(string message)
        {
            Messenger.Raise(new InformationMessage(message, Resources.Title_Confirm, MessageBoxImage.Information, "InformationMessage"));
        }
        #endregion 内部関数
    }
}
