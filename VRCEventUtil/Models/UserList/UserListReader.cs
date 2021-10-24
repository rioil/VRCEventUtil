using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;

namespace VRCEventUtil.Models.UserList
{
    public static class UserListReader
    {
        public static List<InviteUser> ReadInviteUser(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            // TODO 例外処理
            var records = JsonSerializer.Deserialize<InviteUser[]>(File.ReadAllText(filePath), options).ToList();
            var distinctedRecords = records.Distinct(new InviteUserComparer()).ToList();
            if (records.Count != distinctedRecords.Count)
            {
                Logger.Log($"ユーザーリストファイル {filePath} 読み込み時に，{records.Count() - distinctedRecords.Count}件の重複データを削除しました．");
            }

            return distinctedRecords;
        }
    }
}
