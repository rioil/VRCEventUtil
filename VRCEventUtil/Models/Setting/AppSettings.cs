using System;
using System.Collections.Generic;
using System.Text;
using Livet;
using Newtonsoft.Json;

namespace VRCEventUtil.Models.Setting
{
    public class AppSettings : NotificationObject
    {
        #region プロパティ
        /// <summary>
        /// 設定が変更されたか
        /// </summary>
        [JsonIgnore]
        public bool IsChanged { get; set; }
        #endregion プロパティ

        #region 変更通知プロパティ
        /// <summary>
        /// ログイン状態を保存するか
        /// </summary>
        public bool SaveAuthCookies
        {
            get => _saveAuthCookies;
            set
            {
                if (RaisePropertyChangedIfSet(ref _saveAuthCookies, value, nameof(SaveAuthCookies)))
                {
                    IsChanged = true;
                }
            }
        }
        private bool _saveAuthCookies = true;

        /// <summary>
        /// API呼び出しの最小間隔（秒）
        /// </summary>
        public int ApiCallIntervalSec
        {
            get => _apiCallIntervalSec;
            set
            {
                if (RaisePropertyChangedIfSet(ref _apiCallIntervalSec, value, nameof(ApiCallIntervalSec)))
                {
                    IsChanged = true;
                }
            }
        }
        private int _apiCallIntervalSec = 2;

        /// <summary>
        /// VRChatの起動にVRModeを使用するか
        /// </summary>
        public bool UseVRMode
        {
            get => _useVRMode;
            set
            {
                if (RaisePropertyChangedIfSet(ref _useVRMode, value, nameof(UseVRMode)))
                {
                    IsChanged = true;
                }
            }
        }
        private bool _useVRMode;
        #endregion 変更通知プロパティ
    }
}
