using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetFundRequestToRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetFundRequestToRole(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID)
            };
            List<FundRequestToRole> bankMasters = new List<FundRequestToRole>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    FundRequestToRole fundRequestToRole = new FundRequestToRole
                    {
                        ToId = dt.Rows[i]["ToId"] is DBNull? 0: Convert.ToInt32(dt.Rows[i]["ToId"]),
                        ToRole = dt.Rows[i]["ToRole"] is DBNull? string.Empty: dt.Rows[i]["ToRole"].ToString(),
                        FromId = dt.Rows[i]["FromId"] is DBNull? 0: Convert.ToInt32(dt.Rows[i]["FromId"]),
                        FromRole = dt.Rows[i]["FromRole"] is DBNull ? string.Empty : dt.Rows[i]["FromRole"].ToString(),
                        IsUpline = Convert.ToBoolean(dt.Rows[i]["IsUpline"] is DBNull ? false : dt.Rows[i]["IsUpline"]),
                        IsActive = Convert.ToBoolean(dt.Rows[i]["IsActive"] is DBNull ? false : dt.Rows[i]["IsActive"]),

                    };
                    bankMasters.Add(fundRequestToRole);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return bankMasters;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_GetFundRequestToRole";
        }
    }
}

