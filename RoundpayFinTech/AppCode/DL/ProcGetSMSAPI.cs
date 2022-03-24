using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSMSAPI : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSMSAPI(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.CommonInt)
            };
            List<SMSAPIDetail> res = new List<SMSAPIDetail> { };
            DataTable dt = _dal.GetByProcedure(GetName(),param);
            if (dt.Rows.Count > 0) {
                if ((dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0])) == 1) {
                    foreach (DataRow row in dt.Rows)
                    {
                        var sMSAPIDetail = new SMSAPIDetail
                        {
                            ID= row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            URL = row["_URL"] is DBNull ? "" : row["_URL"].ToString(),
                            APIType = row["_APIType"] is DBNull ? 1 : Convert.ToInt32(row["_APIType"]),
                            _APIType = APITypes.GetAPIType(row["_APIType"] is DBNull ? 0 : Convert.ToInt16(row["_APIType"])),
                            TransactionType = row["_TransactionType"] is DBNull ? 1 : Convert.ToInt32(row["_TransactionType"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            Default = row["_IsDefault"] is DBNull ? false : Convert.ToBoolean(row["_IsDefault"]),
                            IsWhatsApp = row["_IsWhatsApp"] is DBNull ? false : Convert.ToBoolean(row["_IsWhatsApp"]),
                            IsHangout = row["_IsHangout"] is DBNull ? false : Convert.ToBoolean(row["_IsHangout"]),
                            IsTelegram = row["_IsTelegram"] is DBNull ? false : Convert.ToBoolean(row["_IsTelegram"]),
                            APIMethod = row["_APIMethod"] is DBNull ? "" : row["_APIMethod"].ToString(),
                            ResType = row["_ResType"] is DBNull ? 0 : Convert.ToInt16(row["_ResType"]),
                            IsMultipleAllowed = row["_IsMultipleAllowed"] is DBNull ? false : Convert.ToBoolean(row["_IsMultipleAllowed"]),
                            APICode = row["_APICode"] is DBNull ? "" : Convert.ToString(row["_APICode"])
                        };
                        if (req.CommonInt > 0)
                        {
                            return sMSAPIDetail;
                        }
                        else {
                            res.Add(sMSAPIDetail);
                        }
                    }
                }               
            }
            if (req.CommonInt > 0)
                return new SMSAPIDetail { };
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetSMSAPI";
    }
}
