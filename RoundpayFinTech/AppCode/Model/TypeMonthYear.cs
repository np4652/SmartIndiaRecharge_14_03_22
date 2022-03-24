using Fintech.AppCode.Interfaces;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Fintech.AppCode.Model
{
    public class TypeMonthYear
    {
        public int ConType { get; set; }
        public string MM { get; set; }
        public string YYYY { get; set; }
    }
    public class ConnectionStringType {
        public static int DBCon = 0;
        public static int DBCon_Month = 1;
        public static int DBCon_Old = 2;
    }
}
