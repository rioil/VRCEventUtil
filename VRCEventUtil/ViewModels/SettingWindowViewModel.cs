using Livet;
using Livet.Commands;
using Livet.Messaging;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using VRCEventUtil.Models;
using VRCEventUtil.Models.Setting;

namespace VRCEventUtil.ViewModels
{
    class SettingWindowViewModel : ViewModel
    {
        public void Initialize()
        {
            Settings = SettingManager.Settings;
            Settings.IsChanged = false;
        }

        /// <summary>
        /// 設定項目
        /// </summary>
        public AppSettings Settings
        {
            get => _settings;
            set => RaisePropertyChangedIfSet(ref _settings, value);
        }
        private AppSettings _settings;

        #region コマンド
        /// <summary>
        /// 設定を保存して画面を閉じます．
        /// </summary>
        public void Save()
        {
            SettingManager.SaveSetting();
            Messenger.Raise(new InteractionMessage("CloseWindow"));
        }
        private ViewModelCommand _saveCommand;
        public ViewModelCommand SaveCommand => _saveCommand ??= new ViewModelCommand(Save);

        /// <summary>
        /// 設定を保存せずに画面を閉じます．
        /// </summary>
        public void Cancel()
        {
            if (Settings.IsChanged)
            {
                var res = Messenger.GetResponse(new ConfirmationMessage("変更は保存されません．設定画面を閉じますか？", "確認", MessageBoxImage.Information, MessageBoxButton.YesNo, "ConfirmMessage"));
                if (res.Response != true)
                {
                    return;
                }
            }

            Messenger.Raise(new InteractionMessage("CloseWindow"));
        }
        private ViewModelCommand _cancelCommand;
        public ViewModelCommand CancelCommand => _cancelCommand ??= new ViewModelCommand(Cancel);

        #endregion コマンド
    }
}
