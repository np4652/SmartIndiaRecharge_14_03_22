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
    public class ProcGetEmailAPI : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEmailAPI(IDAL dal) => _dal = dal;
        
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.CommonInt)
            };

            List<EmailAPIDetail> res = new List<EmailAPIDetail>();

            DataTable dt = _dal.GetByProcedure(GetName(), param);
            if (dt.Rows.Count > 0)
            {
                if ((dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0])) == 1)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var emailAPIDetail = new EmailAPIDetail
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            FromEmail = row["_FromEmail"] is DBNull ?string.Empty : row["_FromEmail"].ToString(),
                            Password = row["_Password"] is DBNull ? string.Empty : row["_Password"].ToString(),
                            HostName = row["_HostName"] is DBNull ? string.Empty : Convert.ToString(row["_HostName"]),
                            Port = row["_Port"] is DBNull ? 0 : Convert.ToInt32(row["_Port"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            IsEmailVerified = row["_IsEmailVerified"] is DBNull ? true : Convert.ToBoolean(row["_IsEmailVerified"]),
                            IsSSL = row["_IsSSL"] is DBNull ? false : Convert.ToBoolean(row["_IsSSL"]),
                            IsDefault = row["_IsDefault"] is DBNull ? false : Convert.ToBoolean(row["_IsDefault"]),
                            UserMailID = row["_MailUserID"] is DBNull ? "" : Convert.ToString(row["_MailUserID"]),
                            WID = row["_WID"] is DBNull ? 0 : Convert.ToInt32(row["_WID"]),
                        };
                        if (req.CommonInt > 0)
                        {
                            return emailAPIDetail;
                        }
                        else
                        {
                            res.Add(emailAPIDetail);
                        }
                    }
                }
            }
            if (req.CommonInt > 0)
                return new EmailAPIDetail { };
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetEmailAPI";
    }
}