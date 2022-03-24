
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class TemplateFormatKeyMapping
    {
        public List<MasterMessage> Messgaes { get; set; }
        public List<MessageTemplateKeyword> Keyswords { get; set; }
    }

    public class TemplateFormatKeyMappingDisplay
    {
        public int FormatID { get; set; }
        public string Messgae { get; set; }
        public List<MessageTemplateKeyword> Keyswords { get; set; }
    }
}
