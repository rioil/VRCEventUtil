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

        public static AppSettings Settings { get; private set; }

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
            SaveSetting(settingFilePath);
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
