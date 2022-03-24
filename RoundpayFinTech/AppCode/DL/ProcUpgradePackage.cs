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
    public class ProcUpgradePackage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpgradePackage(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (PackageAvailableModel)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeId),
                new SqlParameter("@LoginID",req.LoginId),
                new SqlParameter("@PackageId",req.AvailablePackageId),
                //new SqlParameter("@IsAvailable",req.IsAvailable),
                new SqlParameter("@UserId",req.UserId),
                new SqlParameter("@IP",req.IP??""),
                new SqlParameter("@Browser",req.Browser??""),
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
                        if (res.Statuscode == -2)
                        {
                            res.CommonStr = Convert.ToString(dt.Rows[0][2]);
                        }
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
        //public string GetName() => "Proc_UpgradePackage";
        public string GetName() => "Proc_UpgradePackageService";
    }
}
