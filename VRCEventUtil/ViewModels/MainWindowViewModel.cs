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

namespace VRCEventUtil.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        // Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
        public async void Initialize()
        {
            IsLoading = true;
            Username = Settings.Default.Username;
            Configuration.Default.Username = Username;
            IsLoggedIn = await Task.Run(() => ApiManager.Instance.LoadAuthCookies());
            IsLoading = false;
        }

        #region メンバ変数
        #endregion メンバ変数

        #region プロパティ
        /// <summary>
        /// インスタンス管理
        /// </summary>
        public WorldInstanceManager WorldInstanceManager { get; } = new WorldInstanceManager();
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
            set => RaisePropertyChangedIfSet(ref _worldId, value);
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
                    Users = new ObservableCollection<Models.UserList.User>(UserListReader.Read(value));
                }
            }
        }
        private string _userListFilePath;

        /// <summary>
        /// ユーザーリスト
        /// </summary>
        public ObservableCollection<Models.UserList.User> Users
        {
            get => _users;
            set => RaisePropertyChangedIfSet(ref _users, value);
        }
        private ObservableCollection<Models.UserList.User> _users;
        #endregion 変更通知プロパティ

        #region コマンド
        /// <summary>
        /// ログインを行います．
        /// </summary>
        public async void Login()
        {
            if (await ApiManager.Instance.Login(Username, Password, MFACode))
            {
                Log(ApiManager.Instance.CurrentUser.Username);
                Settings.Default.Username = Username;
                Settings.Default.Save();
                IsLoggedIn = true;
            }
            else
            {
                // TODO 通知
            }
        }
        private ViewModelCommand _loginCommand;
        public ViewModelCommand LoginCommand => _loginCommand ??= new ViewModelCommand(Login);

        /// <summary>
        /// Invite送信処理を行います．
        /// </summary>
        public async void Invite()
        {
            var userIds = Users.Select(user => user.Id).ToList();
            var progress = new Progress<string>();
            progress.ProgressChanged += (_, msg) => DispatcherHelper.UIDispatcher.Invoke(() => Log(msg));
            var result = await ApiManager.Instance.Invite(InstanceId, userIds, progress);
            if (result)
            {
                Log("Inviteに成功しました．");
            }
            else
            {
                Log("Inviteに失敗しました．");
            }
        }

        public bool CanInvite() => !string.IsNullOrWhiteSpace(InstanceId);
        private ViewModelCommand _inviteCommand;
        public ViewModelCommand InviteCommand => _inviteCommand ??= new ViewModelCommand(Invite, CanInvite);

        /// <summary>
        /// ワールドインスタンス作成処理を行います．
        /// </summary>
        public async void CreateWorldInstance()
        {
            InstanceId = await ApiManager.Instance.CreateWorldInstance(WorldId);
            Log($"インスタンスを作成しました．\nID:{InstanceId}");
        }
        public ViewModelCommand CreateWorldInstanceCommand => _createWorldInstanceCommand ??= new ViewModelCommand(CreateWorldInstance);
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

            if (dialog.Response.Any())
            {
                UserListFilePath = dialog.Response[0];
            }
        }
        private ViewModelCommand _selectUserListFileCommand;
        public ViewModelCommand SelectUserListFileCommand => _selectUserListFileCommand ??= new ViewModelCommand(SelectUserListFile);

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
