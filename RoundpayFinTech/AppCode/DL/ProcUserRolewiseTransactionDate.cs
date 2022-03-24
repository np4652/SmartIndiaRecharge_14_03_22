using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserRolewiseTransactionDate : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUserRolewiseTransactionDate(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.CommonInt),
                new SqlParameter("@RoleID",req.CommonInt2),
                new SqlParameter("@FromDate",req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate",req.CommonStr2 ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@IsSelf",req.CommonBool)
            };
            var res = new List<UserRolewiseTransaction>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new UserRolewiseTransaction
                            {
                                TransactionDate= item["_TransactionDate"] is DBNull ? "" : item["_TransactionDate"].ToString(),
                                UserID = item["_UserID"] is DBNull ? 0 : Convert.ToInt32(item["_UserID"]),
                                Prefix = item["_Prefix"] is DBNull ? "" : item["_Prefix"].ToString(),
                                Role = item["_Role"] is DBNull ? "" : item["_Role"].ToString(),
                                Name = item["_Name"] is DBNull ? "" : item["_Name"].ToString(),
                                OutletName = item["_OutletName"] is DBNull ? "" : item["_OutletName"].ToString(),
                                MobileNo = item["_MobileNo"] is DBNull ? "" : item["_MobileNo"].ToString(),
                                Requested = item["_Requested"] is DBNull ? 0 : Convert.ToDouble(item["_Requested"]),
                                Debited = item["_Debited"] is DBNull ? 0 : Convert.ToDouble(item["_Debited"]),
                                FailedRequested = item["_FailedRequested"] is DBNull ? 0 : Convert.ToDouble(item["_FailedRequested"]),
                                FailedDebited = item["_FailedDebited"] is DBNull ? 0 : Convert.ToDouble(item["_FailedDebited"]),
                                Commission = item["_Commission"] is DBNull ? 0 : Convert.ToDouble(item["_Commission"]),
                                Surcharge = item["_Surcharge"] is DBNull ? 0 : Convert.ToDouble(item["_Surcharge"]),
                                GST = item["_GST"] is DBNull ? 0 : Convert.ToDouble(item["_GST"]),
                                TDS = item["_TDS"] is DBNull ? 0 : Convert.ToDouble(item["_TDS"]),
                                //SAID = item["_SAID"] is DBNull ? 0 : Convert.ToInt32(item["_SAID"]),
                                //SAName = item["_SAName"] is DBNull ? "" : item["_SAName"].ToString(),
                                SACommission = item["_SACommission"] is DBNull ? 0 : Convert.ToDouble(item["_SACommission"]),
                                SAGST = item["_SAGST"] is DBNull ? 0 : Convert.ToDouble(item["_SAGST"]),
                                SATDS = item["_SATDS"] is DBNull ? 0 : Convert.ToDouble(item["_SATDS"]),
                                //MDID = item["_MDID"] is DBNull ? 0 : Convert.ToInt32(item["_MDID"]),
                                //MDName = item["_MDName"] is DBNull ? "" : item["_MDName"].ToString(),
                                MDCommission = item["_MDCommission"] is DBNull ? 0 : Convert.ToDouble(item["_MDCommission"]),
                                MDGST = item["_MDGST"] is DBNull ? 0 : Convert.ToDouble(item["_MDGST"]),
                                MDTDS = item["_MDTDS"] is DBNull ? 0 : Convert.ToDouble(item["_MDTDS"]),
                                //DTID = item["_DTID"] is DBNull ? 0 : Convert.ToInt32(item["_DTID"]),
                                //DTName = item["_DTName"] is DBNull ? "" : item["_DTName"].ToString(),
                                DTCommission = item["_DTCommission"] is DBNull ? 0 : Convert.ToDouble(item["_DTCommission"]),
                                DTGST = item["_DTGST"] is DBNull ? 0 : Convert.ToDouble(item["_DTGST"]),
                                DTTDS = item["_DTTDS"] is DBNull ? 0 : Convert.ToDouble(item["_DTTDS"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UserRolewiseTransaction_Date";
        }
    }
}