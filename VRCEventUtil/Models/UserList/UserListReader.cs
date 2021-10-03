using System;
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
        public static List<User> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<User>();
                return records.ToList();
            }
        }
    }
}
