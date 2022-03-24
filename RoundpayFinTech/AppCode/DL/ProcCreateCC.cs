using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCreateCC : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCreateCC(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (Customercare)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@DepartmentID",req.DeptID),
                new SqlParameter("@DepartmentRoleID",req.RoleID),
                new SqlParameter("@Name",req.Name??""),
                new SqlParameter("@MobileNo",req.MobileNo??""),
                new SqlParameter("@EmailID",req.EmailID??""),
                new SqlParameter("@PinCode",req.Pincode??""),
                new SqlParameter("@Password",HashEncryption.O.Encrypt(req.Password??"")),
                new SqlParameter("@Address",req.Address??"")
            };
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonStr = dt.Rows[0]["UserID"] is DBNull ? "" : dt.Rows[0]["UserID"].ToString();
                        res.WID = Convert.ToInt32(dt.Rows[0]["WID"] is DBNull ? 0 : dt.Rows[0]["WID"]);
                        res.LoginID = dt.Rows[0]["LoginID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["LoginID"]);
                        res.Password = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["Password"]));
                        res.PinPassword = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["PIN"]));
                        res.UserEmailID = Convert.ToString(dt.Rows[0]["UserEmailID"]);
                        res.UserMobileNo = Convert.ToString(dt.Rows[0]["UserMobileNo"]);
                        res.UserPrefix = Convert.ToString(dt.Rows[0]["Prefix"]);
                        res.UserID = Convert.ToInt32(dt.Rows[0]["NewUserId"]);
                        res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountNumber"]);
                        res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        res.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
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
                    LoginTypeID = req.ID,
                    UserId = req.RoleID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "Proc_CreateCC";
    }
}
