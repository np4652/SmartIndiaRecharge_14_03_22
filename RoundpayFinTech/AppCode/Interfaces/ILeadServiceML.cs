using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.Interfaces
{
    interface ILeadServiceML
    {
        List<LoanTypes> GetLoanType();
        List<CustomerTypes> GetCustomerType();
        List<InsuranceTypes> GetInsuranceTypes();
        IResponseStatus SaveLeadService(LeadService req);
        IResponseStatus SaveLeadServiceApp(LeadServiceRequest req);
        List<LeadServiceRequest> GetLeadServiceRequest(LeadServiceRequest LeadReq);
        IResponseStatus UpdateLeadServiceRequest(int ID,string Remark, int LeadStatus);
        //IResponseStatus UpdateLeadServiceRequestApp(LeadServiceRequest _req);
        List<LeadServiceRequest> GetLeadDetailById(int ID);
    }


}
