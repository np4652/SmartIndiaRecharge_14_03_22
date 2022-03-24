using paytm.exception;
using paytm.security;
using paytm.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace paytm
{
    public static class CheckSum
    {
        #region New Dll Broken
        public static string generateSignature(Dictionary<string, string> input, string key)
        {
            return CheckSum.generateSignature(CheckSum.getStringByParams(input), key);
        }

        public static string generateSignature(string input, string key)
        {
            try
            {
                CheckSum.validateGenerateCheckSumInput(key);
                StringBuilder stringBuilder = new StringBuilder(input);
                stringBuilder.Append("|");
                string randomString = CheckSum.generateRandomString(4);
                stringBuilder.Append(randomString);
                return CheckSum.encrypt(CheckSum.getHashedString(stringBuilder.ToString()) + randomString, key);
            }
            catch (Exception ex)
            {
                CheckSum.ShowException(ex);
                return (string)null;
            }
        }

        //public static bool verifySignature(
        //  Dictionary<string, string> input,
        //  string key,
        //  string CheckSum)
        //{
        //    return CheckSum.verifySignature(CheckSum.getStringByParams(input), key, CheckSum);
        //}

        public static bool verifySignature(string input, string key, string CheckSum)
        {
            try
            {
                validateVerifyCheckSumInput(CheckSum, key);
                string str1 = decrypt(CheckSum, key);
                if (str1 == null || str1.Length < 4)
                    return false;
                string str2 = str1.Substring(str1.Length - 4, 4);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(input);
                stringBuilder.Append("|");
                stringBuilder.Append(str2);
                return str1.Equals(getHashedString(stringBuilder.ToString()) + str2);
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return false;
            }
        }

        public static string encrypt(string input, string key)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(input);
                MemoryStream memoryStream = new MemoryStream();
                Rijndael rijndael = Rijndael.Create();
                rijndael.Key = Encoding.ASCII.GetBytes(key);
                rijndael.IV = new byte[16]
                {
          (byte) 64,
          (byte) 64,
          (byte) 64,
          (byte) 64,
          (byte) 38,
          (byte) 38,
          (byte) 38,
          (byte) 38,
          (byte) 35,
          (byte) 35,
          (byte) 35,
          (byte) 35,
          (byte) 36,
          (byte) 36,
          (byte) 36,
          (byte) 36
                };
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.Close();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                CheckSum.ShowException(ex);
                return (string)null;
            }
        }

        public static string decrypt(string input, string key)
        {
            try
            {
                byte[] buffer = Convert.FromBase64String(input);
                MemoryStream memoryStream = new MemoryStream();
                Rijndael rijndael = Rijndael.Create();
                rijndael.Key = Encoding.ASCII.GetBytes(key);
                rijndael.IV = new byte[16]
                {
          (byte) 64,
          (byte) 64,
          (byte) 64,
          (byte) 64,
          (byte) 38,
          (byte) 38,
          (byte) 38,
          (byte) 38,
          (byte) 35,
          (byte) 35,
          (byte) 35,
          (byte) 35,
          (byte) 36,
          (byte) 36,
          (byte) 36,
          (byte) 36
                };
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(buffer, 0, buffer.Length);
                cryptoStream.Close();
                return Encoding.ASCII.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                CheckSum.ShowException(ex);
                return (string)null;
            }
        }

        //private static void validateGenerateCheckSumInput(string key)
        //{
        //    if (key == null)
        //        throw new ArgumentNullException("Parameter cannot be null", "Specified key");
        //}

        private static void validateVerifyCheckSumInput(string CheckSum, string key)
        {
            if (key == null)
                throw new ArgumentNullException("Parameter cannot be null", "Specified key");
            if (CheckSum == null)
                throw new ArgumentNullException("Parameter cannot be null", "Specified CheckSum");
        }

        private static string getStringByParams(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                return "";
            SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>((IDictionary<string, string>)parameters, (IComparer<string>)StringComparer.Ordinal);
            StringBuilder stringBuilder = new StringBuilder("");
            foreach (KeyValuePair<string, string> keyValuePair in sortedDictionary)
            {
                string str = keyValuePair.Value ?? "";
                stringBuilder.Append(str).Append("|");
            }
            return stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
        }

        private static string generateRandomString(int length)
        {
            if (length <= 0)
                return "";
            Random random = new Random((int)DateTime.Now.Ticks);
            StringBuilder stringBuilder = new StringBuilder("");
            for (int index = 0; index < length; ++index)
            {
                int startIndex = random.Next("@#!abcdefghijklmonpqrstuvwxyz#@01234567890123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ#@".Length);
                stringBuilder.Append("@#!abcdefghijklmonpqrstuvwxyz#@01234567890123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ#@".Substring(startIndex, 1));
            }
            return stringBuilder.ToString();
        }

        private static string getHashedString(string inputValue)
        {
            return BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(inputValue))).Replace("-", "").ToLower();
        }

        private static void ShowException(Exception ex)
        {
            Console.WriteLine("Message : " + ex.Message + Environment.NewLine + "StackTrace : " + ex.StackTrace);
        }
        #endregion
        public static string generateCheckSum(string masterKey, Dictionary<string, string> parameters)
        {
            CheckSum.validateGenerateCheckSumInput(masterKey);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string result;
            try
            {
                foreach (string current in parameters.Keys)
                {
                    if (parameters[current].Trim().ToUpper().Contains("REFUND") || parameters[current].Trim().Contains("|"))
                    {
                        dictionary.Add(current.Trim(), "");
                    }
                    else
                    {
                        dictionary.Add(current.Trim(), parameters[current].Trim());
                    }
                }
                string text = SecurityUtils.createCheckSumString(dictionary);
                string str = StringUtils.generateRandomString(4);
                text += str;
                MessageConsole.WriteLine();
                MessageConsole.WriteLine("Final CheckSum String:::: " + text);
                string text2 = SecurityUtils.getHashedString(text);
                MessageConsole.WriteLine();
                MessageConsole.WriteLine("HashedCheckSum String:::: " + text2);
                text2 += str;
                MessageConsole.WriteLine();
                MessageConsole.WriteLine("HashedCheckSum String with Salt:::: " + text2);
                string text3 = Crypto.Encrypt(text2, masterKey);
                result = text3;
            }
            catch (Exception ex)
            {
                throw new CryptoException("Exception occurred while generating CheckSum. " + ex.Message);
            }
            return result;
        }

        public static string generateCheckSumForRefund(string masterKey, Dictionary<string, string> parameters)
        {
            CheckSum.validateGenerateCheckSumInput(masterKey);
            string result;
            try
            {
                string text = SecurityUtils.createCheckSumString(parameters);
                string str = StringUtils.generateRandomString(4);
                text += str;
                MessageConsole.WriteLine();
                MessageConsole.WriteLine("Final CheckSum String:::: " + text);
                string text2 = SecurityUtils.getHashedString(text);
                MessageConsole.WriteLine();
                MessageConsole.WriteLine("HashedCheckSum String:::: " + text2);
                text2 += str;
                MessageConsole.WriteLine();
                MessageConsole.WriteLine("HashedCheckSum String with Salt:::: " + text2);
                string text3 = Crypto.Encrypt(text2, masterKey);
                result = text3;
            }
            catch (Exception ex)
            {
                throw new CryptoException("Exception occurred while generating CheckSum. " + ex.Message);
            }
            return result;
        }

        public static string generateCheckSumByJson(string masterKey, string json)
        {
            CheckSum.validateGenerateCheckSumInput(masterKey);
            string result;
            try
            {
                string str = StringUtils.generateRandomString(4);
                string text = json + "|";
                text += str;
                string text2 = SecurityUtils.getHashedString(text);
                text2 += str;
                string text3 = Crypto.Encrypt(text2, masterKey);
                result = text3;
            }
            catch (Exception ex)
            {
                throw new CryptoException("Exception occurred while generating CheckSum. " + ex.Message);
            }
            return result;
        }

        public static bool verifyCheckSumByjson(string masterKey, string json, string CheckSum)
        {
            validateVerifyCheckSumInput(masterKey, CheckSum);
            bool result;
            try
            {
                string text = Crypto.Decrypt(CheckSum, masterKey);
                if (text == null || text.Length < 4)
                {
                    result = false;
                }
                else
                {
                    string str = text.Substring(text.Length - 4, 4);
                    MessageConsole.WriteLine("Salt:::: " + str);
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("Input CheckSum:::: " + CheckSum);
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("GeneratedCheckSum String:::: " + json);
                    string text2 = json + "|";
                    text2 += str;
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("GeneratedCheckSum String with Salt:::: " + text2);
                    string text3 = SecurityUtils.getHashedString(text2);
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("HashedGeneratedCheckSum String:::: " + text3);
                    text3 += str;
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("HashedGeneratedCheckSum String with Salt:::: " + text3);
                    result = text3.Equals(text);
                }
            }
            catch (Exception ex)
            {
                throw new CryptoException("Exception occurred while verifying CheckSum. " + ex.Message);
            }
            return result;
        }

        public static bool verifyCheckSum(string masterKey, Dictionary<string, string> parameters, string CheckSum)
        {
            validateVerifyCheckSumInput(masterKey, CheckSum);
            bool result;
            try
            {
                string text = Crypto.Decrypt(CheckSum, masterKey);
                if (text == null || text.Length < 4)
                {
                    result = false;
                }
                else
                {
                    string str = text.Substring(text.Length - 4, 4);
                    MessageConsole.WriteLine("Salt:::: " + str);
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("Input CheckSum:::: " + CheckSum);
                    string text2 = SecurityUtils.createCheckSumString(parameters);
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("GeneratedCheckSum String:::: " + text2);
                    text2 += str;
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("GeneratedCheckSum String with Salt:::: " + text2);
                    string text3 = SecurityUtils.getHashedString(text2);
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("HashedGeneratedCheckSum String:::: " + text3);
                    text3 += str;
                    MessageConsole.WriteLine();
                    MessageConsole.WriteLine("HashedGeneratedCheckSum String with Salt:::: " + text3);
                    result = text3.Equals(text);
                }
            }
            catch (Exception ex)
            {
                throw new CryptoException("Exception occurred while verifying CheckSum. " + ex.Message);
            }
            return result;
        }

        private static void validateGenerateCheckSumInput(string masterKey)
        {
            if (masterKey == null)
            {
                throw new ArgumentNullException("Parameter cannot be null", "masterKey");
            }
        }

        //private static void validateVerifyCheckSumInput(string masterKey, string CheckSum)
        //{
        //    if (masterKey == null)
        //    {
        //        throw new ArgumentNullException("Parameter cannot be null", "masterKey");
        //    }
        //    if (CheckSum == null)
        //    {
        //        throw new ArgumentNullException("Parameter cannot be null", "CheckSum");
        //    }
        //}

        public static string Encrypt(string CardDetails, string masterKey)
        {
            return Crypto.Encrypt(CardDetails, masterKey);
        }

        public static string Decrypt(string carddetails, string masterKey)
        {
            return Crypto.Decrypt(carddetails, masterKey);
        }
    }
}
