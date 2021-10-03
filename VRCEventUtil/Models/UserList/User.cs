using System;
using System.Collections.Generic;
using System.Text;
using Livet;

namespace VRCEventUtil.Models.UserList
{
    public class User : NotificationObject
    {
        public User(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public User() { }

        /// <summary>
        /// ユーザー名
        /// </summary>
        public string Name
        {
            get => _name;
            set => RaisePropertyChangedIfSet(ref _name, value);
        }
        private string _name;

        /// <summary>
        /// ユーザーID
        /// </summary>
        public string Id
        {
            get => _id;
            set => RaisePropertyChangedIfSet(ref _id, value);
        }
        private string _id;
    }
}
