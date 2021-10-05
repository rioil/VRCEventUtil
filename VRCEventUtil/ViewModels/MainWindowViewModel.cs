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
using io.github.vrchatapi.Api;
using io.github.vrchatapi.Client;
using System.Threading.Tasks;
using io.github.vrchatapi.Model;
using VRCEventUtil.Properties;
using VRCEventUtil.Models.UserList;
using System.Windows;

namespace VRCEventUtil.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        // Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
        public async void Initialize()
        {
            IsLoading = true;
            Username = Settings.Default.Username;
            ApiManager.Instance.ApiLog += msg => DispatcherHelper.UIDispatcher.Invoke(() => Log(msg));
            IsLoggedIn = await Task.Run(() => ApiManager.Instance.Login(Username, null));
            if (IsLoggedIn)
            {
                Log($"{Username}として自動ログインしました．");
            }
            else
            {
                Log($"自動ログインに失敗しました．ログインが必要です．");
            }
            IsLoading = false;
        }

        #region メンバ変数
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
        /// インスタンスID
        /// </summary>
        public string InstanceId
        {
            get => _instanceId;
            set
            {
                if (RaisePropertyChangedIfSet(ref _instanceId, value))
                {
                    InviteCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _instanceId;

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
        /// ワールドID
        /// </summary>
        public string WorldId
        {
            get => _worldId;
            set
            {
                if (RaisePropertyChangedIfSet(ref _worldId, value))
                {
                    CreateWorldInstanceCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _worldId;

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
                }
            }
        }
        private bool _isInviting;

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
                Log($"{Username}としてログインしました．");
                Settings.Default.Username = Username;
                Settings.Default.Save();
                IsLoggedIn = true;
            }
            else
            {
                Messenger.Raise(new InformationMessage("ログインに失敗しました．\nユーザー名，パスワード（必要であれば二要素認証コード）が正しいことを確認してください．",
                    "エラー", MessageBoxImage.Warning, "InformationMessage"));
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
            Log("ログアウトしました．");
        }
        private ViewModelCommand _logoutCommand;
        public ViewModelCommand LogoutCommand => _logoutCommand ??= new ViewModelCommand(Logout);

        /// <summary>
        /// Invite送信処理を行います．
        /// </summary>
        public async void Invite()
        {
            InviteProgress = 0;
            IsInviting = true;

            var progress = new Progress<double>();
            progress.ProgressChanged += (_, val) => DispatcherHelper.UIDispatcher.Invoke(() => InviteProgress = (int)val);
            var result = await ApiManager.Instance.Invite(InstanceId, Users, progress);
            if (result)
            {
                Log("Inviteに成功しました．");
            }
            else
            {
                Log("Inviteに失敗しました．");
            }

            IsInviting = false;
        }

        public bool CanInvite() => !IsInviting && !string.IsNullOrWhiteSpace(InstanceId) && Users.Any();
        private ViewModelCommand _inviteCommand;
        public ViewModelCommand InviteCommand => _inviteCommand ??= new ViewModelCommand(Invite, CanInvite);

        /// <summary>
        /// ワールドインスタンス作成処理を行います．
        /// </summary>
        public async void CreateWorldInstance()
        {
            InstanceId = await ApiManager.Instance.CreateWorldInstance(WorldId, InstanceRegion, InstanceDisclosureRange);
            var instanceId = ApiUtil.ParseLocationId(InstanceId).InstanceId;
            Log($"インスタンスを作成しました．\n" +
                $"サーバー地域：{InstanceRegion} 公開範囲：{InstanceDisclosureRange}\n" +
                $"ワールドID：{WorldId}\n" +
                $"インスタンスID：{instanceId}");
        }
        public bool CanCreateWorldInstance() => !string.IsNullOrWhiteSpace(WorldId);
        public ViewModelCommand CreateWorldInstanceCommand => _createWorldInstanceCommand ??= new ViewModelCommand(CreateWorldInstance, CanCreateWorldInstance);
        private ViewModelCommand _createWorldInstanceCommand;

        /// <summary>
        /// ユーザーリストファイル選択ダイアログを表示します．
        /// </summary>
        public void SelectUserListFile()
        {
            var dialog = new OpeningFileSelectionMessage("OpenFileDialog")
            {
                Title = "ユーザーリストファイル選択",
                Filter = "CSVファイル (*.csv)|*.csv"
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

        #endregion コマンド

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
