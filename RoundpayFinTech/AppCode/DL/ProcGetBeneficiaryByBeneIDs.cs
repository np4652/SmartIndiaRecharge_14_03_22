using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBeneficiaryByBeneIDs : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetBeneficiaryByBeneIDs(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new List<AddBeni>();
            SqlParameter[] param = {
                new SqlParameter("@BeneIDs",req.CommonStr??string.Empty),
                new SqlParameter("@APICode",req.CommonStr2??string.Empty),
                new SqlParameter("@SenderNo",req.CommonStr3??string.Empty)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonBool)
                    {
                        var _res = new List<MBeneDetail>();
                        foreach (DataRow item in dt.Rows)
                        {
                            _res.Add(new MBeneDetail
                            {
                                AccountNo = item["_Account"] is DBNull ? string.Empty : item["_Account"].ToString(),
                                BankName = item["_BankName"] is DBNull ? string.Empty : item["_BankName"].ToString(),
                                IFSC = item["_IFSC"] is DBNull ? string.Empty : item["_IFSC"].ToString(),
                                BeneName = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                                BeneID = item["_BeneID"] is DBNull ? string.Empty : item["_BeneID"].ToString()
                            });
                        }
                        return res;
                    }
                    else
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new AddBeni
                            {
                                AccountNo = item["_Account"] is DBNull ? string.Empty : item["_Account"].ToString(),
                                BankName = item["_BankName"] is DBNull ? string.Empty : item["_BankName"].ToString(),
                                IFSC = item["_IFSC"] is DBNull ? string.Empty : item["_IFSC"].ToString(),
                                BeneName = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                                BeneID = item["_BeneID"] is DBNull ? string.Empty : item["_BeneID"].ToString()
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
            }
            if (req.CommonBool)
            {
                return new List<MBeneDetail>();
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetBeneficiaryByBeneIDs";
    }
}
