using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{

    public class ProcInitiateQRGeneration : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcInitiateQRGeneration(IDAL dal) => _dal = dal;
        public string GetName() => "proc_InitiateQRGeneration";
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID)
            };
            var _resp = new ResponseStatus { 
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if(_resp.Statuscode == ErrorCodes.One)
                        _resp.CommonInt = Convert.ToInt32(dt.Rows[0]["_ID"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _resp;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }

}
