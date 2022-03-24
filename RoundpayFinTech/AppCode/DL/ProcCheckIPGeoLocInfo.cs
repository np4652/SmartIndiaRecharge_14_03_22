using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCheckIPGeoLocInfo : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCheckIPGeoLocInfo(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@IP", _req.CommonStr ?? ""),
                new SqlParameter("@SPKey", _req.CommonStr2 ?? "")
            };
            var _res = new IPResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var apiL = new List<IPAPIDetails>();
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                        _res.Msg = dt.Rows[0]["Msg"].ToString();
                    }
                    if (ds.Tables.Count > 1)
                    {
                        var dtAPI = ds.Tables[1];
                        foreach (DataRow dr in dtAPI.Rows)
                        {
                            apiL.Add(new IPAPIDetails
                            {
                                APIID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                APIURL = dr["_URL"] is DBNull ? string.Empty : dr["_URL"].ToString(),
                                APICode = dr["_APICode"] is DBNull ? string.Empty : dr["_APICode"].ToString()
                            });
                        }
                        _res.IPGeoAPIs = apiL;
                    }
                }
                
            }
            catch (Exception ex)
            {
                _res.Msg = ex.Message;
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_CheckIPGeoLocInfo";
    }
}