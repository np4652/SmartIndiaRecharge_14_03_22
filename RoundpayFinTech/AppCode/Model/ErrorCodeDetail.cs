using Fintech.AppCode.Model;

namespace RoundpayFinTech.AppCode.Model
{
    public class ErrorCodeDetail
    {
        public int EID { get; set; }
        public string Error { get; set; }
        public string Code { get; set; }
        public string ErrorWithCode { get; set; }
        public string Operator { get; set; }
        public int ErrType { get; set; }
        public string _ErrType { get; set; }
        public int Status { get; set; }
        public bool IsCode { get; set; }
        public string ModifyDate { get; set; }
        public bool IsDown { get; set; }
        public bool IsResend { get; set; }
    }

    public class ErrorCodeDetailReq : CommonReq
    {
        public ErrorCodeDetail Detail { get; set; }
    }

    public class APIErrorCode
    {
        public int ID { get; set; }
        public int EID { get; set; }
        public int GroupID { get; set; }
        public string GroupCode { get; set; }
        public string ECode { get; set; }
        public string APICode { get; set; }
        public string ModifyDate { get; set; }
        public int ErrorType { get; set; }
    }
    public class APIErrorCodeReq : CommonReq {
        public APIErrorCode APIErrorCode { get; set; }
    }
}
