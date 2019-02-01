using SecurityDriven.Inferno;
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
            byte[] keyBytes = iso.GetBytes(key);
            byte[] sourceBytes = iso.GetBytes(text);
            byte[] encryptedBytes = SuiteB.Encrypt(keyBytes, sourceBytes);

            string encryptedString = Convert.ToBase64String(encryptedBytes);
            return encryptedString;
        }

        public static string Decrypt(string key, string text)
        {
            byte[] keyBytes = iso.GetBytes(key);
            byte[] sourceBytes = Convert.FromBase64String(text);
            byte[] decryptedBytes = SuiteB.Decrypt(keyBytes, sourceBytes);

            string decryptedString = iso.GetString(decryptedBytes);
            return decryptedString;
        }
    }
}
