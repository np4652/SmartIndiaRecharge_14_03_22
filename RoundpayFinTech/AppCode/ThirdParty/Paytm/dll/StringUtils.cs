using System;
using System.Text;

namespace paytm.util
{
    internal class StringUtils
    {
        private const string CHARACTER_SET = "@#!abcdefghijklmonpqrstuvwxyz#@01234567890123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ#@";

        private static Random random = new Random((int)DateTime.Now.Ticks);

        public static string generateRandomString(int length)
        {
            if (length <= 0)
            {
                return "";
            }
            StringBuilder stringBuilder = new StringBuilder("");
            for (int i = 0; i < length; i++)
            {
                int startIndex = StringUtils.random.Next("@#!abcdefghijklmonpqrstuvwxyz#@01234567890123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ#@".Length);
                stringBuilder.Append("@#!abcdefghijklmonpqrstuvwxyz#@01234567890123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ#@".Substring(startIndex, 1));
            }
            return stringBuilder.ToString();
        }

        public static string getStringFromBytes(byte[] byteArr)
        {
            if (Constants.USE_UNICODE_ENCODING)
            {
                return Encoding.Unicode.GetString(byteArr);
            }
            return Encoding.ASCII.GetString(byteArr);
        }

        public static byte[] getBytesFromString(string strInput)
        {
            if (Constants.USE_UNICODE_ENCODING)
            {
                return Encoding.Unicode.GetBytes(strInput);
            }
            return Encoding.ASCII.GetBytes(strInput);
        }
    }
}
