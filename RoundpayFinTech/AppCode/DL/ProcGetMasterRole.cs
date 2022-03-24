using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
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
    public class ProcGetMasterRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMasterRole(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param =
                 {
                new SqlParameter("@LT",req.CommonInt),
                new SqlParameter("@LoginId",req.CommonInt2)
            };

            List<MasterRole> res = new List<MasterRole>();            
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if(dt.Rows.Count>0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var item = new MasterRole
                        {
                            Id = row["_ID"] is DBNull ? 0:Convert.ToInt32(row["_ID"]),
                            RoleName = row["_Role"] is DBNull ? "":row["_Role"].ToString(),
                            RegCharge = row["_RegCharge"] is DBNull ? 0 : Convert.ToInt32(row["_RegCharge"])
                        };
                        res.Add(item);
                    }
                }
            }
            catch(Exception ex)
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetMasterRole";
    }
}
