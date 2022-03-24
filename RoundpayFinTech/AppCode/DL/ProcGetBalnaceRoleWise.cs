using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBalnaceRoleWise : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBalnaceRoleWise(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public object Call()
        {
            List<DashboardData> res =new List<DashboardData>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName());
                foreach (DataRow item in dt.Rows)
                {
                    var _res = new DashboardData
                    {
                        ServiceTypeID = Convert.ToInt16(item["_RoleID"]),
                        ServiceType = item["_Role"].ToString(),
                        Amount = Convert.ToDecimal(item["Balance"]),
                        OpeningBal = Convert.ToDecimal(item["UBalance"]),
                        RoleCount = Convert.ToInt16(item["RoleCount"])
                    };
                    res.Add(_res);
                } 
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
           
        }

        public string GetName()
        {
            return "proc_GetBalnaceRoleWise";
        }
    }
}
