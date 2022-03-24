using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCompanyProfileUserWise: IProcedure
    {
        private readonly IDAL _dal;
        public ProcCompanyProfileUserWise(IDAL dal) => _dal = dal;
        
        public object Call(object obj)
        {
            int UserID = (int)obj;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@LoginId", UserID);
            var CompanyProfile = new CompanyProfileDetail();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    CompanyProfile.Address = dt.Rows[0]["_CompanyAddress"] == null ? "" : dt.Rows[0]["_CompanyAddress"].ToString();
                    CompanyProfile.EmailId = dt.Rows[0]["_EmailID"] == null ? "" : dt.Rows[0]["_EmailID"].ToString();
                    CompanyProfile.Name = dt.Rows[0]["_Name"] == null ? "" : dt.Rows[0]["_Name"].ToString();
                    CompanyProfile.PhoneNo = dt.Rows[0]["_PhoneNo"] == null ? "" : dt.Rows[0]["_PhoneNo"].ToString();
                    CompanyProfile.mobileNo = dt.Rows[0]["_MobileNo"] == null ? "" : dt.Rows[0]["_MobileNo"].ToString();
                    CompanyProfile.AccountPhoneNos = dt.Rows[0]["_AccountsDepNo"] == null ? "" : dt.Rows[0]["_AccountsDepNo"].ToString();
                    CompanyProfile.AccountEmailId = dt.Rows[0]["_AccountsEmailID"] == null ? "" : dt.Rows[0]["_AccountsEmailID"].ToString();

                    CompanyProfile.Facebook = dt.Rows[0]["_Facebook"] == null ? "" : dt.Rows[0]["_Facebook"].ToString();
                    CompanyProfile.Instagram = dt.Rows[0]["_Instagram"] == null ? "" : dt.Rows[0]["_Instagram"].ToString();
                    CompanyProfile.Twitter = dt.Rows[0]["_Twitter"] == null ? "" : dt.Rows[0]["_Twitter"].ToString();
                    CompanyProfile.WhatsApp = dt.Rows[0]["_WhatsApp"] == null ? "" : dt.Rows[0]["_WhatsApp"].ToString();
                    CompanyProfile.website = dt.Rows[0]["_WebSiteName"] == null ? "" : dt.Rows[0]["_WebSiteName"].ToString();
                    CompanyProfile.HeaderTitle = dt.Rows[0]["_HeaderTitle"] == null ? "" : dt.Rows[0]["_HeaderTitle"].ToString();
                    CompanyProfile.CustomerCareMobileNos = dt.Rows[0]["_MobileSupport"] == null ? "" : dt.Rows[0]["_MobileSupport"].ToString();
                    CompanyProfile.CustomerCareEmailIds = dt.Rows[0]["_EmailIDSupport"] == null ? "" : dt.Rows[0]["_EmailIDSupport"].ToString();
                    CompanyProfile.CustomerPhoneNos = dt.Rows[0]["_PhoneNoSupport"] == null ? "" : dt.Rows[0]["_PhoneNoSupport"].ToString();
                    CompanyProfile.CustomerWhatsAppNos = dt.Rows[0]["_WhatsAppSupport"] == null ? "" : dt.Rows[0]["_WhatsAppSupport"].ToString();
                    CompanyProfile.AccountMobileNo = dt.Rows[0]["_AccountMobileNo"] == null ? "" : dt.Rows[0]["_AccountMobileNo"].ToString();
                    CompanyProfile.AccountWhatsAppNos = dt.Rows[0]["_AccountWhatsApp"] == null ? "" : dt.Rows[0]["_AccountWhatsApp"].ToString();
                    CompanyProfile.SalesPersonEmail = dt.Rows[0]["_SalesEmailID"] == null ? "" : dt.Rows[0]["_SalesEmailID"].ToString();
                    CompanyProfile.SalesPersonNo = dt.Rows[0]["_SalesMobile"] == null ? "" : dt.Rows[0]["_SalesMobile"].ToString();
                    CompanyProfile.AppName = dt.Rows[0]["_AppName"] == null ? "" : dt.Rows[0]["_AppName"].ToString();
                    CompanyProfile.OwnerName = dt.Rows[0]["_OwnerName"] == null ? "" : dt.Rows[0]["_OwnerName"].ToString();
                    CompanyProfile.OwnerDesignation = dt.Rows[0]["_OwnerDesignation"] == null ? "" : dt.Rows[0]["_OwnerDesignation"].ToString();
                    CompanyProfile.SignupReferalID = Convert.ToInt32(dt.Rows[0]["_SignupReferalID"] is DBNull ? "0"  : dt.Rows[0]["_SignupReferalID"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return CompanyProfile;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_CompanyProfile_UserWise";        
    }
}

