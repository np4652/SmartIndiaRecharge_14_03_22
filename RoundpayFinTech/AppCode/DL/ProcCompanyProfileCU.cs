using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCompanyProfileCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCompanyProfileCU(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CompanyProfileDetailReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@WID", _req.WID),
                new SqlParameter("@Name", _req.Name),
                new SqlParameter("@MobileNo", _req.mobileNo),
                new SqlParameter("@PhoneNo", _req.PhoneNo),
                new SqlParameter("@EmailId", _req.EmailId),
                new SqlParameter("@Facebook", _req.Facebook),
                new SqlParameter("@Twitter", _req.Twitter),
                new SqlParameter("@Instagram", _req.Instagram),
                new SqlParameter("@CompanyAddress", _req.Address),
                new SqlParameter("@WhatsApp", _req.WhatsApp),
                new SqlParameter("@AccountsDepNo", _req.AccountPhoneNos),
                new SqlParameter("@AccountsEmailID", _req.AccountEmailId),
                new SqlParameter("@Website", _req.website),
                new SqlParameter("@HeaderTitle", _req.HeaderTitle),
                new SqlParameter("@MobileNoSupport", _req.CustomerCareMobileNos),
                new SqlParameter("@PhoneNoSupport", _req.CustomerPhoneNos),
                new SqlParameter("@WhatsAppSupport", _req.CustomerWhatsAppNos),
                new SqlParameter("@EmailIDSupport", _req.CustomerCareEmailIds),
                new SqlParameter("@AccountMobileNo", _req.AccountMobileNo),
                new SqlParameter("@AccountWhatsApp", _req.AccountWhatsAppNos),
                 new SqlParameter("@SalepersonEmail", _req.SalesPersonEmail),
                new SqlParameter("@SalePersonMobile", _req.SalesPersonNo),
                new SqlParameter("@OwnerName", _req.OwnerName),
                new SqlParameter("@OwnerDesignation", _req.OwnerDesignation),
                new SqlParameter("@SignupReferalID", _req.SignupReferalID),
                
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "poc_CompanyProfile_CU";
    }
}
