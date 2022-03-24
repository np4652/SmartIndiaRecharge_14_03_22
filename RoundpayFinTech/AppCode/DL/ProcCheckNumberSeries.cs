using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCheckNumberSeries : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCheckNumberSeries(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Number", _req.CommonStr ?? ""),
                new SqlParameter("@MobileNumber", _req.CommonStr ?? ""),
                new SqlParameter("@SPKey", _req.CommonStr2 ?? "")
            };
            var _res = new HLRResponseStatus
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
                        _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                        _res.Msg = dt.Rows[0]["Msg"].ToString();
                        if (_res.Statuscode == ErrorCodes.One)
                        {
                            _res.CommonInt = dt.Rows[0]["OID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["OID"]);
                            _res.CommonInt2 = dt.Rows[0]["CircleID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["CircleID"]);
                            _res.CommonStr2 = dt.Rows[0]["_Operator"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_Operator"]);
                            _res.Status = dt.Rows[0]["_OpGroupID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OpGroupID"]);
                            _res.CommonBool2 = dt.Rows[0]["_IsCircleOnly"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCircleOnly"]);
                        }
                    }
                    if (ds.Tables.Count > 1)
                    {
                        var dtAPI = ds.Tables[1];
                        foreach (DataRow dr in dtAPI.Rows)
                        {
                            apiL.Add(new HLRAPIDetails {
                                APIID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                APIURL = dr["_URL"] is DBNull ? string.Empty : dr["_URL"].ToString(),
                                APICode = dr["_APICode"] is DBNull ? string.Empty : dr["_APICode"].ToString()
                            });
                        }
                        _res.HLRAPIs = apiL;
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
        public string GetName() => "proc_CheckNumberSeries";
    }
}