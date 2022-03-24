using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFetchBillActive : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFetchBillActive(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BBPSLog)obj;

            if (req.helper == null)
            {
                req.helper = new BBPSLogReqHelper();
            }
            if (req.helper.tpBFAInfo == null)
            {
                req.helper.tpBFAInfo = new System.Data.DataTable();
                req.helper.tpBFAInfo.Columns.Add("InfoName", typeof(string));
                req.helper.tpBFAInfo.Columns.Add("InfoValue", typeof(string));
                req.helper.tpBFAInfo.Columns.Add("Ind", typeof(int));
            }
            if (req.helper.tpBFAmountOps == null)
            {
                req.helper.tpBFAmountOps = new System.Data.DataTable();
                req.helper.tpBFAmountOps.Columns.Add("AmountKey", typeof(string));
                req.helper.tpBFAmountOps.Columns.Add("AmountValue", typeof(string));
                req.helper.tpBFAmountOps.Columns.Add("Ind", typeof(int));
            }
            if (req.helper.tpBFInputParam == null)
            {
                req.helper.tpBFInputParam = new System.Data.DataTable();
                req.helper.tpBFInputParam.Columns.Add("ParamName", typeof(string));
                req.helper.tpBFInputParam.Columns.Add("ParamValue", typeof(string));
                req.helper.tpBFInputParam.Columns.Add("Ind", typeof(int));
            }

            SqlParameter[] param = {
                new SqlParameter("@BillNumber",req.BillNumber??string.Empty),
                new SqlParameter("@BillDate",req.BillDate??string.Empty),
                new SqlParameter("@DueDate",req.DueDate??string.Empty),
                new SqlParameter("@Amount",req.Amount),
                new SqlParameter("@CustomerName",req.CustomerName??string.Empty),
                new SqlParameter("@AccountNumber",req.AccountNumber??string.Empty),
                new SqlParameter("@OPID",req.OPID??string.Empty),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@RequestURl",req.RequestURL??string.Empty),
                new SqlParameter("@Request",req.Request??string.Empty),
                new SqlParameter("@Response",req.Response??string.Empty),
                new SqlParameter("@ApiID",req.APIID),
                new SqlParameter("@SessionNo",req.SessionNo??string.Empty),
                new SqlParameter("@BillPeriod",req.BillPeriod??string.Empty),
                new SqlParameter("@RefferenceID",req.helper.RefferenceID??string.Empty),
                new SqlParameter("@tp_BFAInfo",req.helper.tpBFAInfo),
                new SqlParameter("@tp_BFAmountOps",req.helper.tpBFAmountOps),
                new SqlParameter("@tp_BFInputParam",req.helper.tpBFInputParam),
                new SqlParameter("@EarlyPaymentAmount",req.helper.EarlyPaymentAmount),
                new SqlParameter("@LatePaymentAmount",req.helper.LatePaymentAmount),
                new SqlParameter("@EarlyPaymentDate",req.helper.EarlyPaymentDate??string.Empty),
                new SqlParameter("@Status",req.helper.Status),
                new SqlParameter("@Reason",req.helper.Reason??string.Empty),
                new SqlParameter("@APIContext",req.APIContext??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonInt = dt.Rows[0]["FetchBillID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["FetchBillID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_FetchBillActive";
    }
}
