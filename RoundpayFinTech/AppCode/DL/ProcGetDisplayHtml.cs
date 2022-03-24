using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDisplayHtml : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDisplayHtml(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WID", _req.CommonInt),
                new SqlParameter("@ThemeID", _req.CommonInt2)
            };
            var response = new HomeDisplay();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    response.FullPage = Convert.ToString(dt.Rows[0]["FullPage"]);
                    if (String.IsNullOrEmpty(response.FullPage))
                    {
                        response.AboutUs = Convert.ToString(dt.Rows[0]["AboutUs"]);
                        response.Home = Convert.ToString(dt.Rows[0]["Home"]);
                        response.ContactUs = Convert.ToString(dt.Rows[0]["ContactUs"]);
                        response.Services = Convert.ToString(dt.Rows[0]["Services"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetDisplayHtml";
    }
}
