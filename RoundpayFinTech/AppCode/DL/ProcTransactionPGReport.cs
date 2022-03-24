using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcTransactionPGReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcTransactionPGReport(IDAL dal) => _dal = dal;
        public string GetName() => "Proc_TransactionPGReport";
        public async Task<object> Call(object obj)
        {
            var _req = (TransactionPG)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@FromDate", _req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@TopRows", _req.TopRows),
                new SqlParameter("@VenderID", _req.VendorID),
                new SqlParameter("@TransactionID", _req.TransactionID),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@Type", _req.Type),
                new SqlParameter("@Mobile", _req.MobileNo),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@PGID", _req.PGID),
            };var _alist = new List<TransactionPG>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (!dt.Columns.Contains("Msg"))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new TransactionPG
                        {
                            Statuscode = ErrorCodes.One,
                            Msg = "Success",
                            ID = Convert.ToInt32(dt.Rows[i]["_TID"]),
                            TID = Convert.ToInt32(dt.Rows[i]["_TID"]),
                            UserID = dt.Rows[i]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_UserID"]),
                            UserName = Convert.ToString(dt.Rows[i]["_UserName"]),
                            RequestedAmount = Convert.ToInt32(dt.Rows[i]["_RequestedAmount"] is DBNull ? 0 : dt.Rows[i]["_RequestedAmount"]),
                            Amount = Convert.ToDecimal(dt.Rows[i]["_Amount"] is DBNull ? 0 : dt.Rows[i]["_Amount"]),
                            Type = dt.Rows[i]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_Type"]),
                            PgCharge = dt.Rows[i]["_PgCharge"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_PgCharge"]),
                            PGName = Convert.ToString(dt.Rows[i]["_PGName"]),
                            EntryDate = Convert.ToString(dt.Rows[i]["_EntryDate"]),
                            LastModifyDate = Convert.ToString(dt.Rows[i]["_ModifyDate"]),
                            Operator = Convert.ToString(dt.Rows[i]["_Operator"]),
                            OpType = Convert.ToString(dt.Rows[i]["_OpType"]),
                            OPID = Convert.ToString(dt.Rows[i]["_OPID"]),
                            TransactionID = Convert.ToString(dt.Rows[i]["_TransactionID"]),
                            VendorID = Convert.ToString(dt.Rows[i]["_VendorID"]),
                            LiveID = Convert.ToString(dt.Rows[i]["_LiveID"]),
                            RequestedMode = dt.Rows[i]["_RequestMode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_RequestMode"]),
                            surcharge = dt.Rows[i]["_surcharge"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_surcharge"]),
                            ChargeAmtType = dt.Rows[i]["_ChargeAmtType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ChargeAmtType"]),
                            WalletID = dt.Rows[i]["_WalletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_WalletID"]),
                            UPGID = dt.Rows[i]["_UPGID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_UPGID"]),
                            MobileNo = Convert.ToString(dt.Rows[i]["_MobileNo"])
                        };
                        _alist.Add(item);
                    }
                }
            }
            catch (Exception er)
            {

            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();
    }

    public class ProcTransactionPGLog : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTransactionPGLog(IDAL dal) => _dal = dal;
        public string GetName() => "SELECT _Log ,_Checksum ,[dbo].[CustomFormat](_EntryDate) _EntryDate FROM tbl_TransactionReqRespPG where _TID=@TID";
        public object Call(object obj)
        {
            var _modal = (TransactionPGLogDetail)obj;
            SqlParameter[] param = {
                new SqlParameter("@TID", _modal.TID),
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                List<LogModal> _logDetail = new List<LogModal>();
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new LogModal()
                    {
                        LOG = Convert.ToString(dr["_Log"]),
                        CheckSum = Convert.ToString(dr["_Checksum"]),
                        EntryDate = Convert.ToString(dr["_EntryDate"]),
                    };
                    _logDetail.Add(item);
                }
                _modal.Log = _logDetail;
            }
            catch (Exception ex)
            {

            }
            return _modal;
        }

        public object Call() => throw new NotImplementedException();
    }
}
