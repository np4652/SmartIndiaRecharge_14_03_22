using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcgetShoppingCommRole : IProcedure
    {
        private readonly IDAL _dal;

        public ProcgetShoppingCommRole(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<MasterRole>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new MasterRole()
                        {
                            Id = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
                            RoleName = dr["_Role"] is DBNull ? "" : dr["_Role"].ToString()
                        };
                        res.Add(item);
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
        public string GetName() => "Proc_GetShoppingCommRoles";
    }
}
