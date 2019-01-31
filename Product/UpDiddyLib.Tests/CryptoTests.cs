using System;
using UpDiddyLib.Shared;
using Xunit;

namespace UpDiddyLib.Tests
{
    public class CryptoTests
    {
        public static string KEY = "5&CesNble%XjP#Dt";

        [Fact]
        public void EncryptsText()
        {
            string source = "password123";
            string key = "mykey";
            string encryptedText = Crypto.Encrypt(key, source);
            Assert.NotEqual(source, encryptedText);
        }

        [Fact]
        public void DecryptsText_SimplePassword()
        {
            string source = "password123";
            string key = "mykey";
            string encryptedText = Crypto.Encrypt(key, source);
            string decryptedText = Crypto.Decrypt(key, encryptedText);

            Assert.Equal(source, decryptedText);
        }

        [Fact]
        public void DecryptsText_PasswordWithSymbols()
        {
            string source = "N*leQ{|XPF0~`Ho*zuQ@i%'><>Z44p41zZI[]b3H/}";
            string encryptedText = Crypto.Encrypt(CryptoTests.KEY, source);
            string decryptedText = Crypto.Decrypt(CryptoTests.KEY, encryptedText);
            Assert.Equal(source, decryptedText);
        }
    }
}
