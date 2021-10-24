using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Livet;
using System.Text.Json.Serialization;

namespace VRCEventUtil.Models.UserList
{
    /// <summary>
    /// Inviteするユーザー
    /// </summary>
    public class InviteUser : NotificationObject
    {
        /// <summary>
        /// 名前とID，グループ名を指定してユーザーを作成します．
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="groupName"></param>
        public InviteUser(string id, string? name, string groupName)
        {
            Id = id;
            Name = name;
            GroupName = groupName;
        }

        /// <summary>
        /// ユーザーを作成します．
        /// </summary>
        public InviteUser() { }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public string Id
        {
            get => _id;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _id, value));
        }
        private string _id = string.Empty;

        /// <summary>
        /// ユーザー名
        /// </summary>
        public string? Name
        {
            get => _name;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _name, value));
        }
        private string? _name;

        /// <summary>
        /// グループ名
        /// </summary>
        [JsonPropertyName("group")]
        public string GroupName
        {
            get => _groupName;
            set => RaisePropertyChangedIfSet(ref _groupName, value);
        }
        private string _groupName = UserGroup.DEFAULT_GROUP_NAME;

        /// <summary>
        /// オンラインか
        /// </summary>
        [JsonIgnore]
        public bool IsOnline
        {
            get => _isOnline;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _isOnline, value));
        }
        private bool _isOnline;

        /// <summary>
        /// Invite送信済みか
        /// </summary>
        [JsonIgnore]
        public bool HasInvited
        {
            get => _hasInvited;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _hasInvited, value));
        }
        private bool _hasInvited;

        /// <summary>
        /// ユーザーがインスタンスにいるか
        /// </summary>
        [JsonIgnore]
        public bool IsInInstance
        {
            get => _isInInstance;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _isInInstance, value));
        }
        private bool _isInInstance;
    }

    /// <summary>
    /// InviteUserの等値判定クラス
    /// </summary>
    public class InviteUserComparer : IEqualityComparer<InviteUser>
    {
        public bool Equals([AllowNull] InviteUser x, [AllowNull] InviteUser y)
        {
            return x?.Id == y?.Id;  // IDが一致していれば同一ユーザー
        }

        public int GetHashCode([DisallowNull] InviteUser obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
