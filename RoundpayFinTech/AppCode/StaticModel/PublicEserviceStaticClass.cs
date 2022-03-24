using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.StaticModel
{
    public enum PESFieldType
    {
        Zero = 0,
        Input = 1,
        DropDown = 2,
        TextArea = 3,
        PINCodeOnly=4,
        PINCodeWithState=5,
        State=6,
        City=7,
    }
    public static class FieldInputType
    {
        public const string text = "text";
        public const string number = "number";
        public const string password = "password";
        public const string email = "email";
        public const string date = "date";
        public const string hidden = "hidden";
        public const string tel = "tel";
        public const string time = "time";
        public const string url = "url";
        public const string week = "week";
        public const string file = "file";
    }
}