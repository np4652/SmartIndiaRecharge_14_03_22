using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class procGetEmpTarget : IProcedure
    {
        private IDAL _dal;
        public procGetEmpTarget(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@EmpID", req.CommonInt),
                new SqlParameter("@OID", req.CommonInt2),
                new SqlParameter("@TargetTypeID", req.CommonInt3)                
            };
            var res = new List<EmpTarget>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach(DataRow dr in dt.Rows)
                    {
                        res.Add(new EmpTarget
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            OID = dr["OID"] is DBNull ? 0 : Convert.ToInt32(dr["OID"]),
                            EmpID = dr["_EmpID"] is DBNull ? 0 : Convert.ToInt32(dr["_EmpID"]),
                            Target = dr["_Target"] is DBNull ? 0 : Convert.ToDecimal(dr["_Target"]),
                            Commission = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]),
                            HikePer = dr["_HikePer"] is DBNull ? 0 : Convert.ToDecimal(dr["_HikePer"]),
                            AmtType = dr["_AmtType"] is DBNull ? false : Convert.ToBoolean(dr["_AmtType"]),
                            IsEarned = dr["_IsEarned"] is DBNull ? false : Convert.ToBoolean(dr["_IsEarned"]),
                            IsGift = dr["_IsGift"] is DBNull ? false : Convert.ToBoolean(dr["_IsGift"]),
                            IsHikeOnEarned = dr["_IsHikeOnEarned"] is DBNull ? false : Convert.ToBoolean(dr["_IsHikeOnEarned"]),
                            OName = Convert.ToString(dr["OName"]),
                            ChildTarget = dr["_childTarget"] is DBNull ? 0 : Convert.ToInt32(dr["_childTarget"])
                        });
                    }                    
                }
            }
            catch(Exception ex)
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

            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetEmpTarget";
    }
}
