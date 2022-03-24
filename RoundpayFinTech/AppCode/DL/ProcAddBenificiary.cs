using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAddBenificiary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAddBenificiary(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BenificiaryDetail)obj;
            var res = new BenificiaryModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@SenderMobileNo", req._SenderMobileNo??""),
                new SqlParameter("@Name", req._Name??""),
                new SqlParameter("@AccountNumber", req._AccountNumber??""),
                new SqlParameter("@MobileNo", req._MobileNo??""),
                new SqlParameter("@IFSC", req._IFSC??""),
                new SqlParameter("@BankName", req._BankName??""),
                new SqlParameter("@Branch", req._Branch??""),
                new SqlParameter("@EntryBy", req._EntryBy),
                new SqlParameter("@VerifyStatus", req._VerifyStatus),
                new SqlParameter("@APICode", req._APICode??string.Empty),
                new SqlParameter("@BankID", req._BankID),
                new SqlParameter("@BeneAPIID", req._BeneAPIID),
                new SqlParameter("@CashFreeID", req._CashFreeID??string.Empty)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req._EntryBy
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AddBenificiary";
    }
}
