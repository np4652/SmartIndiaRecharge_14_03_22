using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    //public class ProcUpdateWebsites : IProcedure
    //{

    //    public object Call(object obj)
    //    {
    //        var req = (CommonReq)obj;
    //        SqlParameter[] param = {
    //            new SqlParameter("@LoginID",req.LoginID),
    //            new SqlParameter("@LT",req.LoginTypeID),
                

    //        };
    //        var WebsiteList = new List<WebsiteModel>();
    //        try
    //        {
    //            DataTable dt = _dal.GetByProcedure(GetName(), param);
    //            if (dt.Rows.Count > 0)
    //            {

    //                foreach (DataRow row in dt.Rows)
    //                {
    //                    var Web = new WebsiteModel
    //                    {
    //                        ID = row["_UserID"] is DBNull ? 0 : Convert.ToInt16(row["_UserID"]),
    //                        Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
    //                        DomainName = row["_WebsiteName"] is DBNull ? "" : row["_WebsiteName"].ToString(),
    //                        MobileNo = row["_MobileNO"] is DBNull ? "" : row["_MobileNO"].ToString(),
    //                        IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"])
    //                    };
    //                    WebsiteList.Add(Web);
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var errorLog = new ErrorLog
    //            {
    //                ClassName = GetType().Name,
    //                FuncName = "Call",
    //                Error = ex.Message,
    //                LoginTypeID = req.LoginTypeID,
    //                UserId = req.LoginID
    //            };
    //            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
    //        }

    //        return WebsiteList;

    //    }

    //    public object Call()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string GetName()
    //    {
    //        return "Proc_UpdateWebsiteList";
    //    }
    //}



    public class ProcUpdateWebsites : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateWebsites(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            WebsiteModel req = (WebsiteModel)obj;

            SqlParameter[] param = {

                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@WID",req.WID),
                new SqlParameter("@WebsiteName",req.DomainName),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@IsWebSiteUpdate",req.IsWebsiteUpdate),

            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_UpdateWebsiteList";
        }
    }
}
