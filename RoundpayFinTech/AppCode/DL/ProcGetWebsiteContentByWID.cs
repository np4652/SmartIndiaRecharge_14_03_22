using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWebsiteContentByWID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWebsiteContentByWID(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WID",req.CommonInt),
            };
            var wlCont = new WebsiteModel();
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    wlCont.RefferalContent = dt.Rows[0]["_ReferralContent"] is DBNull ? "" : dt.Rows[0]["_ReferralContent"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return wlCont;

        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "select * from Master_website where _ID=@WID";
    }


}
