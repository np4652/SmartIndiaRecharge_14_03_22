using System;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class AAUserReq
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public int TopRows { get; set; }
        public int SearchType { get; set; }
        public int UserID { get; set; }
        public string Search { get; set; }
        public string MobileNo { get; set; }
    }

    public class AAUserData
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public List<AAUserList> AAUser { get; set; }
    }
    public class AAUserList
    {
        public int UserID { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string PAN { get; set; }
        public string AADHAR { get; set; }
        public bool Agreementstatus { get; set; }
        public int AgreementApprovedBy { get; set; }
        public string AgreementRemark { get; set; }
    }

    public class UpdateUserReq
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string PAN { get; set; }
        public string AADHAR { get; set; }
        public bool Agreementstatus { get; set; }
        public int AgreementApprovedBy { get; set; }
        public string AgreementRemark { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }

    public class AgreementApprovalNotification
    {
        public int UserID { get; set; }
        public string MobileNo { get; set; }
        public string Date { get; set; }
        public bool Status { get; set; }

        internal void Ad(AgreementApprovalNotification agreementApprovalNotification)
        {
            throw new NotImplementedException();
        }
    }

}
