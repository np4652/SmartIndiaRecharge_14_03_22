using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateRailwayKYCStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateRailwayKYCStatus(IDAL dal) => _dal = dal; 
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@IRCTCID", req.CommonStr),
                new SqlParameter("@OutletID", req.CommonInt),
                new SqlParameter("@Status", req.CommonInt2)
            };

            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }

            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.LoginID
                });
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_UpdateRailwayKYCStatus";
    }
}