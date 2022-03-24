using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetLeadServiceRequest : IProcedure
    {
        private readonly IDAL _dal;
        List<LeadServiceRequest> LeadServiceList = null;
        public ProcGetLeadServiceRequest(IDAL dal)
        {
            _dal = dal;

        }
        public object Call(object obj)
        {
            var _req = (LeadServiceRequest)obj;
            SqlParameter[] param =
            {
                 new SqlParameter("@LT",_req.LT),
                new SqlParameter("@OID",_req.OID),
                new SqlParameter("@FromDate",_req.FromDate),
                new SqlParameter("@ToDate",_req.ToDate),
                new SqlParameter("@Mobile",_req.Mobile),
                new SqlParameter("@ID",_req.ID),
                 new SqlParameter("@UserId",_req.UserID),
                new SqlParameter("@LoginId",_req.LoginID),
                new SqlParameter("@OutLetNo",_req.OutletNo),
                new SqlParameter("@TopRows",_req.TopRows),
               new SqlParameter("@RequestModeID",_req.RequestModeID)

            };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                LeadServiceList = new List<LeadServiceRequest>();
                if (dt.Rows.Count > 0)
                {

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (_req.ID == 0)
                        {
                            LeadServiceRequest leadservice = new LeadServiceRequest()
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                Name = dt.Rows[i]["_Name"].ToString(),
                                Mobile = dt.Rows[i]["_Mobile"].ToString(),
                                OpType = dt.Rows[i]["_OpType"].ToString(),
                                RequestIP = dt.Rows[i]["_RequestIP"].ToString(),
                                Browser = dt.Rows[i]["_Browser"].ToString(),
                                Operator = dt.Rows[i]["_Operator"].ToString(),
                                Remark = dt.Rows[i]["_Remark"].ToString(),
                                Outlet = dt.Rows[i]["_Outlet"].ToString(),
                                ModifiedUser = dt.Rows[i]["_ModifiedUser"].ToString(),
                                DateTime = dt.Rows[i]["_DateTime"].ToString(),
                                UpdatedDateTime = dt.Rows[i]["_UpdatedDateTime"].ToString(),
                                LeadSubType = dt.Rows[i]["_LeadSubType"].ToString(),
                                LeadStatus = Convert.ToInt32(dt.Rows[i]["_LeadStatus"]),
                                RequiredFor = dt.Rows[i]["_RequiredFor"].ToString(),
                                Amount = Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                                OpTypeID = Convert.ToInt32(dt.Rows[i]["_OpTypeID"]),
                                Comments = dt.Rows[i]["_Comments"].ToString(),
                                ActiveLoan = dt.Rows[i]["_ActiveLoan"].ToString(),
                                BankName = dt.Rows[i]["_BankName"].ToString(),
                                OccupationType = dt.Rows[i]["_OccupationType"].ToString(),
                                PinCode = dt.Rows[i]["_PinCode"].ToString(),
                                Email = dt.Rows[i]["_Email"].ToString(),
                                PAN = dt.Rows[i]["_PAN"].ToString(),
                                Age = Convert.ToInt32(dt.Rows[i]["_Age"]),
                                CustomerType = dt.Rows[i]["_CustomerType"].ToString(),
                                ComplaintRemark = dt.Rows[i]["_ComplainRemark"] is DBNull ? string.Empty : dt.Rows[i]["_ComplainRemark"].ToString(),
                                Address = dt.Rows[i]["_Address"] is DBNull ? string.Empty : dt.Rows[i]["_Address"].ToString()
                            };

                            LeadServiceList.Add(leadservice);
                        }
                        else
                        {
                            LeadServiceRequest leadservice = new LeadServiceRequest()
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                Name = dt.Rows[i]["_Name"].ToString(),
                                Mobile = dt.Rows[i]["_Mobile"].ToString(),
                                Email = dt.Rows[i]["_Email"].ToString(),
                                PAN = dt.Rows[i]["_PAN"].ToString(),
                                Age = Convert.ToInt32(dt.Rows[i]["_Age"]),
                                LoanType = dt.Rows[i]["_LoanType"].ToString(),
                                InsuranceType = dt.Rows[i]["_InsuranceType"].ToString(),
                                CustomerType = dt.Rows[i]["_CustomerType"].ToString(),
                                RequiredFor = dt.Rows[i]["_RequiredFor"].ToString(),
                                Amount = Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                                OpType = dt.Rows[i]["_OpType"].ToString(),
                                OpTypeID = Convert.ToInt32(dt.Rows[i]["_OpTypeID"]),
                                Comments = dt.Rows[i]["_Comments"].ToString(),
                                RequestIP = dt.Rows[i]["_RequestIP"].ToString(),
                                Browser = dt.Rows[i]["_Browser"].ToString(),
                                Operator = dt.Rows[i]["_Operator"].ToString(),
                                ActiveLoan =dt.Rows[i]["_ActiveLoan"].ToString(),
                                BankName = dt.Rows[i]["_BankName"].ToString(),
                                OccupationType= dt.Rows[i]["_OccupationType"].ToString(),
                                PinCode = dt.Rows[i]["_PinCode"].ToString(),
                                ComplaintRemark = dt.Rows[i]["_ComplainRemark"] is DBNull ? string.Empty : dt.Rows[i]["_ComplainRemark"].ToString(),
                                Address = dt.Rows[i]["_Address"] is DBNull ? string.Empty : dt.Rows[i]["_Address"].ToString()
                            };

                            LeadServiceList.Add(leadservice);
                        }

                    }




                }
                return LeadServiceList;
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return new LeadServiceRequest();
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetLeadServiceRequest";
    }
}
