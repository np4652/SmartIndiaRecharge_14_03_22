using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBirthdayWishAlert : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBirthdayWishAlert(IDAL dal) => _dal = dal;        
        public object Call(object obj) => throw new NotImplementedException();
        public object Call()
        {
            var _List = new List<AlertReplacementModel>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var read = new AlertReplacementModel
                        {
                            LoginID = 1,
                            UserID = Convert.ToInt32(dr["UserID"]),
                            UserName = Convert.ToString(dr["_UserName"]),
                            UserMobileNo = Convert.ToString(dr["_MobileNo"]),
                            UserEmailID = Convert.ToString(dr["_EmailID"]),
                            UserFCMID = Convert.ToString(dr["_FCMID"]),
                            UserCurrentBalance = dr["_Balance"] is DBNull ? 0 : Convert.ToDecimal(dr["_Balance"]),
                            UserAlertBalance = dr["_AlertBalance"] is DBNull ? 0 : Convert.ToDecimal(dr["_AlertBalance"]),
                            WID = dr["_WID"] is DBNull ? 0 : Convert.ToInt32(dr["_WID"]),
                            Company = Convert.ToString(dr["Company"]),
                            CompanyDomain = Convert.ToString(dr["CompanyDomain"]),
                            CompanyAddress = Convert.ToString(dr["CompanyAddress"]),
                            OutletName = Convert.ToString(dr["OutletName"]),
                            BrandName = Convert.ToString(dr["BrandName"]),
                            SupportNumber = Convert.ToString(dr["SupportNumber"]),
                            SupportEmail = Convert.ToString(dr["SupportEmail"]),
                            AccountsContactNo = Convert.ToString(dr["AccountContact"]),
                            AccountEmail = Convert.ToString(dr["_AccountsEmailID"]),
                            FormatID = MessageFormat.LowBalanceFormat,
                            NotificationTitle = "Birthday Wish"
                        };
                        _List.Add(read);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _List;
        }

        public string GetName() => "proc_GetBirthdayWishAlert";
    }
}
