using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWebsiteContentAsync : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetWebsiteContentAsync(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@WID", req)
            };
            SiteTemplateSection res = new SiteTemplateSection();
            try
            {
                DataTable dt = await _dal.GetAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    res = new SiteTemplateSection
                    {
                        Slider = dt.Rows[0]["_slider"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_slider"]),
                        AboutUs = dt.Rows[0]["_AboutUs"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AboutUs"]),
                        ContactUs = dt.Rows[0]["_ContactUs"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_ContactUs"]),
                        BankDetail = dt.Rows[0]["_BankDetail"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_BankDetail"]),
                        FacebookLink = dt.Rows[0]["_FacebookLink"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_FacebookLink"]),
                        TwitterLink = dt.Rows[0]["_TwiterLink"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_TwiterLink"]),
                        WhatsappLink = dt.Rows[0]["_whatsappLink"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_whatsappLink"]),
                        WhyChooseUS = dt.Rows[0]["_WhyChooseUS"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_WhyChooseUS"]),
                        ChatScript = dt.Rows[0]["_ChatScript"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_ChatScript"]),
                        ServiceDetail = dt.Rows[0]["_ServiceDetail"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_ServiceDetail"]),
                        Map = dt.Rows[0]["_Map"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_Map"])
                    };
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res ?? new SiteTemplateSection();
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "select * from tbl_Website_Content(nolock) where _WID=@WID";
    }

    public class ProcGetWebsiteBankDetails : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetWebsiteBankDetails(IDAL dal) => _dal = dal;
        public async Task<object> Call()
        {

            var _res = new List<websiteBanks>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName());
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _res.Add(new websiteBanks
                    {
                        ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                        BankID = Convert.ToInt32(dt.Rows[i]["_BankID"]),
                        BankName = dt.Rows[i]["_Bank"].ToString(),
                        BranchName = dt.Rows[i]["_BranchName"].ToString(),
                        AccountHolder = dt.Rows[i]["_AccountHolder"].ToString(),
                        AccountNo = dt.Rows[i]["_AccountNo"].ToString(),
                        IFSCCode = dt.Rows[i]["_IFSCCode"].ToString()

                    });
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message

                });
            }
            return _res;
        }
        public Task<object> Call(object obj) => throw new NotImplementedException();
        public string GetName() => "Proc_websiteBanks";
    }
}
