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
    public class procGetApiIDforResend:IProcedure
    {
        private readonly IDAL _dal;
        public procGetApiIDforResend(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@WID",req.CommonInt)
            };
            List<SMSAPIDetail> res = new List<SMSAPIDetail> { };
            DataTable dt = _dal.GetByProcedure(GetName(), param);
            if (dt.Rows.Count > 0)
            {
                
                   
                        var sMSAPIDetail = new SMSAPIDetail
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString(),
                            URL = dt.Rows[0]["_URL"] is DBNull ? "" : dt.Rows[0]["_URL"].ToString(),
                            APIType = dt.Rows[0]["_APIType"] is DBNull ? 1 : Convert.ToInt32(dt.Rows[0]["_APIType"]),
                            _APIType = APITypes.GetAPIType(dt.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIType"])),
                            TransactionType = dt.Rows[0]["_TransactionType"] is DBNull ? 1 : Convert.ToInt32(dt.Rows[0]["_TransactionType"]),
                            IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                            Default = dt.Rows[0]["_IsDefault"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDefault"]),
                            IsWhatsApp = dt.Rows[0]["_IsWhatsApp"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsWhatsApp"]),
                            IsHangout = dt.Rows[0]["_IsHangout"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsHangout"]),
                            IsTelegram = dt.Rows[0]["_IsTelegram"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsTelegram"]),
                            APIMethod = dt.Rows[0]["_APIMethod"] is DBNull ? "" : dt.Rows[0]["_APIMethod"].ToString(),
                            ResType = dt.Rows[0]["_ResType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ResType"]),
                            IsMultipleAllowed = dt.Rows[0]["_IsMultipleAllowed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsMultipleAllowed"]),
                            APICode = dt.Rows[0]["_APICode"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_APICode"])
                        };

                return sMSAPIDetail;
            }
            
            return new SMSAPIDetail();
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetApiIDforResend";
    }
}
