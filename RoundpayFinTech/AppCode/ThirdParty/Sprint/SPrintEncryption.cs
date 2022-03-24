using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Sprint
{
    public class AES
    {
        public static string CryptAESIn(string textToCrypt, string crypt_key, string init_Vector)
        {
            try
            {
                byte[] cryptkey = Encoding.ASCII.GetBytes(crypt_key);
                byte[] initVector = Encoding.ASCII.GetBytes(init_Vector);
                using (var rijndaelManaged =
                       new RijndaelManaged { Key = cryptkey, IV = initVector, Mode = CipherMode.CBC })
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream =
                       new CryptoStream(memoryStream,
                           rijndaelManaged.CreateEncryptor(cryptkey, initVector),
                           CryptoStreamMode.Write))
                {
                    using (var ws = new StreamWriter(cryptoStream))
                    {
                        ws.Write(textToCrypt);
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            catch (CryptographicException e)
            {
                return "A Cryptographic error occurred: {0} " + e.Message;
            }
        }
        public static string DecryptAESIn(string cipherData, string crypt_key, string init_Vector)
        {
            try
            {
                byte[] cryptkey = Encoding.ASCII.GetBytes(crypt_key);
                byte[] initVector = Encoding.ASCII.GetBytes(init_Vector);
                using (var rijndaelManaged =
                       new RijndaelManaged { Key = cryptkey, IV = initVector, Mode = CipherMode.CBC })
                using (var memoryStream =
                       new MemoryStream(Convert.FromBase64String(cipherData)))
                using (var cryptoStream =
                       new CryptoStream(memoryStream,
                           rijndaelManaged.CreateDecryptor(cryptkey, initVector),
                           CryptoStreamMode.Read))
                {
                    return new StreamReader(cryptoStream).ReadToEnd();
                }
            }
            catch (CryptographicException e)
            {
                return "A Cryptographic error occurred: {0} " + e.Message;
            }
        }
        public static string EncryptTestWa(string plainText, string crypt_key, string init_Vector)
        {
            byte[] Key = Encoding.ASCII.GetBytes(crypt_key);
            byte[] IV = Encoding.ASCII.GetBytes(init_Vector);
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return Convert.ToBase64String(encrypted);
        }
        public static string DecryptTestWa(string cipher_Text, string crypt_key, string init_Vector)
        {
            byte[] cipherText = Encoding.ASCII.GetBytes(cipher_Text);
            byte[] Key = Encoding.ASCII.GetBytes(crypt_key);
            byte[] IV = Encoding.ASCII.GetBytes(init_Vector);
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }
        public static string EncryptStringToBytesAes(string plainText, string key_k, string iv_v, Int32 KeySize = 256)
        {
            Int32 blockSize = 128;
            byte[] key = Encoding.ASCII.GetBytes(key_k);
            byte[] iv = Encoding.ASCII.GetBytes(iv_v);

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            // Declare the stream used to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt;

            // Declare the RijndaelManaged object
            // used to encrypt the data.
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = KeySize, BlockSize = blockSize, Key = key, IV = iv };

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {

                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                        swEncrypt.Flush();
                        swEncrypt.Close();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            // Return the encrypted bytes from the memory stream.
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        public static string DecryptStringFromBytesAes(string cipherText_c, string key_k, string iv_v, Int32 KeySize = 256)
        {
            Int32 blockSize = 128;
            byte[] cipherText = Encoding.ASCII.GetBytes(cipherText_c);
            byte[] key = Encoding.ASCII.GetBytes(key_k);
            byte[] iv = Encoding.ASCII.GetBytes(iv_v);

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = KeySize, BlockSize = blockSize, Key = key, IV = iv };

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                            srDecrypt.Close();
                        }
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            return plaintext;
        }
    }
}
