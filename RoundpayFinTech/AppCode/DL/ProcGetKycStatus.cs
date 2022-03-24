using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetKycStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetKycStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@UserID", req.CommonInt),
        };
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.KYCStatus = dt.Rows[0]["_KYCStatus"] is DBNull ? -1 : Convert.ToInt16(dt.Rows[0]["_KYCStatus"]);
                    _res.LoginID = req.LoginID;
                    _res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                    _res.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                    _res.UserMobileNo = Convert.ToString(dt.Rows[0]["_MobileNo"]);
                    _res.UserEmailID = Convert.ToString(dt.Rows[0]["_EmailID"]);
                    _res.UserFCMID = Convert.ToString(dt.Rows[0]["_FCMID"]);
                    _res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                    _res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                    _res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                    _res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                    _res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                    _res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                    _res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                    _res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                    _res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountContact"]);
                    _res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                    _res.WhatsappNo = Convert.ToString(dt.Rows[0]["_WhatsappNo"]);
                    _res.HangoutNo = Convert.ToString(dt.Rows[0]["_HangoutId"]);
                    _res.TelegramNo = Convert.ToString(dt.Rows[0]["_TelegramNo"]);
                    _res.OutletID = Convert.ToString(dt.Rows[0]["_OutletID"]);
                    _res.OutletMobile = Convert.ToString(dt.Rows[0]["_OutletMobile"]);
                    _res.KycRejectReason = Convert.ToString(dt.Rows[0]["_Reason"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetKYCStatus";
        /* "select o._Name,o._KYCStatus,o._MobileNo,o._EmailID,u._FCMID,c._Name _CompanyName from tbl_OutletsOfUser o,tbl_UsersLogin u,tbl_CompanyProfile c where u._UserID=o._UserID And o._UserID=@UserID and c._WID=u._WID";*/
    }
}
