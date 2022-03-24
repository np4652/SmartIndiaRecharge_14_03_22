using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserFundRequestReport : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserFundRequestReport(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var _req = (FundOrderFilter)obj;
            var _res = new List<FundRequetResp>();

            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@FDate", _req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@TDate", _req.ToDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@UMobileNo", _req.UMobile ?? ""),
                new SqlParameter("@TMode", _req.TMode),
                new SqlParameter("@RSts", _req.RSts),
                new SqlParameter("@AccountNo", _req.AccountNo ?? ""),
                new SqlParameter("@TransactionID", _req.TransactionID ?? ""),
                new SqlParameter("@Top", _req.Top),
                new SqlParameter("@IsSelf", _req.IsSelf),
                 new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@CCID", _req.CCID),
                new SqlParameter("@CCMobileNo", _req.CCMobileNo??""),
                new SqlParameter("@WalletTypeId",_req.WalletTypeID)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var fundRequetResp = new FundRequetResp
                            {
                                PaymentId = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"]),
                                MobileNo = dt.Rows[i]["IMPSMobile"].ToString(),
                                MODE = dt.Rows[i]["_MODE"].ToString(),
                                Status = dt.Rows[i]["_Status"].ToString(),
                                TransactionId = dt.Rows[i]["_TransactionId"].ToString(),
                                AccountHolder = dt.Rows[i]["_AccountHolder"].ToString(),
                                AccountNo = dt.Rows[i]["_AccountNo"].ToString(),
                                Amount = dt.Rows[i]["_Amount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                                Bank = dt.Rows[i]["_Bank"].ToString(),
                                CardNumber = Convert.ToString(dt.Rows[i]["_CardNumber"]),
                                ChequeNo = Convert.ToString(dt.Rows[i]["_ChequeNo"]),
                                EntryDate = dt.Rows[i]["_EntryDate"].ToString(),
                                UserName = Convert.ToString(dt.Rows[i]["_OutletName"]),
                                UserMobile = Convert.ToString(dt.Rows[i]["_MobileNo"]),
                                Remark= Convert.ToString(dt.Rows[i]["_Remark"]),
                                ApproveDate=Convert.ToString(dt.Rows[i]["_ApproveDate"]),
                                ApproveName =Convert.ToString(dt.Rows[i]["_ApproveName"]),
                                ApproveMobile = Convert.ToString(dt.Rows[i]["_ApproveMobile"]),
                                UPIID = Convert.ToString(dt.Rows[i]["_UPIID"]),
                                Branch = Convert.ToString(dt.Rows[i]["_Branch"]),
                            };
                            _res.Add(fundRequetResp);
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetFundRequest";
    }
}
