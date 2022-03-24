using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcIsVendor : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcIsVendor(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            try
            {
                DataTable dt = await _dal.GetAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    res.CommonBool = Convert.ToBoolean(dt.Rows[0][0]);
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

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "select ISNULL(_IsVendor,0) _IsVendor from tbl_Users where _ID=@LoginID";
    }
}
