using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPaymentGatewayUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPaymentGatewayUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WID",req.CommonInt),
                new SqlParameter("@IsUPI",req.CommonBool)
            };
            var res = new List<PaymentGatewayDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    foreach (DataRow row in dt.Rows)
                    {
                        res.Add(new PaymentGatewayDetail {
                            PGID = row["_PGID"] is DBNull ? 0 : Convert.ToInt32(row["_PGID"]),
                            UPGID = row["_UPGID"] is DBNull ? 0 : Convert.ToInt32(row["_UPGID"]),
                            PG = row["_PG"] is DBNull ? string.Empty : row["_PG"].ToString(),
                            URL = row["_URL"] is DBNull ? string.Empty : row["_URL"].ToString(),
                            StatusCheckURL = row["_StatusCheckURL"] is DBNull ? string.Empty : row["_StatusCheckURL"].ToString(),
                            Code = row["_Code"] is DBNull ? string.Empty : row["_Code"].ToString(),
                            MerchantID = row["_MerchantID"] is DBNull ? string.Empty : row["_MerchantID"].ToString(),
                            MerchantKey = row["_MerchantKey"] is DBNull ? string.Empty : row["_MerchantKey"].ToString(),
                            ENVCode = row["_ENVCode"] is DBNull ? string.Empty : row["_ENVCode"].ToString(),
                            IndustryType = row["_IndustryType"] is DBNull ? string.Empty : row["_IndustryType"].ToString(),
                            SuccessURL = row["_SuccessURL"] is DBNull ? string.Empty : row["_SuccessURL"].ToString(),
                            FailedURL = row["_FailedURL"] is DBNull ? string.Empty : row["_FailedURL"].ToString(),
                            AgentType = row["_AgentType"] is DBNull ? 1 : Convert.ToInt16(row["_AgentType"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.CommonInt
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetPaymentGateway_User";
    }
}
