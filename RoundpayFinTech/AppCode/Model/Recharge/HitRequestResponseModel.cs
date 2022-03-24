using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Recharge
{
    public class HitRequestResponseModel
    {
        public string RequestPre { get; set; }
        public string ResponsePre { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public bool IsValidaionResponseAlso { get; set; }
        public string RequestV { get; set; }
        public string ResponseV { get; set; }
        public bool IsBillValidated { get; set; }
        public bool IsException { get; set; }
    }
    public class BBPSComplainReport
    {
        public int ID { get; set; }
        public int ComplainType { get; set; }
        public string ParticipationType { get; set; }
        public string TransactionID { get; set; }
        public string ComplainID { get; set; }
        public string ComplainTypeID { get; set; }
        public string ComplainDate { get; set; }
        public int ComplainStatus { get; set; }
        public string Remark{ get; set; }
        public string API{ get; set; }
    }
    public class BBPSComplainRequest
    {
        public int ComplainType { get; set; }
        public string ParticipationType { get; set; }
        public string APIOutletID { get; set; }
        public string VendorID { get; set; }
        public string BillerID { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }
        public string MobileNo { get; set; }
        public string TransactionID { get; set; }
        public string ComplaintID { get; set; }
    }
    public class BBPSCpmplainResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string LiveID { get; set; }
        public string Remark { get; set; }
    }

    public class GenerateBBPSComplainProcReq
    {
        public int UserID { get; set; }
        public int StoreOutletID { get; set; }
        public int OutletID { get; set; }
        public int ComplainType { get; set; }
        public int TransactionDoneAt { get; set; }
        public string TransactionID { get; set; }
        public string ParticipationType { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string MobileNo { get; set; }
        public int OID { get; set; }
    }
    public class GenerateBBPSComplainProcResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode { get; set; }
        public string APIOutletID { get; set; }
        public string APICode { get; set; }
        public string VendorID { get; set; }
        public string BillerID { get; set; }
        public string TransactionID { get; set; }
        public int TableID { get; set; }
        public int WID { get; set; }
        public int ComplainStatus { get; set; }
        public string ComplainAssignedTo { get; set; }
        public string ComplainReason { get; set; }
        public string ComplainStatus_ { get; set; }
        public string ReferenceID { get; set; }
    }
    public class BBPSComplainAPIResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string ComplainAssignedTo { get; set; }
        public string ComplainReason { get; set; }
        public string ComplainStatus { get; set; }
        public int ErrorCode{ get; set; }
        public string LiveID { get; set; }
        public string NPCIRefID { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public int TableID { get; set; }
        public int UserID { get; set; }
        public string Remark{ get; set; }
        public string StatusAsOn{ get; set; }
        public bool ShouldSerializeRequest() => (false);
        public bool ShouldSerializeResponse() => (false);
        public bool ShouldSerializeTableID() => (false);
        public bool ShouldSerializeUserID() => (false);
    }

}
