using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VRCEventUtil.Models.Setting
{
    class SettingManager
    {
        public const string DEFAULT_SETTING_FILE_PATH = "settings.json";

        public static string SettingFilePath { get; private set; } = default!;

        public static AppSettings Settings { get; set; } = default!;

        public static bool LoadSetting(string settingFilePath = DEFAULT_SETTING_FILE_PATH)
        {
            if (!File.Exists(settingFilePath))
            {
                CreateDefaultSetting();
                return false;
            }

            try
            {
                var json = File.ReadAllText(settingFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                Settings = JsonSerializer.Deserialize<AppSettings>(json, options);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

            SettingFilePath = settingFilePath;
            AutoSetSteamExePath();
            return true;
        }

        public static bool SaveSetting(string settingFilePath = DEFAULT_SETTING_FILE_PATH)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(Settings, options);
            try
            {
                File.WriteAllText(settingFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

            return true;
        }

        public static void CreateDefaultSetting(string settingFilePath = DEFAULT_SETTING_FILE_PATH)
        {
            Settings = new AppSettings();
            SettingFilePath = settingFilePath;
            AutoSetSteamExePath();
            SaveSetting(settingFilePath);
        }

        /// <summary>
        /// steam.exeのパスを自動設定します．
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static void AutoSetSteamExePath()
        {
            if (Settings is null || SettingFilePath is null)
            {
                throw new InvalidOperationException();
            }

            if (!File.Exists(Settings.SteamExePath))
            {
                Settings.SteamExePath = GetSteamExePath();
                SaveSetting(SettingFilePath);
                Logger.Log($"steam.exeのパスを{Settings.SteamExePath}に自動設定しました．");
            }
        }

        /// <summary>
        /// steam.exeのパスを取得します．
        /// </summary>
        /// <returns>パス．見つからなければnull</returns>
        private static string? GetSteamExePath()
        {
            const string DEFAULT_PATH = @"C:\Program Files (x86)\Steam\steam.exe";
            string? path = null;

            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey("steam").OpenSubKey("Shell").OpenSubKey("Open").OpenSubKey("Command"))
                {
                    var command = key.GetValue(null) as string;
                    if (command is object)
                    {
                        path = Regex.Matches(command, @"""[^""]*""").FirstOrDefault()?.Value.Trim('"');
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                return File.Exists(DEFAULT_PATH) ? DEFAULT_PATH : null;
            }
        }
    }
}
