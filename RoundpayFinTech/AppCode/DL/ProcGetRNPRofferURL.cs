using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetRNPRofferURL : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRNPRofferURL(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.UserID),
                new SqlParameter("@OID",req.CommonInt),
                new SqlParameter("@MobileNo",req.CommonStr)
            };
            var _resp = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var apiL = new List<HLRAPIDetails>();

            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        _resp.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                        _resp.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    }
                    if (ds.Tables.Count > 1)
                    {
                        var dtAPI = ds.Tables[1];
                        foreach (DataRow dr in dtAPI.Rows)
                        {
                            apiL.Add(new HLRAPIDetails
                            {
                                APIID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                APIURL = dr["_URL"] is DBNull ? string.Empty : dr["_URL"].ToString(),
                                APICode = dr["_APICode"] is DBNull ? string.Empty : dr["_APICode"].ToString()
                            });
                        }
                        _resp.HLRAPIs = apiL;
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetRNPRofferURL";
    }
}
