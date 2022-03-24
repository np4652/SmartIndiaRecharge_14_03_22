using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPSTReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPSTReport(IDAL dal) => _dal = dal;
        public string GetName() => "proc_PSTReport";
        public async Task<object> Call(object obj)
        {
            var _req = (CommonFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@FromDate", _req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@RoleID", _req.RoleID),
                new SqlParameter("@MobileNo", _req.MobileNo),
            };
            var _alist = new List<PSTReport>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new PSTReport
                    {
                        Statuscode = ErrorCodes.One,
                        Msg = "Success",
                        UserID = Convert.ToInt32(dr["_UserID"]),
                        UserName = Convert.ToString(dr["_Name"]),
                        OutletName = Convert.ToString(dr["_OutletName"]),
                        MobileNo = Convert.ToString(dr["_MobileNo"]),
                        RoleID = Convert.ToInt32(dr["_RoleID"]),
                        Prefix = Convert.ToString(dr["_Prefix"]),
                        PriAmount = dr["_PriAmount"] is DBNull ? 0 : Convert.ToDouble(dr["_PriAmount"]),
                        SecAmount = dr["_SecAmount"] is DBNull ? 0 : Convert.ToDouble(dr["_SecAmount"]),
                        Recharge = dr["_Recharge"] is DBNull ? 0 : Convert.ToDouble(dr["_Recharge"]),
                        MoneyTransfer = dr["_MoneyTransfer"] is DBNull ? 0 : Convert.ToDouble(dr["_MoneyTransfer"]),
                        BillPayment = dr["_BillPayment"] is DBNull ? 0 : Convert.ToDouble(dr["_BillPayment"]),
                        AEPS = dr["_AEPS"] is DBNull ? 0 : Convert.ToDouble(dr["_AEPS"]),
                        GenralInsurance = dr["_GenralInsurance"] is DBNull ? 0 : Convert.ToDouble(dr["_GenralInsurance"]),
                        Shopping = dr["_Shopping"] is DBNull ? 0 : Convert.ToDouble(dr["_Shopping"]),
                        EServices = dr["_EServices"] is DBNull ? 0 : Convert.ToDouble(dr["_EServices"]),
                        PSAService = dr["_PSAService"] is DBNull ? 0 : Convert.ToDouble(dr["_PSAService"]),
                        DTHSubscription = dr["_DTHSubscription"] is DBNull ? 0 : Convert.ToDouble(dr["_DTHSubscription"])
                    };
                    _alist.Add(item);
                }
            }
            catch (Exception er)
            {

            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();
    }
}
