using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetWebsiteInfoEComm : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWebsiteInfoEComm(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            string WebsiteName = (string)obj;
            SqlParameter[] param ={
                new SqlParameter("@WebsiteName", WebsiteName ?? "")
            };
            WebsiteInfo _res = new WebsiteInfo
            {
                WebsiteName = WebsiteName
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.WID = dt.Rows[0]["_WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
                    _res.WUserID = dt.Rows[0]["_WUserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WUserID"]);
                    _res.ThemeId = dt.Rows[0]["ThemeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ThemeID"]);
                    _res.IsMultipleMobileAllowed = dt.Rows[0]["IsMultipleMobileAllowed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsMultipleMobileAllowed"]);
                    _res.MainDomain = dt.Rows[0]["_MainDomain"] is DBNull ? string.Empty : dt.Rows[0]["_MainDomain"].ToString();
                    _res.ShoppingDomain = dt.Rows[0]["_ShoppingDomain"] is DBNull ? string.Empty : dt.Rows[0]["_ShoppingDomain"].ToString();
                    List<PageMaster> PageMasters = new List<PageMaster>();
                    foreach (DataRow row in dt.Rows)
                    {
                        PageMaster pageMaster = new PageMaster
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            PageName = dt.Rows[0]["_PageName"] is DBNull ? "" : dt.Rows[0]["_PageName"].ToString()
                        };
                        PageMasters.Add(pageMaster);
                    }
                    _res.PageMasters = PageMasters;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _res.WUserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetWebsiteInfoEComm";
    }
}
