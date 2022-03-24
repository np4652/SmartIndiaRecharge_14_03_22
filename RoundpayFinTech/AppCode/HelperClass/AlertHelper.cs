using System;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.HelperClass
{
    public class AlertHelper
    {
        private static Lazy<AlertHelper> Instance = new Lazy<AlertHelper>(() => new AlertHelper());
        public static AlertHelper O => Instance.Value;
        private AlertHelper() { }

        public IEnumerable<TemplateReplacement> GetTemplateReplacement(int FormatID) {
            var mTemplateReplacement = new List<TemplateReplacement>();

            return mTemplateReplacement;
        }
    }
    public class TemplateReplacement
    {
        public string ReplacementName { get; set; }
        public string Replacement { get; set; }
    }
}
