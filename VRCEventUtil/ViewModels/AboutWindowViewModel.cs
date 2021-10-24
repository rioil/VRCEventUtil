using Livet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace VRCEventUtil.ViewModels
{
    public class AboutWindowViewModel : ViewModel
    {
        public const string MISSING_INFO = "unavailable";

        public void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AppName = assembly.GetName().Name ?? MISSING_INFO;
            Version = assembly.GetName().Version?.ToString() ?? MISSING_INFO;
        }

        /// <summary>
        /// アプリ名
        /// </summary>
        public string AppName
        {
            get => _appName;
            set => RaisePropertyChangedIfSet(ref _appName, value);
        }
        private string _appName = default!;

        /// <summary>
        /// バージョン
        /// </summary>
        public string Version
        {
            get => _version;
            set => RaisePropertyChangedIfSet(ref _version, value);
        }
        private string _version = default!;
    }
}
