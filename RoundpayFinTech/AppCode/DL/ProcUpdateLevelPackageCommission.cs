using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateLevelPackageCommission : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateLevelPackageCommission(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (PkgLevelCommissionReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginId),
                new SqlParameter("@LT",req.LoginTypeId),
                new SqlParameter("@PackageId",req.PackageId),
                new SqlParameter("@Comm",req.Commission),
                new SqlParameter("@CommType",req.CommType),
                new SqlParameter("@RoleId",req.RoleId),
                new SqlParameter("@ActionType",req.ActionType),
            };
            try
            {
                if (ApplicationSetting.IsPackageAllowed)
                {
                    var dt = _dal.GetByProcedure(GetName(), param);
                    if (dt.Rows.Count > 0)
                    {
                        res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                        res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeId,
                    UserId = req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_UpdatePackageCommission";
    }
}
