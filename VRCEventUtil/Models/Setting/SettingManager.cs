using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace VRCEventUtil.Models.Setting
{
    class SettingManager
    {
        public const string DEFAULT_SETTING_FILE_PATH = "settings.json";

        public static string SettingFilePath { get; private set; }

        public static AppSettings Settings { get; set; }

        public static bool LoadSetting(string settingFilePath = DEFAULT_SETTING_FILE_PATH)
        {
            if (!File.Exists(settingFilePath))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(settingFilePath);
                Settings = JsonConvert.DeserializeObject<AppSettings>(json, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
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
            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
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
        public static void AutoSetSteamExePath()
        {
            if (!File.Exists(Settings.SteamExePath))
            {
                Settings.SteamExePath = SearchSteamExe();
                SaveSetting(SettingFilePath);
                Logger.Log($"steam.exeのパスを{Settings.SteamExePath}に自動設定しました．");
            }
        }

        /// <summary>
        /// steam.exeを検索してパスを取得します．
        /// </summary>
        /// <returns>パス．見つからなければnull</returns>
        private static string SearchSteamExe()
        {
            const string DEFAULT_PATH = @"C:\Program Files (x86)\Steam\steam.exe";
            if (File.Exists(DEFAULT_PATH))
            {
                return DEFAULT_PATH;
            }

            return null;
        }
    }

    public class ConcreteConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<T>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
