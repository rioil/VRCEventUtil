using Livet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace VRCEventUtil.Models.UserList
{
    /// <summary>
    /// ユーザーグループ
    /// </summary>
    public class UserGroup : NotificationObject
    {
        /// <summary>
        /// デフォルトのグループ名
        /// </summary>
        public const string DEFAULT_GROUP_NAME = "default";

        #region コンストラクタ
        /// <summary>
        /// グループ番号とユーザーリストを指定してグループを作成します．
        /// </summary>
        /// <param name="groupName">グループ名</param>
        /// <param name="users">ユーザーリスト</param>
        public UserGroup(string groupName, IEnumerable<InviteUser> users)
        {
            GroupName = groupName;
            Users = new ObservableCollection<InviteUser>(users);
        }

        /// <summary>
        /// グループ番号とユーザー一人を指定してグループを作成します．
        /// </summary>
        /// <param name="gorupName">グループ名</param>
        /// <param name="user">ユーザー</param>
        public UserGroup(string gorupName, InviteUser user)
        {
            GroupName = gorupName;
            Users = new ObservableCollection<InviteUser> { user };
        }
        #endregion コンストラクタ

        #region 変更通知プロパティ
        /// <summary>
        /// グループ名
        /// </summary>
        public string GroupName
        {
            get => _groupName;
            set => RaisePropertyChangedIfSet(ref _groupName, value);
        }
        private string _groupName = DEFAULT_GROUP_NAME;

        /// <summary>
        /// ユーザーリスト
        /// </summary>
        public ObservableCollection<InviteUser> Users
        {
            get => _users;
            set => RaisePropertyChangedIfSet(ref _users, value);
        }
        private ObservableCollection<InviteUser> _users = default!;
        #endregion 変更通知プロパティ
    }
}
