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
    public class ProcDeletekeyFromWADictionary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeletekeyFromWADictionary(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            int KeyId = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@KeyId", KeyId)
            };
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    response = new ResponseStatus
                    {
                        Statuscode = dt.Rows[0]["Statuscode"] is DBNull ? ErrorCodes.Minus1 : Convert.ToInt32(dt.Rows[0]["Statuscode"]),
                        Msg = dt.Rows[0]["msg"] is DBNull ? ErrorCodes.TempError : Convert.ToString(dt.Rows[0]["msg"]),
                    };
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Delete from MASTER_WhatsappBotDic where _Id=@KeyId;select 1 StatusCode,'Key Deleted Successfully' msg";
    }
}
