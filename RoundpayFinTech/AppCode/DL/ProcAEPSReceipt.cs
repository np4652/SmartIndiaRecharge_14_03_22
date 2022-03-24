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
    public class ProcAEPSReceipt : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAEPSReceipt(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@TransactionID", _req.CommonInt),
        };
            var TranDetail = new TransactionDetail();
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
                        TranDetail.OID = Convert.ToInt32(dt.Rows[0]["_OID"]);
                        TranDetail.OPName = dt.Rows[0]["_OPName"].ToString();
                        TranDetail.DisplayAccount = dt.Rows[0]["_DisplayAccount"].ToString();
                        TranDetail.BankName = dt.Rows[0]["_ExtraParam"].ToString();
                        TranDetail.SenderNo = dt.Rows[0]["_Optional2"].ToString();
                        TranDetail.Channel = dt.Rows[0]["_Optional4"].ToString();
                        TranDetail.IFSC = dt.Rows[0]["_Optional3"].ToString();
                        TranDetail.Account = dt.Rows[0]["_Account"].ToString();
                        TranDetail.CompanyAddress = Convert.ToString(dt.Rows[0]["_CompanyAddress"]);
                        TranDetail.EntryDate = Convert.ToDateTime(dt.Rows[0]["_EntryDate"]);
                        TranDetail.convenientFee = _req.CommonDecimal;
                        TranDetail.TransactionID = dt.Rows[0]["_TransactionID"].ToString();
                        TranDetail.TID = Convert.ToInt32(dt.Rows[0]["_TID"]);
                        TranDetail.LiveID = dt.Rows[0]["_LiveID"].ToString();
                        TranDetail.RequestedAmount = Convert.ToDecimal(dt.Rows[0]["_RequestedAmount"].ToString());
                        TranDetail.Statuscode = Convert.ToInt32(dt.Rows[0]["_Type"]);
                        TranDetail.IsBBPS = Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]);
                        TranDetail.Status = RechargeRespType.GetRechargeStatusText(Convert.ToInt32(dt.Rows[0]["_Type"]));
                        TranDetail.CompanyName = dt.Rows[0]["_CompanyName"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_CompanyName"]);
                        TranDetail.CompanyMobile = dt.Rows[0]["_CompanyMobile"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_CompanyMobile"]);
                        TranDetail.CompanyPhone = dt.Rows[0]["_CompanyPhone"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_CompanyPhone"]);
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
        public string GetName() => "proc_AEPSReceipt";
    }
}

