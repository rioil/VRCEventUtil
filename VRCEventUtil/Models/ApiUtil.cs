using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VRCEventUtil.Models
{
    static class ApiUtil
    {
        const string LAUNCH_URL_PATTERN = @"https://vrchat\.com/home/launch\?worldId=(?<worldId>.*)&instanceId=(?<instanceId>.*)";
        const string LOCATION_ID_PATTERN = @"(?<worldId>wrld_.*):(?<instanceId>\d{5}.*)";

        /// <summary>
        /// Location IDに変換します．
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertToLocationId(string str)
        {

            if (str is null) { throw new ArgumentNullException("インスタンスIDを入力してください．"); }

            var match = Regex.Match(str, LAUNCH_URL_PATTERN);
            if (match.Success)
            {
                return $"{match.Groups["worldId"]}:{match.Groups["instanceId"]}";
            }
            else if (Regex.IsMatch(str, LOCATION_ID_PATTERN))
            {
                return str;
            }
            else
            {
                throw new FormatException("インスタンスIDの形式が正しくありません．");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public static (string WorldId, string InstanceId) ParseLocationId(string locationId)
        {
            var match = Regex.Match(locationId, LOCATION_ID_PATTERN);
            if (match.Success)
            {
                var worldId = match.Groups["worldId"].Value;
                var instanceId = match.Groups["instanceId"].Value;
                return (worldId, instanceId);
            }
            else
            {
                throw new FormatException();
            }
        }
    }
}
