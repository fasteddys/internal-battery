using NETCore.Encrypt;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Shared
{
    public class Crypto
    {
        public static Encoding iso = Encoding.GetEncoding("ISO-8859-1");

        public static string Encrypt(string key, string text)
        {
            return EncryptProvider.AESEncrypt(text, key);
        }

        public static string Decrypt(string key, string text)
        {
            return EncryptProvider.AESDecrypt(text, key);
        }
    }
}
