using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBankByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBankByID(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@LT", _req.LoginTypeID),
                 new SqlParameter("@ID", _req.CommonInt)
        };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    int i = 0;
                    Bank bank = new Bank()
                    {
                        ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                        BankID = Convert.ToInt32(dt.Rows[i]["_BankID"]),
                        BankName = dt.Rows[i]["_Bank"].ToString(),
                        BranchName = dt.Rows[i]["_BranchName"].ToString(),
                        AccountHolder = dt.Rows[i]["_AccountHolder"].ToString(),
                        AccountNo = dt.Rows[i]["_AccountNo"].ToString(),
                        IFSCCode = dt.Rows[i]["_IFSCCode"].ToString(),
                        Charge = Convert.ToDecimal(dt.Rows[i]["_Charge"]),
                        Logo = dt.Rows[i]["_Logo"].ToString(),
                        ISQRENABLE = dt.Rows[i]["IsQREnable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["IsQREnable"]),
                        NeftStatus = dt.Rows[i]["_NeftStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_NeftStatus"]),
                        ThirdPartyTransferStatus = dt.Rows[i]["_ThirdPartyTransferStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_ThirdPartyTransferStatus"]),
                        CashDepositStatus = dt.Rows[i]["_CashDepositStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_CashDepositStatus"]),
                        GCCStatus = dt.Rows[i]["_GCCStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_GCCStatus"]),
                        ChequeStatus = dt.Rows[i]["_ChequeStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_ChequeStatus"]),
                        ScanPayStatus = dt.Rows[i]["_ScanPayStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_ScanPayStatus"]),
                        UPIStatus = dt.Rows[i]["_UPIStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_UPIStatus"]),
                        ExchangeStatus = dt.Rows[i]["_ExchangeStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_ExchangeStatus"]),
                        UPINUmber = Convert.ToString(dt.Rows[i]["_UPINUmber"]),
                        RImageUrl = Convert.ToString(dt.Rows[i]["_RImageUrl"]),
                        QRPath = "Image/BankQR/" + (dt.Rows[i]["_RImageUrl"] is DBNull ? "DefaultQR.png" : dt.Rows[i]["_RImageUrl"].ToString()),
                        IsVirtual = dt.Rows[i]["_IsVirtual"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsVirtual"]),
                        Remark = dt.Rows[i]["_Remark"].ToString(),
                        CDM = dt.Rows[i]["_CDM"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_CDM"]),
                        CDMType = dt.Rows[i]["_CDMType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_CDMType"]),
                        CDMCharges = dt.Rows[i]["_CDMCharges"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_CDMCharges"]),
                    };
                    return bank;
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
            return new Bank();
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetBankByID";
        }

    }
}
