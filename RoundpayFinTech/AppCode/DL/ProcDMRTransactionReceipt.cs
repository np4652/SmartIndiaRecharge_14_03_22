using Fintech.AppCode.Configuration;
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
    public class ProcDMRTransactionReceipt : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDMRTransactionReceipt(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@GroupID", _req.CommonStr??string.Empty),
        };
            var TranDetail = new TransactionDetail();
            var lst = new List<DMRReceiptDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {

                        TranDetail.WID = Convert.ToInt32(dt.Rows[0]["_WID"]);
                        TranDetail.SupportEmail = dt.Rows[0]["_EmailIDSupport"].ToString();
                        TranDetail.Email = dt.Rows[0]["_EmailID"].ToString();
                        TranDetail.UserID = Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        TranDetail.MobileNo = dt.Rows[0]["_MobileNo"].ToString();
                        TranDetail.PinCode = dt.Rows[0]["_PinCode"].ToString();
                        TranDetail.Address = dt.Rows[0]["_Address"].ToString();
                        TranDetail.Name = dt.Rows[0]["_Name"].ToString();
                        TranDetail.BankName = dt.Rows[0]["_Optional1"].ToString();
                        TranDetail.SenderNo = dt.Rows[0]["_Optional2"].ToString();
                        TranDetail.Channel = dt.Rows[0]["_Optional4"].ToString();
                        TranDetail.IFSC = dt.Rows[0]["_Optional3"].ToString();
                        TranDetail.BeneName = dt.Rows[0]["_ExtraParam"].ToString();
                        TranDetail.Account = dt.Rows[0]["_Account"].ToString();
                        TranDetail.CompanyAddress = Convert.ToString(dt.Rows[0]["_CompanyAddress"]);
                        TranDetail.EntryDate = Convert.ToDateTime(dt.Rows[0]["_EntryDate"]);
                        TranDetail.convenientFee = _req.CommonDecimal;
                        TranDetail.SenderName = dt.Rows[0]["_SenderName"] is DBNull ? string.Empty : dt.Rows[0]["_SenderName"].ToString();
                        TranDetail.CompanyName = dt.Rows[0]["_CName"].ToString();
                        TranDetail.CompanyMobile = dt.Rows[0]["_CMob"].ToString();
                        TranDetail.CompanyEmail = dt.Rows[0]["_CEmail"].ToString();
                        foreach (DataRow item in dt.Rows)
                        {
                            var detail = new DMRReceiptDetail
                            {
                                TransactionID = item["_TransactionID"].ToString(),
                                TID = Convert.ToInt32(item["_TID"]),
                                LiveID = item["_LiveID"].ToString(),
                                RequestedAmount = Convert.ToDecimal(item["_RequestedAmount"].ToString()),
                                Statuscode = Convert.ToInt32(item["_Type"]),
                                Status = RechargeRespType.GetRechargeStatusText(Convert.ToInt32(item["_Type"]))
                            };
                            if (detail.Statuscode.In(RechargeRespType.PENDING, RechargeRespType.REQUESTSENT))
                            {
                                detail.Status = "PROCESSING";
                            }
                            lst.Add(detail);
                        }
                        TranDetail.lists = lst;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return TranDetail;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_DMRTransactionReceipt";
    }
}
