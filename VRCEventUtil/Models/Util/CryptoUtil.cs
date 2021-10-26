using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VRCEventUtil.Models.Util
{
    public static class CryptoUtil
    {
        public const int IV_LEN = 16;

        private static byte[] DefaultKey => _defaultKey ??= GenerateKey();
        private static byte[]? _defaultKey;

        public static (byte[] iv, byte[] crypto) Encrypt(string str, byte[]? key = null)
        {
            using var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;

            var iv = aes.IV;
            aes.Key = (key ?? DefaultKey).Take(aes.Key.Length).ToArray();

            var bytes = Encoding.UTF8.GetBytes(str);
            using var encryptor = aes.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            return (iv, encryptedBytes);
        }

        public static string Decrypt(byte[] iv, byte[] crypto, byte[]? key = null)
        {
            using Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;

            aes.IV = iv.Take(aes.IV.Length).ToArray();
            aes.Key = (key ?? DefaultKey).Take(aes.Key.Length).ToArray();

            using var encryptedStream = new MemoryStream(crypto);
            using var cryptoStream = new CryptoStream(encryptedStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var decryptReader = new StreamReader(cryptoStream, Encoding.UTF8);
            string decryptedData = decryptReader.ReadToEnd();

            return decryptedData;
        }

        public static string Decrypt(IEnumerable<byte> iv, IEnumerable<byte> crypto, IEnumerable<byte>? key = null)
        {
            return Decrypt(iv.ToArray(), crypto.ToArray(), key?.ToArray());
        }


        private static byte[] GenerateKey()
        {
            var key = DeviceIdentificationHelper.GetMacAddress()
                .Select(addr => addr.GetHashCode())
                .Aggregate((key, addr) => key ^ addr);

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(BitConverter.GetBytes(key));

            return hash;
        }
    }
}
