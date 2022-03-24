using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class procTotalPending : IProcedure
    {
        private readonly IDAL _dal;
        public procTotalPending(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public object Call()
        {
            var res = new Dashboard_Chart();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    res.DisputeCount = dt.Rows[0]["DisputeCount"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["DisputeCount"]);
                    res.DMRDisputeCount= Convert.ToInt16(dt.Rows[0]["DMRDisputeCount"]);
                    res.PCount = dt.Rows[0]["RechargeCount"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["RechargeCount"]);
                    res.DmrPCount = Convert.ToInt16(dt.Rows[0]["DMRCount"]);
                    res.KCount = Convert.ToInt16(dt.Rows[0]["KYCCount"]);
                    res.Dispute = Convert.ToDecimal(dt.Rows[0]["Dispute"]);
                    res.DMRDispute = Convert.ToDecimal(dt.Rows[0]["DMRDispute"]);
                    res.PAmount = Convert.ToDecimal(dt.Rows[0]["Recharge"]);
                    res.Dmr = Convert.ToDecimal(dt.Rows[0]["DMR"]);
                    res.MoveToBank = Convert.ToDecimal(dt.Rows[0]["MoveToBankRequest"]);
                    res.MTBCount = Convert.ToInt16(dt.Rows[0]["MTBCount"]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
           
        }

        public string GetName()
        {
            return "proc_Total_Pending";
        }
    }
}
