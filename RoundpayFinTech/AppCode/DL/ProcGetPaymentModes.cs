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
    public class ProcGetPaymentModes : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPaymentModes(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID)
            };
            var res = new List<OperatorDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new OperatorDetail
                        {
                            OID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                            CommSettingType = item["_CommSettingType"] is DBNull ? 0 : Convert.ToInt32(item["_CommSettingType"]),
                            Name = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                            Charge = item["_Charge"] is DBNull ? 0M : Convert.ToDecimal(item["_Charge"]),
                            ChargeAmtType = item["_AmtType"] is DBNull ? false : Convert.ToBoolean(item["_AmtType"]),
                            IsActive = item["_IsActive"] is DBNull ? false : Convert.ToBoolean(item["_IsActive"]),
                            SPKey = item["_SpKey"] is DBNull ? string.Empty : item["_SpKey"].ToString(),
                            Type = item["_Type"] is DBNull ? 0 : Convert.ToInt32(item["_Type"])
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
                    UserId = 1
                });
            }

            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetPaymentModes";
    }
}
