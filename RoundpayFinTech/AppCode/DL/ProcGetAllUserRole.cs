using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAllUserRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAllUserRole(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@UserID", req.LoginID);
            List<RoleMaster> Roles = new List<RoleMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        RoleMaster roleMaster = new RoleMaster
                        {
                            ID = Convert.ToInt32(dr["ValueField"]),
                            Role = dr["TextField"].ToString(),
                            Ind = Convert.ToInt32(dr["Sno"])
                        };
                        Roles.Add(roleMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Roles;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_User_SelectRole";
    }
}

