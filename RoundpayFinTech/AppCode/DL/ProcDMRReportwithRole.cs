using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDMRReportwithRole : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcDMRReportwithRole(IDAL dal)
        {
            _dal = dal;
        }
        public string GetName()
        {
            return "Proc_DMRReportwithRole";
        }
        public async Task<object> Call(object obj)
        {
            _RechargeReportFilter _req = (_RechargeReportFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Type", _req.Status),
                new SqlParameter("@OutletNo", _req.OutletNo ?? ""),
                new SqlParameter("@TransactionID", _req.TransactionID ?? ""),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@APIRequestID", _req.APIRequestID ?? ""),
                new SqlParameter("@AccountNo", _req.AccountNo ?? ""),
                new SqlParameter("@UserOutletMobile", _req.UserOutletMobile ?? ""),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@VendorID", _req.VendorID ?? ""),
                new SqlParameter("@FromDate", string.IsNullOrEmpty(_req.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate),
                new SqlParameter("@ToDate", string.IsNullOrEmpty(_req.ToDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.ToDate),
                new SqlParameter("@TopRows", _req.TopRows == 0 ? 50 : _req.TopRows),
                new SqlParameter("@SenderMobile", _req.SenderMobile ?? ""),
                 new SqlParameter("@CCMobileNo", _req.CCMobileNo??""),
                new SqlParameter("@UserID", _req.UserID)
            };
            var _alist = new List<DMRReportResponse>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);

                if (!dt.Columns.Contains("Msg"))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new DMRReportResponse
                        {
                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            TID = Convert.ToInt32(dt.Rows[i]["_TID"]),
                            TransactionID = dt.Rows[i]["_TransactionID"].ToString(),
                            _Type = Convert.ToInt32(dt.Rows[i]["_Type"]),
                            RefundStatus = Convert.ToInt32(dt.Rows[i]["_RefundStatus"]),
                            UserID = Convert.ToInt32(dt.Rows[i]["_UserID"]),
                            OutletNo = dt.Rows[i]["_OutletNo"].ToString(),
                            Outlet = dt.Rows[i]["_OutletName"] is DBNull ? "" : dt.Rows[i]["_OutletName"].ToString(),
                            Account = dt.Rows[i]["_Account"].ToString(),
                            OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"]),
                            CommType = dt.Rows[i]["_CommType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_CommType"]),
                            Commission = dt.Rows[i]["_CommAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_CommAmount"]),
                            Operator = dt.Rows[i]["_Operator"] is DBNull ? "" : dt.Rows[i]["_Operator"].ToString(),
                            LastBalance = Convert.ToDecimal(dt.Rows[i]["_LastBalance"]),
                            RequestedAmount = Convert.ToDecimal(dt.Rows[i]["_RequestedAmount"]),
                            Amount = Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                            FixedCharge = dt.Rows[i]["_CCF"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_CCF"]),
                            Charge = dt.Rows[i]["_Surcharge"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_Surcharge"]),
                            GSTAfterCharge = dt.Rows[i]["_RefundGST"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_RefundGST"]),
                            TDSAfterCharge = dt.Rows[i]["_TDS"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_TDS"]),
                            AmtWithTDS = dt.Rows[i]["_AmtWithTDS"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_AmtWithTDS"]),
                            CreditedAmount = dt.Rows[i]["_Credited_Amount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_Credited_Amount"]),
                            Balance = Convert.ToDecimal(dt.Rows[i]["_Balance"]),
                            EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                            API = dt.Rows[i]["_API"] is DBNull ? "" : dt.Rows[i]["_API"].ToString(),
                            LiveID = dt.Rows[i]["_LiveID"] is DBNull ? "" : dt.Rows[i]["_LiveID"].ToString(),
                            VendorID = dt.Rows[i]["_VendorID"] is DBNull ? "" : dt.Rows[i]["_VendorID"].ToString(),
                            Optional1 = dt.Rows[i]["_Optional1"] is DBNull ? "" : dt.Rows[i]["_Optional1"].ToString(),
                            Optional2 = dt.Rows[i]["_Optional2"] is DBNull ? "" : dt.Rows[i]["_Optional2"].ToString(),
                            Optional3 = dt.Rows[i]["_Optional3"] is DBNull ? "" : dt.Rows[i]["_Optional3"].ToString(),
                            Optional4 = dt.Rows[i]["_Optional4"] is DBNull ? "" : dt.Rows[i]["_Optional4"].ToString(),
                            ModifyDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "-" : dt.Rows[i]["_ModifyDate"].ToString(),
                            ApiRequestID = dt.Rows[i]["_ApiRequestID"] is DBNull ? "" : dt.Rows[i]["_ApiRequestID"].ToString(),
                            SenderMobile = dt.Rows[i]["SenderMobile"] is DBNull ? "" : dt.Rows[i]["SenderMobile"].ToString(),
                            BankName = dt.Rows[i]["_Optional1"] is DBNull ? "" : dt.Rows[i]["_Optional1"].ToString(),
                            OutletUserCompany = dt.Rows[i]["OutletUserName"] is DBNull ? "" : dt.Rows[i]["OutletUserName"].ToString(),
                            GroupID = dt.Rows[i]["_GroupID"] is DBNull ? "" : dt.Rows[i]["_GroupID"].ToString(),
                            SubAdmin = dt.Rows[i]["_SubAdmin"] is DBNull ? "" : dt.Rows[i]["_SubAdmin"].ToString(),
                            SAMobile = dt.Rows[i]["_SAMobile"] is DBNull ? "" : dt.Rows[i]["_SAMobile"].ToString(),
                            SACommType = dt.Rows[i]["_SACommType"] is DBNull ? "" : dt.Rows[i]["_SACommType"].ToString(),
                            SASlabCommType = dt.Rows[i]["_SASlabCommType"] is DBNull ? "" : dt.Rows[i]["_SASlabCommType"].ToString(),
                            SAComm = dt.Rows[i]["_SAComm"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_SAComm"]),
                            SAGST = dt.Rows[i]["_SAGSTTaxAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_SAGSTTaxAmount"]),
                            SATDS = dt.Rows[i]["_SATDS"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_SATDS"]),
                            MasterDistributer = dt.Rows[i]["_MasterDistributer"] is DBNull ? "" : dt.Rows[i]["_MasterDistributer"].ToString(),
                            MDMobile = dt.Rows[i]["_MDMobile"] is DBNull ? "" : dt.Rows[i]["_MDMobile"].ToString(),
                            MDCommType = dt.Rows[i]["_MDCommType"] is DBNull ? "" : dt.Rows[i]["_MDCommType"].ToString(),
                            MDSlabCommType = dt.Rows[i]["_MDSlabCommType"] is DBNull ? "" : dt.Rows[i]["_MDSlabCommType"].ToString(),
                            MDComm = dt.Rows[i]["_MDComm"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_MDComm"]),
                            MDGST = dt.Rows[i]["_MDGSTTaxAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_MDGSTTaxAmount"]),
                            MDTDS = dt.Rows[i]["_MDTDS"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_MDTDS"]),
                            Distributer = dt.Rows[i]["_Distributer"] is DBNull ? "" : dt.Rows[i]["_Distributer"].ToString(),
                            DTMobile = dt.Rows[i]["_DTMobile"] is DBNull ? "" : dt.Rows[i]["_DTMobile"].ToString(),
                            DTCommType = dt.Rows[i]["_DTCommType"] is DBNull ? "" : dt.Rows[i]["_DTCommType"].ToString(),
                            DTSlabCommType = dt.Rows[i]["_DTSlabCommType"] is DBNull ? "" : dt.Rows[i]["_DTSlabCommType"].ToString(),
                            DTComm = dt.Rows[i]["_DTComm"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_DTComm"]),
                            DTGST = dt.Rows[i]["_DTGSTTaxAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_DTGSTTaxAmount"]),
                            DTTDS = dt.Rows[i]["_DTTDS"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_DTTDS"]),
                            Role = dt.Rows[i]["_Role"] is DBNull ? "" : dt.Rows[i]["_Role"].ToString(),
                            CCName = dt.Rows[i]["_Customercare"] is DBNull ? "" : dt.Rows[i]["_Customercare"].ToString(),
                            CCMobile = dt.Rows[i]["_CCMobileNo"] is DBNull ? "" : dt.Rows[i]["_CCMobileNo"].ToString()
                        };
                        item.SlabCommType = item.CommType ? "COM" : "SUR";
                        item.Type_ = RechargeRespType.GetRechargeStatusText(item._Type);
                        if (_req.IsExport && item._Type == RechargeRespType.REQUESTSENT)
                        {
                            item.Type_ = RechargeRespType._PENDING;
                        }

                        item.RefundStatus_ = RefundType.GetRefundTypeText(item.RefundStatus);
                        _alist.Add(item);
                    }

                }
            }
            catch (Exception er)
            { }
            return _alist;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }
    }
}
