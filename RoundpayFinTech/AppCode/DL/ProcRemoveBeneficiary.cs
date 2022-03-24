using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRemoveBeneficiary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRemoveBeneficiary(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                ErrorCode=ErrorCodes.Unknown_Error
            };
            SqlParameter[] param = {
                new SqlParameter("@BeneID",req.CommonStr??string.Empty),
                new SqlParameter("@ID",req.CommonInt)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) 
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg= dt.Rows[0]["Msg"] is DBNull?string.Empty: dt.Rows[0]["Msg"].ToString();                     
                    res.ErrorCode= dt.Rows[0]["ErrorCode"] is DBNull? res.ErrorCode : Convert.ToInt16(dt.Rows[0]["ErrorCode"]); 
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    UserId = req.CommonInt
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_RemoveBeneficiary";
    }
}
