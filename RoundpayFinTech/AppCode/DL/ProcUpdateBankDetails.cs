using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateBankDetails : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateBankDetails(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            var _req = (GetEditUser)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@RequestID", _req.RequestID),
                new SqlParameter("@RequestStatus", _req.RequestStatus),
            };

            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.WID = Convert.ToInt32(dt.Rows[0]["WID"]);
                        res.LoginID = Convert.ToInt32(dt.Rows[0]["LoginID"]);
                        res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        res.UserMobileNo= Convert.ToString(dt.Rows[0]["UserMobileNo"]);
                        res.UserEmailID = Convert.ToString(dt.Rows[0]["UserEmailID"]);
                        res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountsContactNo"]);
                        res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        res.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        res.FormatID = MessageFormat.UserPartialApproval;
                        res.NotificationTitle = nameof(MessageFormat.UserPartialApproval);
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
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_UpdateBankRequest";
    }
}
