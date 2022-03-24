using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcIsEmailVerified : IProcedure
    {
        private readonly IDAL _dal;
        public ProcIsEmailVerified(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", LoginID)
            };
            bool res = false;
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res =  Convert.ToBoolean(dt.Rows[0][0]);
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


        public string GetName() => "select ISNULL(_IsEmailVerified,0) from tbl_Users where _ID=@LoginID";
        
    }
}
