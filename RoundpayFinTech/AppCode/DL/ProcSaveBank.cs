using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveBank : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveBank(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (Bank)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.EntryBy),
                new SqlParameter("@ID", _req.ID),
                new SqlParameter("@BankID", _req.BankID),
                new SqlParameter("@BranchName", _req.BranchName),
                new SqlParameter("@AccountHolder", _req.AccountHolder),
                new SqlParameter("@AccountNo", _req.AccountNo),//@AccountNo
                new SqlParameter("@IFSCCode", _req.IFSCCode),
                new SqlParameter("@Charge", _req.Charge),
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@IsQrEnable", _req.ISQRENABLE),
                new SqlParameter("@NeftID", _req.NeftID),
                new SqlParameter("@ThirdPartyTransferID", _req.ThirdPartyTransferID),
                new SqlParameter("@CashDepositID", _req.CashDepositID),
                new SqlParameter("@GCCID", _req.GCCID),
                new SqlParameter("@ChequeID", _req.ChequeID),
                new SqlParameter("@ScanPayID", _req.ScanPayID),
                new SqlParameter("@UPIID", _req.UPIID),
                new SqlParameter("@ExchangeID", _req.ExchangeID),
                new SqlParameter("@NeftStatus", _req.NeftStatus),
                new SqlParameter("@ThirdPartyTransferStatus", _req.ThirdPartyTransferStatus),
                new SqlParameter("@CashDepositStatus", _req.CashDepositStatus),
                new SqlParameter("@GCCStatus", _req.GCCStatus),
                new SqlParameter("@ChequeStatus", _req.ChequeStatus),
                new SqlParameter("@ScanPayStatus", _req.ScanPayStatus),
                new SqlParameter("@UPIStatus", _req.UPIStatus),
                new SqlParameter("@ExchangeStatus", _req.ExchangeStatus),
                new SqlParameter("@IsBankLogo", _req.IsbankLogoAvailable),
                new SqlParameter("@RImageUrl", _req.ISQRENABLE ? _req.RImageUrl : ""),
                new SqlParameter("@UPINumber", _req.UPIStatus==true?_req.UPINUmber:""),
                new SqlParameter("@IsVirtual", _req.IsVirtual),
                new SqlParameter("@Remark", _req.Remark),
                new SqlParameter("@CDMID", _req.CDMID),
                new SqlParameter("@CDM", _req.CDM),
                new SqlParameter("@CDMType", _req.CDMType),
                new SqlParameter("@CDMCharges", _req.CDMCharges)
        };
            var _resp = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    _resp.CommonInt = dt.Rows[0]["_BankID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_BankID"]);
                    _resp.CommonStr = _req.ID > 0 && !_req.ISQRENABLE ? _req.RImageUrl : "";
                }
                if (!string.IsNullOrEmpty(_resp.CommonStr))
                {
                    StringBuilder filePath = new StringBuilder();
                    filePath.Append(DOCType.BankQRPath);
                    filePath.Append(_resp.CommonStr);
                    if (File.Exists(filePath.ToString()))
                    {
                        File.Delete(filePath.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.EntryBy
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_SaveBank";
    }
}
