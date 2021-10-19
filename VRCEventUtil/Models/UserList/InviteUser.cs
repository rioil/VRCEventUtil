using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Livet;
using System.Text.Json.Serialization;

#nullable enable

namespace VRCEventUtil.Models.UserList
{
    /// <summary>
    /// Inviteするユーザー
    /// </summary>
    public class InviteUser : NotificationObject
    {
        /// <summary>
        /// 名前とIDを指定してユーザーを作成します．
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public InviteUser(string id, string? name)
        {
            Id = id;
            Name = name;
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
        /// スタッフか
        /// </summary>
        public bool IsStaff
        {
            get => _IsStaff;
            set => RaisePropertyChangedIfSet(ref _IsStaff, value);
        }
        private bool _IsStaff;

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
        /// ユーザーがワールドにいるか
        /// </summary>
        [JsonIgnore]
        public bool IsInWorld
        {
            get => _isInWorld;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _isInWorld, value));
        }
        private bool _isInWorld;
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
