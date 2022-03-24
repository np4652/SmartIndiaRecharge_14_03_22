using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAssignService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAssignService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int UserID = (int)obj;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@UserID", UserID);
            var Package = new List<Package_Cl>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var package_Cl = new Package_Cl
                        {
                            Name = dr["_Name"].ToString(),
                            Service = dr["_Service"].ToString(),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
                            IsDisplayService = dr["_IsDisplayService"] is DBNull ? false : Convert.ToBoolean(dr["_IsDisplayService"]),
                            ServiceID = Convert.ToInt32(dr["_ID"]),
                            SCode = Convert.ToString(dr["_SCode"]),
                            ParentID = Convert.ToInt32(dr["_ParentID"]),
                            IsServiceActive = dr["_IsServiceActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsServiceActive"]),
                            IsShowMore = dr["_IsShowMore"] is DBNull ? false : Convert.ToBoolean(dr["_IsShowMore"]),
                            Upline=Convert.ToString(dr["_Upline"]),
                            UplineMobile = Convert.ToString(dr["_UplineMobile"]),
                            CCContact = Convert.ToString(dr["_CCContact"]),
                            IsAdditionalServiceType = Convert.ToBoolean(dr["_IsAdditionalServiceType"]),
                            ServiceTypeID= Convert.ToString(dr["_ServiceTypeID"])
                        };
                        Package.Add(package_Cl);
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Package;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_AssignService";
    }
}
