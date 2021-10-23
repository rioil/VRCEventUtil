using Livet;
using System;
using System.Collections.Generic;
using System.Text;

namespace VRCEventUtil.Models
{
    public class UserLog : NotificationObject
    {
        /// <summary>
        /// ログ情報を作成します．
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="dateTime"></param>
        public UserLog(string msg, DateTime dateTime)
        {
            Message = msg;
            Time = dateTime;
        }

        /// <summary>
        /// ログ情報を作成します．
        /// </summary>
        /// <param name="msg"></param>
        public UserLog(string msg) : this(msg, DateTime.Now) { }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message
        {
            get => _message;
            set => RaisePropertyChangedIfSet(ref _message, value);
        }
        private string _message = default!;

        /// <summary>
        /// 時間
        /// </summary>
        public DateTime Time
        {
            get => _time;
            set => RaisePropertyChangedIfSet(ref _time, value);
        }
        private DateTime _time;
    }
}
