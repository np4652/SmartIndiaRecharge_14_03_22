using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateBookingStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateBookingStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (_CallbackData)obj;
            _req.Statuscode = ErrorCodes.Minus1;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@BookingStatus", _req.BookingStatus),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@Remark", _req.Msg == null ? string.Empty : (_req.Msg.Length > 200 ? _req.Msg.Substring(0, 200) : _req.Msg)),
                new SqlParameter("@IPAddress", _req.RequestIP ?? string.Empty),
                new SqlParameter("@Browser", _req.Browser ?? string.Empty),
                 new SqlParameter("@VendorID", (_req.VendorID ?? string.Empty).Length > 50 ? (_req.VendorID ?? "").Substring(0, 50) : (_req.VendorID ?? "")),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@RequestPage", _req.RequestPage == null ? string.Empty : _req.RequestPage.Length > 50 ? _req.RequestPage.Substring(0, 50) : _req.RequestPage),
                new SqlParameter("@InstallTime", _req.InstallationTime??""),
                new SqlParameter("@InstallCharges", _req.InstalltionCharges??""),
                new SqlParameter("@TechnicianName", _req.TechnicianName??""),
                new SqlParameter("@TechnicianMobile", _req.TechnicianMobile??""),
                new SqlParameter("@CustomerID", _req.CustomerID??""),
                new SqlParameter("@STBID", _req.STBID??""),
                new SqlParameter("@VCNo", _req.VCNO??""),
                new SqlParameter("@Approvaltime", _req.ApprovalTime)

            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _req.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _req.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_req.Statuscode == ErrorCodes.One)
                    {
                        _req.UserID = Convert.ToInt32(dt.Rows[0]["UserID"] is DBNull ? 0 : dt.Rows[0]["UserID"]);
                        _req.UpdateUrl = dt.Rows[0]["UpdateURL"] is DBNull ? string.Empty : dt.Rows[0]["UpdateURL"].ToString();
                        _req.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        _req.RequestMode = Convert.ToInt32(dt.Rows[0]["RequestMode"] is DBNull ? 0 : dt.Rows[0]["RequestMode"]);
                        _req.IsCallbackFound = dt.Rows[0]["IsCallbackFound"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsCallbackFound"]);
                        //New lines
                        _req.AccountNo = dt.Rows[0]["AccountNo"] is DBNull ? string.Empty : dt.Rows[0]["AccountNo"].ToString();
                        _req.Balance = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        _req.RequestedAmount = dt.Rows[0]["RequestedAmount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["RequestedAmount"]);
                        _req.FCMID = dt.Rows[0]["FCMID"] is DBNull ? string.Empty : dt.Rows[0]["FCMID"].ToString();
                        _req.Operator = dt.Rows[0]["Operator"] is DBNull ? string.Empty : dt.Rows[0]["Operator"].ToString();
                        _req.MobileNo = dt.Rows[0]["MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["MobileNo"].ToString();
                        _req.EmailID = dt.Rows[0]["EmailID"] is DBNull ? string.Empty : dt.Rows[0]["EmailID"].ToString();
                        _req.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _req.Company = dt.Rows[0]["Company"] is DBNull ? string.Empty : dt.Rows[0]["Company"].ToString();
                        _req.CompanyDomain = dt.Rows[0]["CompanyDomain"] is DBNull ? string.Empty : dt.Rows[0]["CompanyDomain"].ToString();
                        _req.SupportNumber = dt.Rows[0]["SupportNumber"] is DBNull ? string.Empty : dt.Rows[0]["SupportNumber"].ToString();
                        _req.SupportEmail = dt.Rows[0]["SupportEmail"] is DBNull ? string.Empty : dt.Rows[0]["SupportEmail"].ToString();
                        _req.AccountContact = dt.Rows[0]["AccountContact"] is DBNull ? string.Empty : dt.Rows[0]["AccountContact"].ToString();
                        _req.AccountEmail = dt.Rows[0]["AccountEmail"] is DBNull ? string.Empty : dt.Rows[0]["AccountEmail"].ToString();
                        //New LInes End

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
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _req;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UpdateBookingStatus";
    }
}
