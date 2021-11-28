using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Livet;

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
        public TimeSpan ApiCallInterval
        {
            get => _apiCallInterval;
            set
            {
                if (RaisePropertyChangedIfSet(ref _apiCallInterval, value, nameof(ApiCallInterval)))
                {
                    IsChanged = true;
                }
            }
        }
        private TimeSpan _apiCallInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// PreReleaseをアップデート確認に含めるか
        /// </summary>
        public bool CheckPreRelease
        {
            get => _checkPreRelease;
            set => RaisePropertyChangedIfSet(ref _checkPreRelease, value);
        }
        private bool _checkPreRelease;

        /// <summary>
        /// SteamのEXEファイルのパス
        /// </summary>
        public string? SteamExePath
        {
            get => _steamExePath;
            set
            {
                if (RaisePropertyChangedIfSet(ref _steamExePath, value))
                {
                    IsChanged = true;
                }
            }
        }
        private string? _steamExePath;

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

        /// <summary>
        /// 設定の簡易コピーを作成します．
        /// </summary>
        /// <returns></returns>
        public AppSettings ShallowCopy() => (AppSettings)MemberwiseClone();
    }
}
