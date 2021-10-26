using Livet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //AssemblyVersion = assembly.GetName().Version?.ToString() ?? MISSING_INFO;
            var info = FileVersionInfo.GetVersionInfo(assembly.Location);
            FileVersion = info.FileVersion;
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
        /// アセンブリバージョン
        /// </summary>
        //public string AssemblyVersion
        //{
        //    get => _assemblyVersion;
        //    set => RaisePropertyChangedIfSet(ref _assemblyVersion, value);
        //}
        //private string _assemblyVersion = default!;

        /// <summary>
        /// ファイルバージョン
        /// </summary>
        public string FileVersion
        {
            get => _fileVersion;
            set => RaisePropertyChangedIfSet(ref _fileVersion, value);
        }
        private string _fileVersion = default!;
    }
}
