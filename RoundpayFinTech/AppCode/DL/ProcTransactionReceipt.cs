using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcTransactionReceipt : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTransactionReceipt(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@TransactionID", _req.CommonInt),
        };
            var TranDetail = new TransactionDetail
            {
                IsError = true,
                Status = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        TranDetail.IsError = false;
                        TranDetail.WID = Convert.ToInt32(dt.Rows[0]["_WID"]);
                        TranDetail.SupportEmail = dt.Rows[0]["_EmailIDSupport"] is DBNull ? string.Empty : dt.Rows[0]["_EmailIDSupport"].ToString();
                        TranDetail.PhoneNoSupport = dt.Rows[0]["_PhoneNoSupport"] is DBNull ? string.Empty : dt.Rows[0]["_PhoneNoSupport"].ToString();
                        TranDetail.MobileSupport = dt.Rows[0]["_MobileSupport"] is DBNull ? string.Empty : dt.Rows[0]["_MobileSupport"].ToString();
                        TranDetail.Email = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                        TranDetail.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        TranDetail.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                        TranDetail.PinCode = dt.Rows[0]["_PinCode"] is DBNull ? string.Empty : dt.Rows[0]["_PinCode"].ToString();
                        TranDetail.Address = dt.Rows[0]["_Address"] is DBNull ? string.Empty : dt.Rows[0]["_Address"].ToString();
                        TranDetail.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                        TranDetail.OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]);
                        TranDetail.OPName = dt.Rows[0]["_OPName"] is DBNull ? string.Empty : dt.Rows[0]["_OPName"].ToString();
                        TranDetail.DisplayAccount = dt.Rows[0]["_DisplayAccount"] is DBNull ? string.Empty : dt.Rows[0]["_DisplayAccount"].ToString();
                        TranDetail.BankName = dt.Rows[0]["_Optional1"] is DBNull ? string.Empty : dt.Rows[0]["_Optional1"].ToString();
                        TranDetail.SenderNo = dt.Rows[0]["_Optional2"] is DBNull ? string.Empty : dt.Rows[0]["_Optional2"].ToString();
                        TranDetail.Channel = dt.Rows[0]["_Optional4"] is DBNull ? string.Empty : dt.Rows[0]["_Optional4"].ToString();
                        TranDetail.IFSC = dt.Rows[0]["_Optional3"] is DBNull ? string.Empty : dt.Rows[0]["_Optional3"].ToString();
                        TranDetail.BeneName = dt.Rows[0]["_ExtraParam"] is DBNull ? string.Empty : dt.Rows[0]["_ExtraParam"].ToString();
                        TranDetail.Account = dt.Rows[0]["_Account"] is DBNull ? string.Empty : dt.Rows[0]["_Account"].ToString();
                        TranDetail.CompanyAddress = dt.Rows[0]["_CompanyAddress"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_CompanyAddress"]);
                        TranDetail.EntryDate = Convert.ToDateTime(dt.Rows[0]["_EntryDate"]);
                        TranDetail.convenientFee = _req.CommonDecimal;
                        TranDetail.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        TranDetail.TID = dt.Rows[0]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TID"]);
                        TranDetail.LiveID = dt.Rows[0]["_LiveID"].ToString();
                        TranDetail.RequestedAmount = dt.Rows[0]["_RequestedAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[0]["_RequestedAmount"].ToString());
                        TranDetail.Statuscode = dt.Rows[0]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Type"]);
                        TranDetail.IsBBPS = dt.Rows[0]["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]);
                        TranDetail.Status = RechargeRespType.GetRechargeStatusText(Convert.ToInt32(dt.Rows[0]["_Type"]));

                        TranDetail.CompanyName = dt.Rows[0]["_CompanyName"] is DBNull ? string.Empty : dt.Rows[0]["_CompanyName"].ToString();
                        TranDetail.O15 = dt.Rows[0]["_O15"] is DBNull ? string.Empty : dt.Rows[0]["_O15"].ToString();
                        TranDetail.O16 = dt.Rows[0]["_O16"] is DBNull ? string.Empty : dt.Rows[0]["_O16"].ToString();
                        TranDetail.O17 = dt.Rows[0]["_O17"] is DBNull ? string.Empty : dt.Rows[0]["_O17"].ToString();
                        TranDetail.CustomerMobile = dt.Rows[0]["_CustomerNo"] is DBNull ? string.Empty : dt.Rows[0]["_CustomerNo"].ToString();
                        TranDetail.BillerID = dt.Rows[0]["_BillerID"] is DBNull ? string.Empty : dt.Rows[0]["_BillerID"].ToString();
                        TranDetail.AccountNoKey = dt.Rows[0]["_AccountNoKey"] is DBNull ? string.Empty : dt.Rows[0]["_AccountNoKey"].ToString();
                    }
                    else
                    {
                        TranDetail.Status = "No Record Exists!";
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
        public string GetName() => "proc_TransactionReceipt";
    }
}

