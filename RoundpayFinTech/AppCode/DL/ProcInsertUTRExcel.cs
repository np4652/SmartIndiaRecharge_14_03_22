using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcInsertUTRExcel : IProcedureAsync
    {
        private IDAL _dal;

        public ProcInsertUTRExcel(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var req = (UTRExcelReq)obj;
            var record = req.Record.Select(x => new { x.UserIdentity, x.UTR, x.Amount, x.VirtualAccount, x.CustomerCode, x.CustomerAccountNumber, x.Type, x.ProcName, x.Status }).ToList();
            DataTable dataTable = record.ToDataTable();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@IP", req.CommonStr),
                new SqlParameter("@Browser", req.CommonStr2),
                new SqlParameter("@Record", dataTable)
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_InsertUTRExcel";

    }
}
