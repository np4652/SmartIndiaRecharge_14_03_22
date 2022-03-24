using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWebsitePackage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWebsitePackage(IDAL dal) => _dal = dal;
        public string GetName() => "Proc_GetWebsitePackage";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req.CommonInt),
            };
            var res = new WebsiteInfo();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.WID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.WebsiteName = dt.Rows[0]["_WebsiteName"] is DBNull ? "" : dt.Rows[0]["_WebsiteName"].ToString();
                    res.AppPackageID = dt.Rows[0]["_AppPackageID"] is DBNull ? "" : dt.Rows[0]["_AppPackageID"].ToString();
             
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            res.ReffralID = req.CommonInt;
            return res;  
        }
        public object Call() => throw new NotImplementedException();
    }
}
