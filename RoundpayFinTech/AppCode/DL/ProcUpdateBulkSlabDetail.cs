using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateBulkSlabDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateBulkSlabDetail(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            SlabCommissionReq req = (SlabCommissionReq)obj;
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param =
            {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.SlabID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@OPId",req.OPID),
                new SqlParameter("@IP",req.CommonStr??""),
                new SqlParameter("@Browser",req.CommonStr2??""),
                new SqlParameter("@SlabRoleID",req.RoleID),
                new SqlParameter("@Action",req.Action),
                new SqlParameter("@Mode",req.Mode),
                new SqlParameter("@IsGenralOrReal",req.IsGenralOrReal),
                new SqlParameter("@Amount",req.Amount),
                new SqlParameter("@AmountType",req.AmountType),
                new SqlParameter("@CommissionType",req.CommissionType)
            };

            try
            {
                
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.LoginID = req.LoginID;
                        res.LoginMobileNo = Convert.ToString(dt.Rows[0]["LoginMobileNo"]);
                        res.LoginEmailID = Convert.ToString(dt.Rows[0]["LoginEmailID"]);
                        res.LoginFCMID = Convert.ToString(dt.Rows[0]["LoginFCMID"]);
                        res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        res.Company = Convert.ToString(dt.Rows[0]["CompanyName"]);
                        res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountNumber"]);
                        res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        res.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        res.UserMobileNo = Convert.ToString(dt.Rows[0]["MobileNos"]);
                        res.UserEmailID = Convert.ToString(dt.Rows[0]["EmailIDs"]);
                        res.UserFCMID = Convert.ToString(dt.Rows[0]["FCMIDs"]);
                        res.UserIds = Convert.ToString(dt.Rows[0]["UserIds"]);
                        res.Operator = Convert.ToString(dt.Rows[0]["Operator"]);
                        res.FormatID = MessageFormat.MarginRevised;
                        res.NotificationTitle = "Margin Revised";
                        res.bccList = res.UserEmailID.Split(',').ToList();

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_Update_Bulk_SlabDetail";
    }
}
