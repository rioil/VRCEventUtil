using Livet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using VRCEventUtil.Models;

namespace VRCEventUtil
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        // Application level error handling
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO
            MessageBox.Show(
                "予期しないエラーが発生しました.プログラムを終了します．",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            Logger.Log(e.ExceptionObject as Exception);

            Environment.Exit(1);
        }
    }
}
