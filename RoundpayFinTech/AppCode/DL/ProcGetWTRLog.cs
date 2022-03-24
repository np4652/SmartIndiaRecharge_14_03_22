
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWTRLog : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWTRLog(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            _RefundLogFilter _req = (_RefundLogFilter)obj;
            SqlParameter[] param = {
             new SqlParameter("@LoginID", _req.LoginID),
             new SqlParameter("@LT", _req.LoginTypeID),
             new SqlParameter("@RefundStatus", _req.Status),
             new SqlParameter("@OutletMobile", _req.OutletNo ?? ""),
             new SqlParameter("@TransactionID", _req.TransactionID ?? ""),
             new SqlParameter("@TID", _req.TID),
             new SqlParameter("@Account", _req.AccountNo ?? ""),
             new SqlParameter("@OID", _req.OID),
             new SqlParameter("@APIID", _req.APIID),
             new SqlParameter("@DateType", _req.DateType == 0 ? 1 : _req.DateType),
             new SqlParameter("@FromDate", string.IsNullOrEmpty(_req.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate),
            new SqlParameter("@ToDate", string.IsNullOrEmpty(_req.ToDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.ToDate),
             new SqlParameter("@Top", _req.TopRows == 0 ? 50 : _req.TopRows),
            new SqlParameter("@RightAccount", _req.RightAccountNo ?? ""),
            };

            List<RefundTransaction> _res = new List<RefundTransaction>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count < 1)
                    return _res;
                if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var transaction = new RefundTransaction
                        {
                            TID = Convert.ToInt32(row["_TID"]),
                            TransactionID = row["_TransactionID"].ToString(),
                            RefundType_ = RefundType.GetRefundTypeText(Convert.ToInt32(row["_RefundStatus"] is DBNull ? 0 : row["_RefundStatus"])),
                            AccountNo = row["_Account"].ToString(),
                            RequestedAmount = Convert.ToDecimal(row["_RequestedAmount"]),
                            EntryDate = row["_RechargeDate"].ToString(),
                            RefundRequestDate = row["_RRDate"] is DBNull ? "" : row["_RRDate"].ToString(),
                            RefundActionDate = row["_AcceptRejectDate"] is DBNull ? "" : row["_AcceptRejectDate"].ToString(),
                            APIName = row["_API"] is DBNull ? "" : row["_API"].ToString(),
                            Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                            OutletMobile = row["OutletMobile"] is DBNull ? "" : row["OutletMobile"].ToString(),
                            OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
                            RStatus = RechargeRespType.GetRechargeStatusText(Convert.ToInt32(row["_Type"] is DBNull ? 0 : row["_Type"])),
                            LiveID = row["_LiveID"] is DBNull ? "" : row["_LiveID"].ToString(),
                            RefundRemark = row["_RefundRemark"] is DBNull ? "" : row["_RefundRemark"].ToString(),
                            VendorID = row["_VendorID"] is DBNull ? "" : row["_VendorID"].ToString(),
                            RightAccountNo = row["_RightAccount"] is DBNull ? "" : row["_RightAccount"].ToString(),
                            RequestMode = row["_RequestMode"] is DBNull ? "" : row["_RequestMode"].ToString()
                        };
                        _res.Add(transaction);
                    }
                }
            }
            catch (Exception)
            {
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetWTRLog";
        }
    }
}