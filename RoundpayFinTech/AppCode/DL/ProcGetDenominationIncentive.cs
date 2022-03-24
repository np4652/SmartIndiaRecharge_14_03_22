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
    public class ProcGetDenominationIncentive : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenominationIncentive(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt),
            };
            var res = new List<IncentiveDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new IncentiveDetail
                            {
                                Denomination = item["Denomination"] is DBNull ? 0 : Convert.ToInt32(item["Denomination"]),
                                Comm = item["_Comm"] is DBNull ? 0 : Convert.ToDecimal(item["_Comm"]),
                                AmtType = item["_AmtType"] is DBNull ? false : Convert.ToBoolean(item["_AmtType"])
                            });
                        }
                    }
                }
            }
            catch
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetDenominationIncentive";
    }
}
