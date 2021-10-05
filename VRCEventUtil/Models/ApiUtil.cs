using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VRCEventUtil.Models
{
    static class ApiUtil
    {
        const string LAUNCH_URL_PATTERN = @"^https://vrchat\.com/home/launch\?worldId=(?<worldId>.*)&instanceId=(?<instanceId>.*)$";
        const string LOCATION_ID_PATTERN = @"^(?<worldId>wrld_.*):(?<instanceId>\d{5}.*)$";
        const string WORLD_ID_PATTERN = @"^(?<worldId>wrld_[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12})$";
        const string WORLD_URL_PATTERN = @"^https://vrchat\.com/home/world/(?<worldId>wrld_[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12})$";

        /// <summary>
        /// Location IDの形式が正しいか判定します．
        /// </summary>
        /// <param name="locationIdOrUrl"></param>
        /// <returns></returns>
        public static bool ValidateLocationIdOrUrl(string locationIdOrUrl) => Regex.IsMatch(locationIdOrUrl, LAUNCH_URL_PATTERN) || Regex.IsMatch(locationIdOrUrl, LOCATION_ID_PATTERN);

        /// <summary>
        /// Location IDを解析します．
        /// </summary>
        /// <param name="locationIdOrUrl"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public static bool TryParseLocationIdOrUrl(string locationIdOrUrl, out string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationIdOrUrl))
            {
                locationId = null;
                return false;
            }

            var match = Regex.Match(locationIdOrUrl, LAUNCH_URL_PATTERN);
            if (match.Success)
            {
                locationId = $"{match.Groups["worldId"]}:{match.Groups["instanceId"]}";
                return true;
            }
            else if (Regex.IsMatch(locationIdOrUrl, LOCATION_ID_PATTERN))
            {
                locationId = locationIdOrUrl;
                return true;
            }
            else
            {
                locationId = null;
                return false;
            }
        }

        /// <summary>
        /// Location IDに変換します．
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
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
        /// <exception cref="FormatException"></exception>
        public static (string WorldId, string InstanceId) ResolveLocationId(string locationId)
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

        /// <summary>
        /// ワールドIDまたはURLを解析してワールドIDを返します．
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static string ParseWorldId(string worldId)
        {
            if (Regex.IsMatch(worldId, WORLD_ID_PATTERN))
            {
                return worldId;
            }

            var match = Regex.Match(worldId, WORLD_URL_PATTERN);
            if (match.Success)
            {
                return match.Groups["worldId"].Value;
            }

            throw new FormatException();
        }

        /// <summary>
        /// ワールドIDまたはワールドURLを解析してワールドIDを返します．
        /// </summary>
        /// <param name="worldIdOrUrl"></param>
        /// <returns></returns>
        public static bool TryParseWorldId(string worldIdOrUrl, out string worldId)
        {
            if (Regex.IsMatch(worldIdOrUrl, WORLD_ID_PATTERN))
            {
                worldId = worldIdOrUrl;
                return true;
            }

            var match = Regex.Match(worldIdOrUrl, WORLD_URL_PATTERN);
            if (match.Success)
            {
                worldId = match.Groups["worldId"].Value;
                return true;
            }

            worldId = null;
            return false;
        }

        /// <summary>
        /// ワールドIDの形式が正しいかを判定します．
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        public static bool ValidateWorldId(string worldId)
        {
            if (Regex.IsMatch(worldId, WORLD_ID_PATTERN)) { return true; }
            if (Regex.IsMatch(worldId, WORLD_URL_PATTERN)) { return true; }

            return false;
        }
    }
}
