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
    public class ProcGetFundRequestForApproval : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetFundRequestForApproval(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            List<FundRequetResp> resp = new List<FundRequetResp>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            FundRequetResp ll = new FundRequetResp
                            {
                                PaymentId = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                MobileNo = row["IMPSMobile"].ToString(),
                                MODE = row["_MODE"].ToString(),
                                Status = row["_Status"].ToString(),
                                TransactionId = row["_TransactionId"].ToString(),
                                AccountHolder = row["_AccountHolder"].ToString(),
                                AccountNo = row["_AccountNo"].ToString(),
                                Amount = row["_Amount"] is DBNull ? 0M : Convert.ToDecimal(row["_Amount"]),
                                Bank = row["_Bank"].ToString(),
                                CardNumber = row["_CardNumber"].ToString(),
                                ChequeNo = row["_ChequeNo"].ToString(),
                                EntryDate = row["_EntryDate"].ToString(),
                                UserName = row["_OutletName"].ToString(),
                                UserMobile = row["_MobileNo"].ToString(),
                                CommRate = row["_CommRate"] is DBNull ? 0M : Convert.ToDecimal(row["_CommRate"]),
                                UserId = row["_UserId"] is DBNull ? 0 : Convert.ToInt32(row["_UserId"]),
                                ReceiptURL = Convert.ToString(row["_ReceiptURL"]),
                                KYCStatus = Convert.ToInt32(row["KYCStatus"]),
                                RoleName = row["RoleName"].ToString(),
                                WalletTypeID = row["_WalletTypeID"] is DBNull ? 0 : Convert.ToInt32(row["_WalletTypeID"]),
                                WalletType = row["_WalletType"] is DBNull ? string.Empty : row["_WalletType"].ToString()
                            };
                            resp.Add(ll);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetFundRequestForApproval";
        }
    }
}
