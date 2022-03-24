using System.Collections.Generic;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.StaticModel
{
    
    public class LastTransaction
    {
        public List<Status> Table { get; set; }
        public class Status
        {
            private string _blank = " ";
            private string _mobile;
            private string _apiName;
            private string _operator;
            private string _code;
            private string _requestId;
            private string _date;
            [XmlElement("Mobile")]
            public string Mobile { get => _mobile ?? _blank; set => _mobile = value; }
            [XmlElement("Amount")]
            public decimal Amount { get; set; }
            [XmlElement("ApiName")]
            public string ApiName { get => _apiName ?? _blank; set => _apiName = value; }
            [XmlElement("Operator")]
            public string Operator { get => _operator ?? _blank; set => _operator = value; }
            [XmlElement("Code")]
            public string Code { get => _code ?? _blank; set => _code = value; }
            [XmlElement("RequestId")]
            public string RequestId { get => _requestId ?? _blank; set => _requestId = value; }
            [XmlElement("Date")]
            public string Date { get => _date ?? _blank; set => _date = value; }

            private string _msg;
            private string _tid;

            [XmlElement("Message")]
            public string Message { get => _msg ?? _blank; set => _msg = value; }
            [XmlElement("TransactionID")]
            public string TransactionID { get => _tid ?? _blank; set => _tid = value; }
        }
    }
    public class LastTransactionSMS
    {
        public List<Status> Table { get; set; }
        public class Status
        {
            private string _blank = " ";
            private string _mobile;
            private string _apiName;
            private string _operator;
            private string _code;
            private string _requestId;
            private string _date;
            [XmlElement("Mobile")]
            public string Mobile { get => _mobile ?? _blank; set => _mobile = value; }
            [XmlElement("Message")]
            public string Message { get => _msg ?? _blank; set => _msg = value; }
            [XmlElement("Amount")]
            public decimal Amount { get; set; }
            [XmlElement("ApiName")]
            public string ApiName { get => _apiName ?? _blank; set => _apiName = value; }
            [XmlElement("Operator")]
            public string Operator { get => _operator ?? _blank; set => _operator = value; }
            [XmlElement("Code")]
            public string Code { get => _code ?? _blank; set => _code = value; }
            [XmlElement("RequestId")]
            public string RequestId { get => _requestId ?? _blank; set => _requestId = value; }
            [XmlElement("Date")]
            public string Date { get => _date ?? _blank; set => _date = value; }
            private string _msg;
            private string _tid;            
            [XmlElement("TransactionID")]
            public string TransactionID { get => _tid ?? _blank; set => _tid = value; }
        }        
    }
    //public class StatusSocial
    //{
    //    private string _blank = " ";
    //    private string _mobile;
    //    private string _msg;
    //    private string _tid;
    //    [XmlElement("Mobile")]
    //    public string Mobile { get => _mobile ?? _blank; set => _mobile = value; }
    //    [XmlElement("Message")]
    //    public string Message { get=> _msg??_blank; set=> _msg=value; }
    //    [XmlElement("TransactionID")]
    //    public string TransactionID { get => _tid ?? _blank; set => _tid = value; }        
    //}
}
