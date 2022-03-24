using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RoundpayFinTech_bA.AppCode.ThirdParty.BillAvenue
{
    public class BA_AesCryptUtil
    {
        private readonly byte[] data;
        private readonly byte[] AesIV;
        public BA_AesCryptUtil(string Key)
        {
            data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Key));
            AesIV = new byte[16]
                    {
                      (byte) 0,
                      (byte) 1,
                      (byte) 2,
                      (byte) 3,
                      (byte) 4,
                      (byte) 5,
                      (byte) 6,
                      (byte) 7,
                      (byte) 8,
                      (byte) 9,
                      (byte) 10,
                      (byte) 11,
                      (byte) 12,
                      (byte) 13,
                      (byte) 14,
                      (byte) 15
                    };
        }
        public string encrypt(string strToEncrypt)
        {
            using (new RijndaelManaged())
            {
                byte[] bytes = EncryptStringToBytes(strToEncrypt, this.data, this.AesIV);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte num in bytes)
                    stringBuilder.AppendFormat("{0:x2}", (object)num);
                return stringBuilder.ToString();
            }
        }
        private static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
            {
                rijndaelManaged.Key = Key;
                rijndaelManaged.IV = IV;
                ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            streamWriter.Write(plainText);
                        return memoryStream.ToArray();
                    }
                }
            }
        }
        public string decrypt(string strToDecrypt)
        {
            using (new RijndaelManaged())
                return DecryptStringFromBytes(strToDecrypt, this.data, this.AesIV);
        }
        private static string DecryptStringFromBytes(string encryptedText, byte[] Key, byte[] IV)
        {
            int length = encryptedText.Length;
            byte[] buffer = new byte[length / 2];
            for (int startIndex = 0; startIndex < length; startIndex += 2)
                buffer[startIndex / 2] = Convert.ToByte(encryptedText.Substring(startIndex, 2), 16);
            if (buffer == null || buffer.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
            {
                rijndaelManaged.Key = Key;
                rijndaelManaged.IV = IV;
                ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}
