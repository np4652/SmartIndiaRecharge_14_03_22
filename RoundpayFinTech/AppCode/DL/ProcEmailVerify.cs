using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcEmailVerify : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEmailVerify(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", LoginID)
            };
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Update tbl_Users set _IsEmailVerified = 1 where _Id=@LoginID;select 1,'E-Mail verified' Msg";

    }
}

