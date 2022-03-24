using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLogFetchBill : IProcedure
    {
        private readonly IDAL _dal;
        public ProcLogFetchBill(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (BBPSLog)obj;
            if (req.helper == null)
            {
                req.helper = new BBPSLogReqHelper();
            }
            SqlParameter[] param = {
                new SqlParameter("@BillNumber", req.BillNumber??string.Empty),
                new SqlParameter("@BillDate",req.BillDate??string.Empty),
                new SqlParameter("@DueDate", req.DueDate??string.Empty),
                new SqlParameter("@Amount", req.Amount),
                new SqlParameter("@CustomerName", req.CustomerName??string.Empty),
                new SqlParameter("@AccountNumber", req.AccountNumber??string.Empty),
                new SqlParameter("@OPID", req.OPID??string.Empty),
                new SqlParameter("@UserID", req.UserID),
                new SqlParameter("@ApiID", req.APIID),
                new SqlParameter("@RequestURl", req.RequestURL??string.Empty),
                new SqlParameter("@Request", req.Request??string.Empty),
                new SqlParameter("@Response", req.Response??string.Empty),
                new SqlParameter("@SessionNo", req.SessionNo??string.Empty),
                new SqlParameter("@BillPeriod", req.BillPeriod??string.Empty),
                new SqlParameter("@AmountName1",req.helper.AmountName1??string.Empty),
                new SqlParameter("@AmountValue1",req.helper.AmountValue1??string.Empty),
                new SqlParameter("@AmountName2",req.helper.AmountName2??string.Empty),
                new SqlParameter("@AmountValue2",req.helper.AmountValue2??string.Empty),
                new SqlParameter("@AmountName3",req.helper.AmountName3??string.Empty),
                new SqlParameter("@AmountValue3",req.helper.AmountValue3??string.Empty),
                new SqlParameter("@AmountName4",req.helper.AmountName4??string.Empty),
                new SqlParameter("@AmountValue4",req.helper.AmountValue4??string.Empty),
                new SqlParameter("@InfoName1",req.helper.InfoName1??string.Empty),
                new SqlParameter("@InfoValue1",req.helper.InfoValue1??string.Empty),
                new SqlParameter("@InfoName2",req.helper.InfoName2??string.Empty),
                new SqlParameter("@InfoValue2",req.helper.InfoValue2??string.Empty),
                new SqlParameter("@InfoName3",req.helper.InfoName3??string.Empty),
                new SqlParameter("@InfoValue3",req.helper.InfoValue3??string.Empty),
                new SqlParameter("@InfoName4",req.helper.InfoName4??string.Empty),
                new SqlParameter("@InfoValue4",req.helper.InfoValue4??string.Empty),
                new SqlParameter("@Param1",req.helper.Param1??string.Empty),
                new SqlParameter("@ParamValue1",req.helper.ParamValue1??string.Empty),
                new SqlParameter("@Param2",req.helper.Param2??string.Empty),
                new SqlParameter("@ParamValue2",req.helper.ParamValue2??string.Empty),
                new SqlParameter("@Param3",req.helper.Param3??string.Empty),
                new SqlParameter("@ParamValue3",req.helper.ParamValue3??string.Empty),
                new SqlParameter("@Param4",req.helper.Param4??string.Empty),
                new SqlParameter("@ParamValue4",req.helper.ParamValue4??string.Empty),
                new SqlParameter("@Param5",req.helper.Param5??string.Empty),
                new SqlParameter("@ParamValue5",req.helper.ParamValue5??string.Empty),
                new SqlParameter("@RefferenceID",req.helper.RefferenceID??string.Empty)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0] is DBNull ? ErrorCodes.Minus1 : dt.Rows[0][0]);
                    res.Msg = dt.Rows[0][1] is DBNull ? string.Empty : dt.Rows[0][1].ToString();
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
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_FetchBill";
    }
}
