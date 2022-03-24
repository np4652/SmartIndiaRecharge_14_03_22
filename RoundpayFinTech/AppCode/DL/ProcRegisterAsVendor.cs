using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRegisterAsVendor : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRegisterAsVendor(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (CommonReq)obj;
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LT",req.LoginTypeID),
                    new SqlParameter("@LoginID",req.LoginID),
                    new SqlParameter("@UserID",req.CommonInt)
                };
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    response.Statuscode = Convert.ToInt32(dt.Rows[0]["StatusCode"]);
                    response.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_RegisterAsVendor";
    }
}

