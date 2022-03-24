using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Org.BouncyCastle.Asn1.X509;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcTargetSegment : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTargetSegment(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };

            var data = new List<TargetSegment>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            data.Add(new TargetSegment
                            {
                                Type = Convert.ToString(dr["_Type"]),
                                Target = Convert.ToDecimal(dr["_Target"]) > 0 ? Convert.ToDecimal(dr["_Target"]) : 1,
                                Achieve = Convert.ToDecimal(dr["_Ach"]) > 0 ? Convert.ToDecimal(dr["_Ach"]) : 1,
                                //AchievePercent = Convert.ToDecimal(dr["_Target"]) > 0 && Convert.ToDecimal(dr["_Ach"]) > 0 ? ((Convert.ToDecimal(dr["_Target"]) - Convert.ToDecimal(dr["_Ach"])) / Convert.ToDecimal(dr["_Target"])) * Convert.ToDecimal(0.01) : 0
                                AchievePercent = ((Convert.ToDecimal(dr["_Ach"]) > 0 ? Convert.ToDecimal(dr["_Ach"]) : 1) / (Convert.ToDecimal(dr["_Target"]) > 0 ? Convert.ToDecimal(dr["_Target"]) : 1)) * 100,
                                strIncentive = Convert.ToString(dr["_incentive"])
                            });
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
                    LoginTypeID = 1,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return data;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_TargetSegment";
    }
}