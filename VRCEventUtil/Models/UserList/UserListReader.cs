﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using CsvHelper;

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

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<InviteUser>();
                var distinctedRecords = records.Distinct(new InviteUserComparer()).ToList();
                if (records.Count() != distinctedRecords.Count)
                {
                    Logger.Log($"ユーザーリストファイル {filePath} 読み込み時に，{records.Count() - distinctedRecords.Count}件の重複データを削除しました．");
                }

                return distinctedRecords;
            }
        }
    }
}
