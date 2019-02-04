using System;
using UpDiddyLib.Shared;
using Xunit;

namespace UpDiddyLib.Tests
{
    public class CryptoTests
    {
        public static string KEY = "Z4LshSKZpOf2p8ExvnVMm25&MiiQ2R0d";

        [Fact]
        public void EncryptsText()
        {
            string source = "password123";
            string encryptedText = Crypto.Encrypt(KEY, source);
            Assert.NotEqual(source, encryptedText);
        }

        [Fact]
        public void DecryptsText_SimplePassword()
        {
            string source = "password123";

            string encryptedText = Crypto.Encrypt(KEY, source);
            string decryptedText = Crypto.Decrypt(KEY, encryptedText);

            Assert.Equal(source, decryptedText);
        }

        [Fact]
        public void DecryptsText_PasswordWithSymbols()
        {
            string source = "N*leQ{|XPF0~`Ho*zuQ@i%'><>Z44p41zZI[]b3H/}";
            string encryptedText = Crypto.Encrypt(KEY, source);
            string decryptedText = Crypto.Decrypt(KEY, encryptedText);
            Assert.Equal(source, decryptedText);
        }
    }
}
