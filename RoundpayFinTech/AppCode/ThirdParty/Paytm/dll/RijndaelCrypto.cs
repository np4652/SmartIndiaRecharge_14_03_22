using paytm.util;
using System;
using System.IO;
using System.Security.Cryptography;

namespace paytm.security
{
    internal class RijndaelCrypto
    {
        private static bool USE_CUSTOM_INIT_VECTOR = true;

        public static string Encrypt(string clearText, string masterKey)
        {
            byte[] bytesFromString = StringUtils.getBytesFromString(clearText);
            byte[] bytesFromString2 = StringUtils.getBytesFromString(masterKey);
            var passwordDeriveBytes = new PasswordDeriveBytes(masterKey, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118
            });
            byte[] iV;
            if (USE_CUSTOM_INIT_VECTOR)
            {
                iV = Constants.CRYPTO_INIT_VECTOR;
            }
            else
            {
                iV = passwordDeriveBytes.GetBytes(16);
            }
            byte[] inArray = Encrypt(bytesFromString, bytesFromString2, iV);
            return Convert.ToBase64String(inArray);
        }

        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream memoryStream = new MemoryStream();
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = Key;
            rijndael.IV = IV;
            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(clearData, 0, clearData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        public static string Decrypt(string cipherText, string masterKey)
        {
            byte[] cipherData = Convert.FromBase64String(cipherText);
            byte[] bytesFromString = StringUtils.getBytesFromString(masterKey);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(masterKey, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118
            });
            byte[] iV;
            if (USE_CUSTOM_INIT_VECTOR)
            {
                iV = Constants.CRYPTO_INIT_VECTOR;
            }
            else
            {
                iV = passwordDeriveBytes.GetBytes(16);
            }
            byte[] byteArr = Decrypt(cipherData, bytesFromString, iV);
            return StringUtils.getStringFromBytes(byteArr);
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            MemoryStream memoryStream = new MemoryStream();
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = Key;
            rijndael.IV = IV;
            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(cipherData, 0, cipherData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }
    }
}
