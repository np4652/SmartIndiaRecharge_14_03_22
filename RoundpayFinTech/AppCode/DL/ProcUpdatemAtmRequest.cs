using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdatemAtmRequest : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdatemAtmRequest(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdatemAtmRequest";
        public async Task<object> Call(object obj)
        {
            var _req = (mAtmRequestResponse)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@mAtmStatus", _req.Status),
                new SqlParameter("@id", _req.ID)
            };
            mAtmRequestResponse _res = new mAtmRequestResponse()
            {
                Status = ErrorCodes.Minus1,
                Msg = "Error!"
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if(dt!=null && dt.Rows.Count > 0)
                {
                    _res.StatusCode = Convert.ToInt32(dt.Rows[0]["StatusCode"]);
                    _res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception er)
            {

            }
            return _res;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }
}
