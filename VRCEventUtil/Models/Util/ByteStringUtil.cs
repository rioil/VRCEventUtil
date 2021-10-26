using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace VRCEventUtil.Models.Util
{
    /// <summary>
    /// 文字列・バイト配列変換ユーティリティ
    /// </summary>
    public static class ByteStringUtil
    {
        /// <summary>
        /// <see cref="ByteToString(IEnumerable{byte})"/>で作成した文字列を元のバイト配列に変換します．
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static IEnumerable<byte> StringToByte(string str)
        {
            if (string.IsNullOrEmpty(str)) { return Enumerable.Empty<byte>(); }
            return Convert.FromBase64String(str);
        }

        /// <summary>
        /// バイト配列を文字列に変換します．
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToString(IEnumerable<byte> bytes)
        {
            return Convert.ToBase64String(bytes.ToArray());
        }
    }
}
