using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class LeadService
    {
        public int LT { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string PAN { get; set; }
        public int LoanTypeID { get; set; }
        public int InsuranceTypeID { get; set; }
        public decimal Amount { get; set; }
        public int CustomerTypeID { get; set; }
        public string RequiredFor { get; set; }
        public string Comments { get; set; }
        public string Remark { get; set; }
        public int EntryBy { get; set; }
        public int ModifyBy { get; set; }
        public int RequestModeID { get; set; }
        public int OID { get; set; }
        public int LeadStatus { get; set; }
        public string LeadSubType {get;set;}
        public string Outlet { get; set; }
        public string ModifiedUser { get; set; }
        public string DateTime { get; set; }
        public string UpdatedDateTime { get; set; }
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public int BankID { get; set; }
        public bool HaveLoan { get; set; }
        public string OccupationType { get; set; }
        public string PinCode { get; set; }
        public string BankName { get; set; }
        public string ActiveLoan { get; set; }

        public string ComplaintRemark { get; set; }
        public string Address { get; set; }
    }
    public class LeadServiceRequest:LeadService
    {
        public int RequestMode { get; set; }
        public string RequestIP { get; set; }
        public string Browser { get; set; }
        public string LoanType { get; set; }
        public string CustomerType { get; set; }
        public string InsuranceType { get; set; }
        public int OpTypeID { get; set; }
        public string OpType { get; set; }
        public string Operator { get; set; }
        public List<LeadServiceRequest> Leadservicelst { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAPIUser { get; set; }
        public bool IsEndUser { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public string OutletNo { get; set; }
        public int TopRows { get; set; }
        public IEnumerable<OperatorDetail> Operatorlst { get; set; }
        public bool IsExport { get; set; }
        
    }
    public class LoanTypes 
    {
        public int ID { get; set; }
        public string LoanType { get; set; }
    }
    public class CustomerTypes 
    {
        public int ID { get; set; }
        public string CustomerType { get; set; }
    }
    public class InsuranceTypes
    {
        public int ID { get; set; }
        public string InsuranceType { get; set; }
    }
}
