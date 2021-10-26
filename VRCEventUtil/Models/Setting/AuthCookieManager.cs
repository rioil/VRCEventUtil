using System;
using System.Collections.Generic;
using System.Text;
using VRCEventUtil.Models.Util;
using VRCEventUtil.Properties;
using System.Linq;

namespace VRCEventUtil.Models.Setting
{
    /// <summary>
    /// 認証クッキーの管理を行います．
    /// </summary>
    internal static class AuthCookieManager
    {
        static AuthCookieManager()
        {
            try
            {
                var authCookieData = ByteStringUtil.StringToByte(Settings.Default.AuthCookie);
                var iv = authCookieData.Take(CryptoUtil.IV_LEN);
                var crypto = authCookieData.Skip(CryptoUtil.IV_LEN);
                AuthCookie = CryptoUtil.Decrypt(iv, crypto);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            try
            {
                var mfaCookieData = ByteStringUtil.StringToByte(Settings.Default.MFACookie);
                var iv = mfaCookieData.Take(CryptoUtil.IV_LEN);
                var crypto = mfaCookieData.Skip(CryptoUtil.IV_LEN);
                MFACookie = CryptoUtil.Decrypt(iv, crypto);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        /// <summary>
        /// 認証クッキー
        /// </summary>
        public static string AuthCookie
        {
            get => _authCookie ?? string.Empty;
            set
            {
                if (_authCookie != value)
                {
                    _authCookie = value;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Settings.Default.AuthCookie = string.Empty;
                    }
                    else
                    {
                        var (iv, crypto) = CryptoUtil.Encrypt(_authCookie);
                        Settings.Default.AuthCookie = ByteStringUtil.ByteToString(iv.Concat(crypto));
                    }
                    Settings.Default.Save();
                }
            }
        }
        private static string? _authCookie;

        /// <summary>
        /// 多要素認証クッキー
        /// </summary>
        public static string MFACookie
        {
            get => _mfaCookie ?? string.Empty;
            set
            {
                if (value != _mfaCookie)
                {
                    _mfaCookie = value;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Settings.Default.MFACookie = string.Empty;
                    }
                    else
                    {
                        var (iv, crypto) = CryptoUtil.Encrypt(_mfaCookie);
                        Settings.Default.MFACookie = ByteStringUtil.ByteToString(iv.Concat(crypto));
                    }
                    Settings.Default.Save();
                }
            }
        }
        private static string? _mfaCookie;

        /// <summary>
        /// 認証クッキーをクリアします．
        /// </summary>
        public static void ClearCookies()
        {
            AuthCookie = string.Empty;
            MFACookie = string.Empty;
        }
    }
}
