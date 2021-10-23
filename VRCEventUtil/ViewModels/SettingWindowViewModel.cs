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
using Livet.Messaging.IO;
using VRCEventUtil.Properties;

namespace VRCEventUtil.ViewModels
{
    class SettingWindowViewModel : ViewModel
    {
        public void Initialize()
        {
            Settings =  SettingManager.Settings.ShallowCopy();
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
        private AppSettings _settings = default!;

        #region コマンド

        /// <summary>
        /// SteamのEXEファイルパス選択画面を表示します．
        /// </summary>
        public void SelectSteamExePath()
        {
            var dialog = new OpeningFileSelectionMessage("OpenFileDialog")
            {
                Title = Resources.Title_SelectSteamExeFile,
                Filter = Resources.FileFilter_Exe
            };
            Messenger.Raise(dialog);

            if (dialog.Response is object && dialog.Response.Any())
            {
                Settings.SteamExePath = dialog.Response[0];
            }
        }
        private ViewModelCommand? _selectSteamExePathCommand;
        public ViewModelCommand SelectSteamExePathCommand => _selectSteamExePathCommand ??= new ViewModelCommand(SelectSteamExePath);

        /// <summary>
        /// 設定を保存して画面を閉じます．
        /// </summary>
        public void Save()
        {
            SettingManager.Settings = Settings;
            SettingManager.SaveSetting();
            Messenger.Raise(new InteractionMessage("CloseWindow"));
        }
        private ViewModelCommand? _saveCommand;
        public ViewModelCommand SaveCommand => _saveCommand ??= new ViewModelCommand(Save);

        /// <summary>
        /// 設定を保存せずに画面を閉じます．
        /// </summary>
        public void Cancel()
        {
            if (Settings.IsChanged)
            {
                var res = Messenger.GetResponse(new ConfirmationMessage(Resources.Confirm_DiscardSettingChanges, Resources.Title_Confirm,
                    MessageBoxImage.Information, MessageBoxButton.YesNo, "ConfirmMessage"));
                if (res.Response != true)
                {
                    return;
                }
            }

            Messenger.Raise(new InteractionMessage("CloseWindow"));
        }
        private ViewModelCommand? _cancelCommand;
        public ViewModelCommand CancelCommand => _cancelCommand ??= new ViewModelCommand(Cancel);

        #endregion コマンド
    }
}
