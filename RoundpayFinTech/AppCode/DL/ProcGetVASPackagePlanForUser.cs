using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetVASPackagePlanForUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVASPackagePlanForUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var res = new List<VasUserPackage>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0][0].ToString() != "-1")
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var slabMaster = new VasUserPackage
                            {
                                VasPackageID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                IsExpired = row["_IsExpired"] is DBNull ? false : Convert.ToBoolean(row["_IsExpired"]),
                                PlanExpirationDate = row["_PackageExpiration"] is DBNull ? string.Empty : row["_PackageExpiration"].ToString()
                            };
                            res.Add(slabMaster);
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetVASPackagePlanForUser";
    }
}