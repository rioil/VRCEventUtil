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
        /// <summary>
        /// ユーザーリストファイルの読み取りを行います．
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ユーザーリスト</returns>
        /// <exception cref="FileNotFoundException">ファイルが存在しない場合</exception>
        /// <exception cref="UserListFileException">ファイルの読み取りに失敗した場合</exception>
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
            
            try
            {
                var records = JsonSerializer.Deserialize<InviteUser[]>(File.ReadAllText(filePath), options).ToList();
                var uniqueRecords = records.Distinct(new InviteUserComparer()).ToList();
                if (records.Count != uniqueRecords.Count)
                {
                    Logger.Log($"ユーザーリストファイル {filePath} 読み込み時に，{records.Count() - uniqueRecords.Count}件の重複データを削除しました．");
                }

                return uniqueRecords;
            }
            catch (JsonException ex)
            {
                Logger.Log(ex);
                throw new UserListFileException($"ユーザーリストファイルの形式が正しくありません．\n{ex.LineNumber}行目に問題があります．", ex);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw new UserListFileException("ユーザーリストファイルの読み込みに失敗しました．", ex);
            }
        }
    }

    internal class UserListFileException : Exception
    {
        public UserListFileException(string? message) : base(message)
        {
        }

        public UserListFileException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
