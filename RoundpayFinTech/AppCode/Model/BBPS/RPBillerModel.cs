using System;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.BBPS
{
    public class RPBillerModel
    {
        public List<RPBiller> billerList { get; set; }
        public List<RPBillerParam> billerParamList { get; set; }
        public List<RPBillerPaymentChanel> billerPaymentChanel { get; set; }
        public List<RPBillerOperatorDictionary> billerOperatorDictionary { get; set; }
    }
    public class RPBiller
    {
        public int OID { get; set; }
        public string Name { get; set; }
        public string OPID { get; set; }
        public int OpTypeID { get; set; }
        public string OpType { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsBilling { get; set; }
        public string AccountName { get; set; }
        public string AccountRemark { get; set; }
        public bool IsAccountNumeric { get; set; }
        public string RPBillerID { get; set; }
        public string BillerAmountOptions { get; set; }
        public bool BillerAdhoc { get; set; }
        public int ExactNess { get; set; }
        public string BillerCoverage { get; set; }
        public string BillerName { get; set; }
        public string AccountNoKey { get; set; }
        public bool IsBillValidation { get; set; }
        public string BillerPaymentModes { get; set; }
        public string RegExAccount { get; set; }
        public bool IsAmountOptions { get; set; }
        public string EarlyPaymentAmountKey { get; set; }
        public string LatePaymentAmountKey { get; set; }
        public string EarlyPaymentDateKey { get; set; }
        public string BillMonthKey { get; set; }
        public bool IsAmountValidation { get; set; }
    }
    public class RPBillerParam
    {
        public int ID { get; set; }
        public int OID { get; set; }
        public string ParamName { get; set; }
        public string DataType { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string RegEx { get; set; }
        public bool IsAccountNo { get; set; }
        public bool IsOptional { get; set; }
        public bool IsActive { get; set; }
        public bool IsCustomerNo { get; set; }
        public bool IsDropDown { get; set; }
        public string Remark { get; set; }
    }
    public class RPBillerPaymentChanel
    {
        public int ID { get; set; }
        public int OID { get; set; }
        public string PaymentChanel { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
    }

    public class RPBillerOperatorDictionary
    {
        public int ParamID { get; set; }
        public int Ind { get; set; }
        public int OID { get; set;}
        public string DropDownValue { get; set; }
      
    }
}
