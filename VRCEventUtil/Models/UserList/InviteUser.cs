using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CsvHelper.Configuration.Attributes;
using Livet;

namespace VRCEventUtil.Models.UserList
{
    public class InviteUser : NotificationObject
    {
        public InviteUser(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public InviteUser() { }

        /// <summary>
        /// ユーザー名
        /// </summary>
        [Index(0)]
        public string Name
        {
            get => _name;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _name, value));
        }
        private string _name;

        /// <summary>
        /// ユーザーID
        /// </summary>
        [Index(1)]
        public string Id
        {
            get => _id;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _id, value));
        }
        private string _id;

        /// <summary>
        /// オンラインか
        /// </summary>
        [Ignore]
        public bool IsOnline
        {
            get => _isOnline;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _isOnline, value));
        }
        private bool _isOnline;

        /// <summary>
        /// Invite送信済みか
        /// </summary>
        [Ignore]
        public bool HasInvited
        {
            get => _hasInvited;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _hasInvited, value));
        }
        private bool _hasInvited;

        /// <summary>
        /// ユーザーがワールドにいるか
        /// </summary>
        [Ignore]
        public bool IsInWorld
        {
            get => _isInWorld;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _isInWorld, value));
        }
        private bool _isInWorld;

        /// <summary>
        /// インスタンスチェックが予定されているか
        /// </summary>
        [Ignore]
        public bool IsLocationCheckScheduled
        {
            get => _isLocationCheckScheduled;
            set => DispatcherHelper.UIDispatcher.Invoke(() => RaisePropertyChangedIfSet(ref _isLocationCheckScheduled, value));
        }
        private bool _isLocationCheckScheduled;
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
