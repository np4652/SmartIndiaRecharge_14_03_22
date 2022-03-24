using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcValidateAEPSURLRequest : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcValidateAEPSURLRequest(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIUserID",req.LoginID),
                new SqlParameter("@PartnerID",req.CommonInt),
                new SqlParameter("@OutletID",req.CommonInt2),
                new SqlParameter("@URLID",req.CommonInt3),
                new SqlParameter("@TransactionID",req.CommonStr??string.Empty)
            };

            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0], CultureInfo.InvariantCulture);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonStr = dt.Rows[0]["_PartnerOutlet"] is DBNull ? string.Empty : dt.Rows[0]["_PartnerOutlet"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 0,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_ValidateAEPSURLRequest";
    }
}
