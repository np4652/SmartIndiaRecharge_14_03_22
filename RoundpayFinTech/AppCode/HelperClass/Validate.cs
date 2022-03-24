using Fintech.AppCode.Configuration;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Validators
{
    public class Validate
    {
        public bool IsDecimal(object s) => Regex.IsMatch(Convert.ToString(s), @"^[0-9]+(\.?[0-9]?)") && !string.IsNullOrEmpty(Convert.ToString(s));
        
        public string ReplaceAllSpecials(string s) => Regex.Replace(s, "[^0-9A-Za-z]+", " ");
        public bool HasSpecialChar(string s) => Regex.IsMatch(s, "[^0-9A-Za-z ]+");
        public bool IsWhiteSpace(string s) => Regex.IsMatch(s, @"(\S)+");
        public bool IsNumeric(string s) => Regex.IsMatch(s, @"^[0-9]+$") && s != "";
        public bool IsAlphaNumeric(string s) => Regex.IsMatch(s, @"^[a-zA-Z0-9]*$") && s != "";
        public bool IsMobile(string s) => Regex.IsMatch(s, @"^([6-9]{1})([0-9]{9})$");

        public bool IsInternationalMobile(string s) => Regex.IsMatch(s, @"^(?=.*[0-9])[- +()0-9]+$");
        public bool IsStartsWithNumber(string s) => Regex.IsMatch(s, @"^\d+");
        public bool IsEndWithNumber(string s) => Regex.IsMatch(s, @"(\d+)$");
        public bool IsEmail(string s) => Regex.IsMatch(s, @"\A(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
        public bool IsPAN(string s) => Regex.IsMatch(s, @"^([a-zA-Z]){5}([0-9]){4}([a-zA-Z]){1}?$");
        public bool IsGSTIN(string s) => Regex.IsMatch(s, @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$");
        public bool IsAADHAR(string s) => Regex.IsMatch(s, @"^\d{4}\d{4}\d{4}$");
        public bool IsPANUpper(string s) => Regex.IsMatch(s, @"^([A-Z]){5}([0-9]){4}([A-Z]){1}?$");
        public bool IsPinCode(string s) => Regex.IsMatch(s, @"^\d{6}$");
        public bool IsValidBankAccountNo(string s) => (s.Length >= 9 && s.Length <= 18);
        public bool IsWebURL(string s) => Regex.IsMatch(s, @"^http(s)?://([\\w-]+.)+[\\w-]+(/[\\w- ./?%&=])?$");
        public bool IsTransactionIDValid(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            if (s[0] != Criteria.StartCharOFTransaction)
                return false;
            if (s.Length < 10)
                return false;
            return IsNumeric(s.Substring(1, 6));
        }
        public bool IsGeoCodeInValid(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;
            if (!s.Contains(","))
                return true;
            if (!s.Split(',')[0].Contains("."))
                return true;
            if (!s.Split(',')[1].Contains("."))
                return true;
            if (s.Split(',')[0].Split('.')[1].Length != 4)
                return true;
            if (s.Split(',')[1].Split('.')[1].Length != 4)
                return true;
            return false;
        }
        public bool IsLatitudeInValid(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;
            if (!s.Contains("."))
                return true;
            if (!IsNumeric(s.Split('.')[0].Replace("+", "").Replace("-", "")))
                return true;
            if (s.Split('.')[0].Length < 2 || s.Split('.')[0].Length >= 3)
                return true;
            if (s.Split('.')[1].Length != 4)
                return true;
            if (!IsNumeric(s.Split('.')[1]))
                return true;
            if (Convert.ToDecimal(s) > 90 || Convert.ToDecimal(s) < -90)
                return true;
            return false;
        }
        public bool IsLongitudeInValid(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;
            if (!s.Contains("."))
                return true;
            if (!IsNumeric(s.Split('.')[0].Replace("+", "").Replace("-", "")))
                return true;
            if (s.Split('.')[0].Length < 2 || s.Split('.')[0].Length > 4)
                return true;
            if (s.Split('.')[1].Length != 4)
                return true;
            if (!IsNumeric(s.Split('.')[1]))
                return true;
            if (Convert.ToDecimal(s) > 180 || Convert.ToDecimal(s) < -180)
                return true;
            return false;
        }
        public bool IsDateIn_dd_MMM_yyyy_Format(string s, char seprator = ' ')
        {
            if (string.IsNullOrEmpty(s))
                return false;
            if (!s.Contains(seprator + ""))
                return false;
            var sArr = s.Split(seprator);
            if (sArr.Length != 3)
                return false;
            if (sArr[0].Length != 2 || !IsNumeric(sArr[0]))
                return false;
            if (Convert.ToInt32(sArr[0]) > 31 || Convert.ToInt32(sArr[0]) < 0)
                return false;
            if (sArr[1].Length != 3 || IsNumeric(sArr[1]) || (!Months.Contains(sArr[1]) && !MonthsCaps.Contains(sArr[1])))
                return false;
            if (sArr[2].Length != 4 || !IsNumeric(sArr[2]))
                return false;

            return true;
        }
        public string[] Months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public string[] MonthsCaps = new string[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
        public string DTYB(string s)
        {
            string m = s.Substring(3, 2);
            int i = IsNumeric(m) ? Convert.ToInt32(m) : 1;
            return s.Replace("-" + m + "-", "-" + Months[i - 1] + "-");
        }
        private string[] LowerMonth => Months.Select(s => s.ToLower()).ToArray();
        public bool IsFileExecutable(byte[] FileContent)
        {
            if (FileContent == null)
                return false;
            byte[] newByteArr = GetSubBytes(FileContent, 0, 2);
            return Encoding.UTF8.GetString(newByteArr).ToUpper() == "MZ" || Encoding.UTF8.GetString(newByteArr).ToUpper() == "ZM";
        }
        private byte[] GetSubBytes(byte[] oldBytes, int start, int len)
        {
            if (oldBytes.Length >= len && start > -1 && start < len)
            {
                byte[] newByteArr = new byte[len];
                Array.Copy(oldBytes, start, destinationArray: newByteArr, destinationIndex: 0, length: len);
                return newByteArr;
            }
            return null;
        }
        public bool IsFileAllowed(byte[] fileContent, string ext)
        {
            if (fileContent == null)
                return false;
            if (string.IsNullOrEmpty(ext))
                return false;
            if (!ext.ToUpper().In(FileFormatsAllowed))
                return false;
            var SubByte = GetSubBytes(fileContent, 0, 20);
            string Start20BytesStr = SubByte?.Length > 0 ? Encoding.UTF8.GetString(SubByte) : "";
            if (Start20BytesStr.Length < 1)
                return false;
            if (!CheckFFSignature.Any(Start20BytesStr.ToUpper().Contains))
                return false;
            return true;
        }
        public string GetIfAllowedExtensionIsExists(byte[] fileContent,string expected="")
        {
            string s = string.Empty;
            var SubByte = GetSubBytes(fileContent, 0, 20);
            string Start20BytesStr = SubByte?.Length > 0 ? Encoding.UTF8.GetString(SubByte) : "";
            var signatures = CheckFFSignature.Where(x => Start20BytesStr.ToUpper().Contains(x));
            if (signatures != null)
            {
                s = signatures.Any() ? signatures.ElementAt(0).ToUpper() : string.Empty;
                if (!string.IsNullOrEmpty(s))
                {
                    if (s.Equals("JPG") && (string.IsNullOrEmpty(expected) || s.Equals(expected)))
                        return FileFormatsAllowed.ElementAt(2);
                    if (s.Equals("JPEG") || s.Equals("JFIF") || s.Equals("EXIF") && (string.IsNullOrEmpty(expected) || s.Equals(expected)))
                        return FileFormatsAllowed.ElementAt(1);
                    if (s.Equals("WEBP") && (string.IsNullOrEmpty(expected) || s.Equals(expected)))
                        return FileFormatsAllowed.ElementAt(0);
                    if (s.Equals("GIF") && (string.IsNullOrEmpty(expected) || s.Equals(expected)))
                        return FileFormatsAllowed.ElementAt(5);
                    if (s.Equals("%PDF") && (string.IsNullOrEmpty(expected) || s.Equals(expected)))
                        return FileFormatsAllowed.ElementAt(6);
                    if (s.Equals("PK") && (string.IsNullOrEmpty(expected) || s.Equals(expected)))
                        return FileFormatsAllowed.ElementAt(4);
                    if (s.Equals("PNG") && (string.IsNullOrEmpty(expected) || s.Equals(expected)))
                        return FileFormatsAllowed.ElementAt(3);
                    return string.Empty;
                }
            }
            return s;
        }

        public bool IsValidAPK(byte[] fileContent) {
            var SubByte = GetSubBytes(fileContent, 0, 20);
            string Start20BytesStr = SubByte?.Length > 0 ? Encoding.UTF8.GetString(SubByte) : "";
            return Start20BytesStr.ToUpper().StartsWith("PK");
        }
        public long CalculateSizeOfBase64File(string s)
        {
            var IsEndWithDoubleEqual = s.Substring(s.Length - 3, 2).Equals("==") ? true : false;
            return (s.Length * 3 / 4) - (IsEndWithDoubleEqual ? 2 : 1);
        }
        public byte[] TryToConnvertBase64String(string s)
        {
            byte[] b = null;
            try
            {
                b = Convert.FromBase64String(s);
            }
            catch (Exception)
            {
            }
            return b;
        }
        public ResponseStatus IsImageValid(IFormFile file)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (file != null)
            {
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename).ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                {
                    byte[] filecontent = null;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        filecontent = ms.ToArray();
                    }
                    if (!Validate.O.IsFileAllowed(filecontent, ext))
                    {
                        res.Msg = "Invalid File Format!";
                        return res;
                    }
                    else if (!file.ContentType.Any())
                        res.Msg = "File not found!";
                    else if (file.Length < 1)
                        res.Msg = "Empty file not allowed!";
                    else if (file.Length / 1024 > 1024)
                        res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    else
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = "its a valid Image";
                    }
                }
                else
                {
                    res.Msg = "Only png, jpg, jpeg are allowed to be uploaded";
                }
            }
            else
            {
                res.Msg = "No Image Found";
            }
            return res;
        }
        public static Validate O => instance.Value;
        private static Lazy<Validate> instance = new Lazy<Validate>(() => new Validate());
        private Validate() { }
        public readonly IEnumerable<string> FileFormatsAllowed = new List<string> { ".WEBP", ".JPEG", ".JPG", ".PNG", ".DOCX", ".GIF", ".PDF" };
        private IEnumerable<string> CheckFFSignature = new List<string> { "RIFF", "EXIF", "JPG", "JPEG", "JFIF", "PNG", "GIF", "%PDF", "PK", "GIF", "WEBP" };
        public string MaskNumeric(string s, string replacewith) => Regex.Replace(s, "[0-9]", replacewith);
        public string Prefix(string s, int PLen = 2)
        {
            return !string.IsNullOrEmpty(s) && !IsNumeric(s ?? "") ? s.Substring(0, PLen) : "";
        }

        public string LoginID(string s, int PLen = 2) => !string.IsNullOrEmpty(s) ? s.Substring(PLen, s.Length - PLen) : "0";

        public bool ValidateJSON(string s)
        {
            try
            {
                JToken.Parse(s);
                return true;
            }
            catch { }
            return false;
        }
        public string MaskAadhar(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            if (s.Length != 12)
                return s;
            return s.Replace(s.Substring(2, 6), "XXXXXX");
        }

        public bool IsUserAdult(int CurrentYear, int BirthYear)
        {
            var bYear = BirthYear - 100;
            var cYear = CurrentYear - 18;
            if (bYear <= BirthYear && BirthYear <= cYear)
                return true;
            else
                return false;
        }

        public string PaymentMODEMTTM(int p)
        {
            if (p == 2)
                return "RTGS";
            if (p == 1)
                return "NEFT";
            return "IMPS";
        }
    }
}
