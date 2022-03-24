using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateBillerInfo : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateBillerInfo(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BillerInfoUpdateModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@BillerAdhoc",req.BillerAdhoc),
                new SqlParameter("@ExactNess",req.ExactNess),
                new SqlParameter("@BillerCoverage",req.BillerCoverage),
                new SqlParameter("@BillerName",req.BillerName),
                new SqlParameter("@IsBillValidation",req.IsBillValidation),
                new SqlParameter("@IsAmountInValidation",req.IsAmountInValidation),
                new SqlParameter("@BillerPaymentModes",req.BillerPaymentModes??string.Empty),
                new SqlParameter("@BillerAmountOptions",req.BillerAmountOptions??string.Empty),
                new SqlParameter("@tp_OperatorParams",req.tp_OperatorParams),
                new SqlParameter("@tp_OperatorPaymentChanel",req.tp_OperatorPaymentChanel),
                new SqlParameter("@BillFetchRequirement",req.BillFetchRequirement),
                new SqlParameter("@tp_OperatorDictionary",req.tp_OperatorDictionary)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError};
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
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
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UpdateBillerInfo";
    }
}
