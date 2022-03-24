using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAvailablePackage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAvailablePackage(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (PackageAvailableModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeId),
                new SqlParameter("@LoginID",_req.LoginId),
            };
            var _list = new List<PackageAvailableModel>();
            if (!ApplicationSetting.IsPackageAllowed)
                return _list;
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var packageAvailable = new PackageAvailableModel
                        {
                            UserId = dr["_UserId"] is DBNull ? 0 : Convert.ToInt32(dr["_UserId"]),
                            AvailablePackageId = dr["_Id"] is DBNull ? 0 : Convert.ToInt32(dr["_Id"]),
                            IsAvailable = dr["_IsAvailable"] is DBNull ? false : Convert.ToBoolean(dr["_IsAvailable"]),
                            PackageId = dr["_PackageID"] is DBNull ? 0 : Convert.ToInt32(dr["_PackageID"])
                        };
                        _list.Add(packageAvailable);
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
                    LoginTypeID = _req.LoginTypeId,
                    UserId = _req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _list;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetAvailablePackage";
    }
}
