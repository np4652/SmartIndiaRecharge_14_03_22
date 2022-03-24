using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAPISTATUSCHECKList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAPISTATUSCHECKList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var _res = new List<APISTATUSCHECK>();
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@CheckText",req.CommonStr??""),
                new SqlParameter("@Status",req.CommonInt)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var aPISTATUSCHECK = new APISTATUSCHECK
                            {
                                ID = row["StatusID"] is DBNull ? 0 : Convert.ToInt32(row["StatusID"]),
                                ErrorCode = row["ErrCode"] is DBNull ? "" : row["ErrCode"].ToString(),
                                Status = row["_Status"] is DBNull ? 0 : Convert.ToInt16(row["_Status"]),
                                Checks = row["Checks"] is DBNull ? "" : row["Checks"].ToString(),
                                VendorID = row["VendorID"] is DBNull ? "" : row["VendorID"].ToString(),
                                OperatorID = row["OperatorId"] is DBNull ? "" : row["OperatorId"].ToString(),
                                TransactionID = row["TransactionId"] is DBNull ? "" : row["TransactionId"].ToString(),
                                Balance = row["Balance"] is DBNull ? "" : row["Balance"].ToString(),
                                IndLength = row["IndLength"] is DBNull ? 0 : Convert.ToInt32(row["IndLength"]),
                                EntryDate = row["EntryDate"] is DBNull ? "" : row["EntryDate"].ToString(),
                                ModifyDate = row["ModifyDate"] is DBNull ? "" : row["ModifyDate"].ToString()
                            };
                            _res.Add(aPISTATUSCHECK);
                        }
                    }
                }
            }
            catch{}
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_APISTATUSCHECK_List";
    }
}
