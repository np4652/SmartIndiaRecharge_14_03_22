using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class OperatorParamModels
    {
        public List<OperatorOptional> operatorOptionals { get; set; }
        public List<OperatorParam> operatorParams { get; set; }
        public List<OperatorOptionalDictionary> OpOptionalDic { get; set; }
    }
    public class AccOpenSetting : OperatorOptionalReq
    {

        public string Content { get; set; }
        public string RedirectURL { get; set; }
    }
    public class ReqAccOpenSetting
    {
        public WebsiteInfo _path { get; set; }
        public AccOpenSetting AccList { get; set; }

    }
    public class OperatorParam
    {
        public int ID { get; set; }
        public string OperatorName { get; set; }
        public string DropDown { get; set; }
        public string ParamName { get; set; }
        public string DataType { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public int Ind { get; set; }
        public string RegEx { get; set; }
        public string Remark { get; set; }
        public bool IsOptional { get; set; }
        public bool IsDropDown { get; set; }
        public bool IsCustomerNo { get; set; }
    }
    public class OperatorOptional
    {
        public int ID { get; set; }
        public int OID { get; set; }
        public int OptionalType { get; set; }
        public string DisplayName { get; set; }
        public string Remark { get; set; }
        public bool IsList { get; set; }
        public bool IsMultiSelection { get; set; }
    }
    public class OperatorOptionalReq : OperatorOptional
    {
        public int LoginID { get; set; }
        public int LT { get; set; }
    }
    public class AccountOpData {
        public int OID { get; set; }
        public string RedirectURL { get; set; }
        public string Content { get; set; }
        public string Name { get; set; }
    }
}
