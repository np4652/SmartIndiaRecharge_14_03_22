using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAllEmailProviders : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAllEmailProviders(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@UserID", req.LoginID);
            List<EmailProvider> Provider = new List<EmailProvider>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Provider.Add(new EmailProvider
                        {
                            //  ID = Convert.ToInt32(dr["_ID"]),
                            ProviderName = dr["_ProviderName"].ToString(),
                            HostName = dr["_HostName"].ToString(),
                            SMTPPort = dr["_SMTPPort"].ToString(),
                            // IsSSL = Convert.ToBoolean(dr["_IsSSL"]).ToString(),
                            ProviderID = dr["_ID"].ToString() + "-" + dr["_ProviderName"].ToString() + "-" + dr["_HostName"].ToString() + "-" + dr["_SMTPPort"].ToString() + "-" + Convert.ToBoolean(dr["_IsSSL"]).ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Provider;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_Email_Provide";
    }
}
