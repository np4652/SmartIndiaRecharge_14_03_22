using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace RoundpayFinTech.AppCode.DL
{
   
    public class ProcSelectNewWhatsappMsg : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSelectNewWhatsappMsg(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new WhatsappResponse
            {
                statuscode = ErrorCodes.Minus1,
                msg = ErrorCodes.TempError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@MobileNo",req.CommonStr)
            };
            try
            {
             DataSet ds=_dal.GetByProcedureAdapterDS(GetName(),param);
                if (ds.Tables.Count>0)
                {
                    res.msg = "New Message";
                  
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        res.statuscode = 2;
                    }
                    else if (ds.Tables[0].Rows.Count > 0)
                    {
                        res.statuscode = 1;
                    }
                    else {
                        res.statuscode = -1;
                    }
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
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_SelectNewWhatsappMsg";
    }
    public class ProcGetWhatsappAPI : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWhatsappAPI(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int apiidid = (int)obj;

            var res = new WhatsappAPI();
            try
            {
                string query = "select _ID,_ApiCode,_URL,_GroupListURL,_GroupDetailURL,_Name from tbl_SMSAPI  where _IsWhatsApp=1 and _ID= " + apiidid.ToString();
                DataTable dt = _dal.Get(query);
                if (dt.Rows.Count > 0)
                {
                    res.APIID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                            res.APICODE = dt.Rows[0]["_ApiCode"] is DBNull ? string.Empty : dt.Rows[0]["_ApiCode"].ToString();
                    res.APINAME = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                    res.APIURL = dt.Rows[0]["_URL"] is DBNull ? string.Empty : dt.Rows[0]["_URL"].ToString();
                    res.GroupListUrl = dt.Rows[0]["_GroupListURL"] is DBNull ? string.Empty : dt.Rows[0]["_GroupListURL"].ToString();
                    res.GroupDetailUrl = dt.Rows[0]["_GroupDetailURL"] is DBNull ? string.Empty : dt.Rows[0]["_GroupDetailURL"].ToString();
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
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "";
    }
}
