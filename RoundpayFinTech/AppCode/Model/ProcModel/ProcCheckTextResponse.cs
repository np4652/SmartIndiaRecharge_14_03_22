using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcCheckTextResponse : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcCheckTextResponse(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (APISTATUSCHECK)obj;
            SqlParameter[] param = {
                new SqlParameter("@Msg", _req.Msg)
            };
            
            var _res = new APISTATUSCHECK
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains("Msg"))
                    {
                        _res.Msg = dt.Rows[0]["Msg"].ToString();
                    }
                    else
                    {
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Match found";
                        _res.ID = Convert.ToInt32(dt.Rows[0]["ID"] is DBNull ? 0 : dt.Rows[0]["ID"]);
                        _res.ErrorCode = dt.Rows[0]["ErrCode"] is DBNull ? "" : dt.Rows[0]["ErrCode"].ToString();
                        _res.Checks = dt.Rows[0]["Checks"].ToString();
                        _res.Status = Convert.ToInt32(dt.Rows[0]["Status"] is DBNull ? 0 : dt.Rows[0]["Status"]);
                        _res.VendorID = dt.Rows[0]["VendorID"] is DBNull ? "" : dt.Rows[0]["VendorID"].ToString();
                        _res.VendorIDIndex = dt.Rows[0]["VendorIDIndex"] is DBNull ? (int?)null : Convert.ToInt32(dt.Rows[0]["VendorIDIndex"]);
                        _res.VendorIDIndex = dt.Rows[0]["VendorIDIndex"] is DBNull ? (int?)null : Convert.ToInt32(dt.Rows[0]["VendorIDIndex"]);
                        _res.OperatorID = dt.Rows[0]["OperatorID"].ToString();
                        _res.OperatorIDIndex = dt.Rows[0]["OperatorIDIndex"] is DBNull ? (int?)null : Convert.ToInt32(dt.Rows[0]["OperatorIDIndex"]);
                        _res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? "" : dt.Rows[0]["TransactionID"].ToString();
                        _res.TransactionIDIndex = dt.Rows[0]["TransactionIDIndex"] is DBNull ? (int?)null : Convert.ToInt32(dt.Rows[0]["TransactionIDIndex"]);
                        _res.Balance = dt.Rows[0]["Balance"] is DBNull ? "" : dt.Rows[0]["Balance"].ToString();
                        _res.BalanceIndex = Convert.ToInt32(dt.Rows[0]["BalanceIndex"] is DBNull ? null : dt.Rows[0]["BalanceIndex"]);
                    }
                }
            }
            catch (Exception)
            { }
            return _res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_CheckTextResponse";
    }
}
