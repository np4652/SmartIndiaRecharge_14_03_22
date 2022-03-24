using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace RoundpayFinTech.AppCode.DL
{
    public class EmailDL
    {
        private readonly IDAL _dal;
        public EmailDL(IDAL dal)
        {
            _dal = dal;
        }
        public DataTable GetEmailSetting(int WID, int RoleId = 0)
        {
            SqlParameter[] param = {
                new SqlParameter("@WID",WID)
            };
            string Query = @"if Exists(select 1 from tbl_emailsetting(nolock) where _WID=@WID And _IsActive=1)
		                          begin
			                        select top 1 _ID ID, _FromEmail FromEmail, _Password [Password], _HostName HostName, _Port [Port], _EntryDate EntryDate, _ModifyDate ModifyDate, _IsActive IsActive,_IsSSL IsSSL,_MailUserID MailUserID from tbl_EmailSetting
			                        where _WID=@WID And _IsActive=1 order by id
		                        end
	                        else
		                        begin
		                        	select top 1 _ID ID, _FromEmail FromEmail, _Password [Password], _HostName HostName, _Port [Port], _EntryDate EntryDate, _ModifyDate ModifyDate, _IsActive IsActive,_IsSSL IsSSL,_MailUserID MailUserID from tbl_EmailSetting where _IsDefault=1 order by id
		                        end";
            if (RoleId == Role.Admin)
                Query = "select top 1 _ID ID, _FromEmail FromEmail, _Password [Password], _HostName HostName, _Port [Port], _EntryDate EntryDate, _ModifyDate ModifyDate, _IsActive IsActive,_IsSSL IsSSL,_MailUserID MailUserID from tbl_EmailSetting where _onlyForAdmin=1 order by id";

                try
                {
                    return _dal.Get(Query, param);
                    //return _dal.Get("select top 1 _ID ID, _FromEmail FromEmail, _Password [Password], _HostName HostName, _Port [Port], _EntryDate EntryDate, _ModifyDate ModifyDate, _IsActive IsActive,_IsSSL IsSSL,_MailUserID MailUserID from tbl_EmailSetting where _IsActive=1 and _WID=" + WID + " order by id");
                }
                catch (Exception ex)
                {
                    return new DataTable();
                }
        }
        public bool SaveMail(SendEmail sendEmail)
        {
            if (sendEmail.Body != null)
            {
                try 
                {
                    const string insertQuery = "INSERT INTO tbl_SendEmail(_From,_Recipients,_Subject,_Body,_IsSent,_EntryDate,_ModifyDate,_WID)VALUES(@_From,@_Recipients,@_Subject,@_Body,@_IsSent,@DT,@DT,@_WID); ";
                    StringBuilder sbQuery = new StringBuilder("declare @DT datetime=getdate(); ");
                    Hashtable param = new Hashtable
                    {
                        { "@_From", sendEmail.From??string.Empty},
                        { "@_Recipients", sendEmail.Recipients??string.Empty },
                        { "@_Subject", sendEmail.Subject??string.Empty },
                        { "@_Body", sendEmail.Body??string.Empty},
                        { "@_IsSent", sendEmail.IsSent },
                        { "@_WID", sendEmail.WID }
                    };
                    sbQuery.Append(insertQuery);
                    _dal.Execute(sbQuery.ToString(), param);
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
        public bool SaveContactMail(SendEmail sendEmail)
        {
            if (sendEmail.Body != null)
            {
                try
                {
                    const string insertQuery = "INSERT INTO tbl_Contactus(_FromMail,_ToEmail,_Name,_userEmail,_MobileNO,_Message,_RequestModeID,_RequestIP, _Entrydate , _Issent , _Body,_RequestPage)" +
                        "VALUES(@_FromMail,@_ToEmail,@_UserName,@_EmailID,@_UserMobileNo,@_Message,@_RequestMode,@_RequestIP,@DT,@_IsSent,@_Body,@_RequestPage); ";
                    StringBuilder sbQuery = new StringBuilder("declare @DT datetime=getdate();");
                    Hashtable param = new Hashtable
                    {
                        { "@_FromMail", sendEmail.From??string.Empty},
                        { "@_ToEmail", sendEmail.Recipients??string.Empty },
                        { "@_UserName", sendEmail.UserName??string.Empty},
                        { "@_EmailID", sendEmail.EmailID??string.Empty},
                        { "@_UserMobileNo", sendEmail.UserMobileNo??string.Empty},
                        { "@_Message", sendEmail.Message??string.Empty},
                        { "@_RequestMode", sendEmail.RequestMode },
                        { "@_RequestIP", sendEmail.RequestIP??string.Empty},
                        { "@_IsSent", sendEmail.IsSent },
                        { "@_Body", sendEmail.Body??string.Empty},
                        { "@_RequestPage", sendEmail.RequestPage??string.Empty},
                    };
                    sbQuery.Append(insertQuery);
                    _dal.Execute(sbQuery.ToString(), param);
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
    }
}
