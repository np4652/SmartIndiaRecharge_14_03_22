using System;
using System.Security.Cryptography;
using System.Text;

namespace RoundpayFinTech.AppCode.ThirdParty.Eko
{
    public class EKOAPIHelper
    {
        public const string SecretKey = "secret-key";
        public const string SecretKeyTimeStamp = "secret-key-timestamp";
        public const string DeveloperKey = "developer_key";
        private static Lazy<EKOAPIHelper> Instance = new Lazy<EKOAPIHelper>(() => new EKOAPIHelper());
        public static EKOAPIHelper O => Instance.Value;
        private EKOAPIHelper() { }

        public string GetSecretKeyTimeStamp()
        {
            //return "1516705204593";
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        }

        public string GetSecretKey(string CurrentTimeStamp, string EkoKey)
        {
            //return "MC6dKW278tBef+AuqL/5rW2K3WgOegF0ZHLW/FriZQw=";
            var Encodedkey = Convert.ToBase64String(Encoding.UTF8.GetBytes(EkoKey));
            #region CalcHMACSHA256Hash
            var asciiEncoding = new ASCIIEncoding();
            var EncodedKeyBytes = asciiEncoding.GetBytes(Encodedkey);
            var CurrentTimeStampBytes = asciiEncoding.GetBytes(CurrentTimeStamp);
            using (var hmcsha256 = new HMACSHA256(EncodedKeyBytes))
            {
                return Convert.ToBase64String(hmcsha256.ComputeHash(CurrentTimeStampBytes));
            }
            #endregion
        }
        //public string GetURL()
        //{
        //    //return "https://staging.eko.in:25004/ekoapi/v1/";
        //    return "https://api.eko.co.in:25002/ekoicici/v1/";
        //}
        
    }
}
