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
    public class ProcUpdateFintechBillerInfoNotExistent : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateFintechBillerInfoNotExistent(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (RPBillerInfoUpdate)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@Name",req.Name),
                new SqlParameter("@OPID",req.OPID),
                new SqlParameter("@OpTypeID",req.OpTypeID),
                new SqlParameter("@MinLength",req.MinLength),
                new SqlParameter("@MaxLength",req.MaxLength),
                new SqlParameter("@MinAmount",req.MinAmount),
                new SqlParameter("@MaxAmount",req.MaxAmount),
                new SqlParameter("@IsBBPS",req.IsBBPS),
                new SqlParameter("@IsBilling",req.IsBilling),
                new SqlParameter("@AccountName",req.AccountName),
                new SqlParameter("@AccountRemark",req.AccountRemark),
                new SqlParameter("@IsAccountNumeric",req.IsAccountNumeric),
                new SqlParameter("@RPBillerID",req.RPBillerID),
                new SqlParameter("@BillerAmountOptions",req.BillerAmountOptions),
                new SqlParameter("@BillerAdhoc",req.BillerAdhoc),
                new SqlParameter("@ExactNess",req.ExactNess),
                new SqlParameter("@BillerCoverage",req.BillerCoverage),
                new SqlParameter("@BillerName",req.BillerName),
                new SqlParameter("@AccountNoKey",req.AccountNoKey),
                new SqlParameter("@IsBillValidation",req.IsBillValidation),
                new SqlParameter("@IsAmountInValidation",req.IsAmountInValidation),
                new SqlParameter("@BillerPaymentModes",req.BillerPaymentModes??string.Empty),
                new SqlParameter("@RegExAccount",req.RegExAccount??string.Empty),
                new SqlParameter("@EarlyPaymentAmountKey",req.EarlyPaymentAmountKey??string.Empty),
                new SqlParameter("@LatePaymentAmountKey",req.LatePaymentAmountKey??string.Empty),
                new SqlParameter("@EarlyPaymentDateKey",req.EarlyPaymentDateKey??string.Empty),
                new SqlParameter("@IsAmountOptions",req.IsAmountOptions),
                new SqlParameter("@tp_OperatorParams",req.tp_OperatorParams),
                new SqlParameter("@tp_OperatorPaymentChanel",req.tp_OperatorPaymentChanel),
                new SqlParameter("@tp_OperatorDictionary",req.tp_OperatorDictionary)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
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

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateFintechBillerInfoNotExistent";
    }
}
