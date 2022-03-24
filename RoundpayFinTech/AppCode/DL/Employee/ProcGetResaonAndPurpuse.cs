using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetResaonAndPurpuse : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetResaonAndPurpuse(IDAL dal) => _dal = dal;
        public object Call()
        {
            var res = new ReasonAndPurpuse();
            var purpuseList = new List<Purpuse>();
            var reasonList = new List<ReasonToUseOBrand>();
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName());
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            purpuseList.Add(new Purpuse
                            {
                                PurpuseID = Convert.ToInt32(row["_ID"]),
                                PurpuseDetail = Convert.ToString(row["_Purpuse"])
                            });
                        }
                    }
                    res.Purpuse = purpuseList;

                    dt = ds.Tables[1];
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            reasonList.Add(new ReasonToUseOBrand
                            {
                                ReasonID = Convert.ToInt32(row["_ID"]),
                                Reason = Convert.ToString(row["_Reason"])
                            });
                        }
                    }
                    res.Resaon = reasonList;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 3,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call(object obj) => throw new NotImplementedException();
        public string GetName() => "proc_GetResaonAndPurpuse";
    }
}
