using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateCallMeHistory : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateCallMeHistory(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new AlertReplacementModel()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@CallHistory",_req.CommonStr ?? ""),
                new SqlParameter("@CallMeID",_req.CommonInt),
                new SqlParameter("@StatusID",_req.CommonInt2),
            };

            var dt = _dal.GetByProcedure("proc_UpdateCallMeHistory", param);
            if (dt.Rows.Count > 0)
            {
                try
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    res.UserMobileNo = Convert.ToString(dt.Rows[0]["UserMobileNumber"]);
                    res.LoginMobileNo = Convert.ToString(dt.Rows[0]["LoginMobile"]);
                    res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                    res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                    res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                    res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                    res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                    res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                    res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                    res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                    res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountsContactNo"]);
                    res.UserEmailID = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                    res.UserFCMID = Convert.ToString(dt.Rows[0]["FCMID"]);                    
                    res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                    res.UserName = Convert.ToString(dt.Rows[0]["UName"]);
                    res.LoginID = _req.LoginID;
                    res.FormatID = MessageFormat.CallNotPicked;
                    res.NotificationTitle = nameof(MessageFormat.CallNotPicked);
                }
                catch (Exception ex)
                {
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "Call",
                        Error = ex.Message,
                        LoginTypeID = _req.LoginTypeID,
                        UserId = _req.LoginID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateCallMeHistory";
    }
}
