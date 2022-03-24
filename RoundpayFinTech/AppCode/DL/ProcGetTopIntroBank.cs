using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using RoundpayFinTech.AppCode.Model;
using System.Data;
using Fintech.AppCode.Model.Reports;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTopIntroBank : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetTopIntroBank(IDAL dal) => _dal = dal;
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetTopIntroBank";
        public object Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@UserID",(int)obj)
            };
            var resp = new List<ASBanks>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        resp.Add(new ASBanks
                        {
                            BankName = dr["_Bank"] is DBNull ? string.Empty : dr["_Bank"].ToString(),
                            BankID = dr["_BankID"] is DBNull ? 0 : Convert.ToInt32(dr["_BankID"])
                        });
                    }
                }
            }

            catch (Exception ex)
            {
            }
            return resp;
        }
    }
}
