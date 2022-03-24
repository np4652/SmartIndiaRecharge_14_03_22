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
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCheckBillerInfoAllowed : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCheckBillerInfoAllowed(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt),
                new SqlParameter("@TypeID",req.CommonInt2),
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var resList = new List<ResponseStatus>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode > 0)
                    {
                        if (req.CommonInt == -1)
                        {
                            foreach (DataRow item in dt.Rows)
                            {
                                resList.Add(new ResponseStatus
                                {
                                    CommonInt = item[0] is DBNull ? 0 : Convert.ToInt32(item[0]),
                                    CommonStr = item["_BillerID"] is DBNull ? string.Empty : item["_BillerID"].ToString(),
                                    CommonStr2 = item["_TransactionID"] is DBNull ? string.Empty : item["_TransactionID"].ToString(),
                                    CommonStr3 = item["_APICode"] is DBNull ? string.Empty : item["_APICode"].ToString(),
                                    CommonStr4 = item["_APIOpTypeID"] is DBNull ? string.Empty : item["_APIOpTypeID"].ToString(),
                                    ReffID = item["_RPBillerID"] is DBNull ? string.Empty : item["_RPBillerID"].ToString(),
                                    CommonInt2 = item["_ExactNess"] is DBNull ? 0 : Convert.ToInt16(item["_ExactNess"])
                                });
                            }
                        }
                        else
                        {
                            res.CommonStr = dt.Rows[0]["_BillerID"] is DBNull ? string.Empty : dt.Rows[0]["_BillerID"].ToString();
                            res.CommonStr2 = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                            res.CommonStr3 = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                            res.CommonStr4 = dt.Rows[0]["_APIOpTypeID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOpTypeID"].ToString();
                            res.ReffID = dt.Rows[0]["_RPBillerID"] is DBNull ? string.Empty : dt.Rows[0]["_RPBillerID"].ToString();
                            res.CommonInt2 = dt.Rows[0]["_ExactNess"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ExactNess"]);
                        }
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
            if (req.CommonInt == -1)
                return resList;
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_CheckBillerInfoAllowed";
    }
}
