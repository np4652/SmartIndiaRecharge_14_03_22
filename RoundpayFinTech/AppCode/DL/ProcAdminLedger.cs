using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAdminLedger : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAdminLedger(IDAL dal) => _dal = dal;
        public string GetName() => "proc_AdminLedger";
        public object Call(object obj)
        {
            var _req = (ProcAdminLedgerRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@DebitCredit_F", _req.DebitCredit_F),
                new SqlParameter("@Service_F", _req.Service_F),
                new SqlParameter("@TransactionId_F", _req.TransactionId_F ?? ""),
                new SqlParameter("@FromDate_F", string.IsNullOrEmpty(_req.FromDate_F) ? DateTime.Now.ToString("dd MMM yyyy") :_req.FromDate_F),
                new SqlParameter("@ToDate_F", string.IsNullOrEmpty(_req.ToDate_F) ? DateTime.Now.ToString("dd MMM yyyy") : _req.ToDate_F),
                new SqlParameter("@Mobile_F", _req.Mobile_F ?? ""),
                new SqlParameter("@TopRows", _req.TopRows),
                new SqlParameter("@CCID", _req.CCID),
                new SqlParameter("@CCMobileNo", _req.CCMobileNo??""),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@WalletTypeID", _req.WalletTypeID),
                new SqlParameter("@URT_F", _req.UTR_F),
            };
            var _alist = new List<ProcAdminLedgerResponse>();
            var _res = new ProcAdminLedgerResponse
            {
                ResultCode = ErrorCodes.Minus1,
                Msg =ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Columns.Contains("Msg"))
                {
                    _res.ResultCode = ErrorCodes.Minus1;
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new ProcAdminLedgerResponse
                        {
                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"]),
                            Remark = Convert.ToString(dt.Rows[i]["_Remark"]),
                            MobileNo = Convert.ToString(dt.Rows[i]["_MobileNo"]),
                            LastBalance = dt.Rows[i]["_LastAmount"] is DBNull ? 0.0M : Convert.ToDecimal(dt.Rows[i]["_LastAmount"]),
                            Credit = dt.Rows[i]["_Credit"] is DBNull ? 0.0M : Convert.ToDecimal(dt.Rows[i]["_Credit"]),
                            Debit = dt.Rows[i]["_Debit"] is DBNull ? 0.0M : Convert.ToDecimal(dt.Rows[i]["_Debit"]),
                            CurentBalance = dt.Rows[i]["_CurrentAmount"] is DBNull ? 0.0M : Convert.ToDecimal(dt.Rows[i]["_CurrentAmount"]),
                            TrDate = Convert.ToString(dt.Rows[i]["_EntryDateN"]),
                            EntryDate = Convert.ToString(dt.Rows[i]["_EntryDate"]),
                            TID = Convert.ToString(dt.Rows[i]["_TID"]),
                            Description = Convert.ToString(dt.Rows[i]["_Description"]),
                            WalletID = dt.Rows[i]["_WalletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_WalletID"]),
                            UTR = dt.Rows[i]["_UTR"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[i]["_UTR"]),
                            BankName = dt.Rows[i]["_BankName"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[i]["_BankName"]),
                            ToUserID = dt.Rows[i]["_OtherUserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OtherUserID"]),
                            OutletName = dt.Rows[i]["_OutletName"] is DBNull ? string.Empty : dt.Rows[i]["_OutletName"].ToString(),
                            Role = dt.Rows[i]["_Role"] is DBNull ? string.Empty : dt.Rows[i]["_Role"].ToString()
                        };
                        _alist.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                //var _ = _err.FnErrorLog(GetType().Name,"Call",ex.Message,_req.LoginID);
            }
            return _alist;
        }
        public object Call() => throw new NotImplementedException();
    }
}
