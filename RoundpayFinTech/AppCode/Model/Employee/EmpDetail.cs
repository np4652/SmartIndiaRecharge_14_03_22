
namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class EmpInfo
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int EmpID { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public string EmpCode { get; set; }
        public string EmailID { get; set; }
        public string Aadhar { get; set; }
        public string Address { get; set; }
        public string PAN { get; set; }
        public int RoleID { get; set; }
        public string Pincode { get; set; }
        public string Token { get; set; }
        public short RequestModeID { get; set; }
    }
    public class EmpRequest
    {
        public int LoginID { get; set; }
        public int LTID { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        
    }
    public class EmpReq: EmpRequest
    {
        public string MobileNo { get; set; }
        public int EmpID { get; set; }
    }

    public class EmpDetailRequest: EmpRequest
    {
        public EmpInfo empInfo { get; set; }
        public int ReferalID { get; set; }
    }
}
