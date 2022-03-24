using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GetEditUser)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@Name",req.Name??""),
                new SqlParameter("@OutletName",req.OutletName?? ""),
                new SqlParameter("@ProfilePic",req.ProfilePic?? ""),
                new SqlParameter("@MobileNo",req.MobileNo?? ""),
                new SqlParameter("@EmailID",req.EmailID?? ""),
                new SqlParameter("@GSTIN",req.GSTIN?? ""),
                new SqlParameter("@PAN",req.PAN?? ""),
                new SqlParameter("@AADHAR",req.AADHAR?? ""),
                new SqlParameter("@Address",req.Address?? ""),
                new SqlParameter("@PinCode",req.Pincode?? ""),
                new SqlParameter("@IsGSTApplicable",req.IsGSTApplicable),
                new SqlParameter("@IsTDSApplicable",req.IsTDSApplicable),
                new SqlParameter("@CommRate",req.CommRate),
                new SqlParameter("@DMRModelID",req.DMRModelID),
                new SqlParameter("@IP",req.IP??""),
                new SqlParameter("@Browser",req.Browser??""),
                new SqlParameter("@DOB",req.DOB??""),
                new SqlParameter("@ShopType",req.ShopType??""),
                new SqlParameter("@Qualification",req.Qualification??""),
                new SqlParameter("@Poupulation",req.Poupulation??""),
                new SqlParameter("@LocationType",req.LocationType??""),
                new SqlParameter("@Landmark",req.Landmark??""),
                new SqlParameter("@AlternateMobile",req.AlternateMobile??""),
                new SqlParameter("@Latlong",req.Latlong??""),
                new SqlParameter("@BankName",req.BankName??""),
                new SqlParameter("@BranchName",req.BranchName??""),
                new SqlParameter("@IFSC",req.IFSC??""),
                new SqlParameter("@AccountNumber",req.AccountNumber??""),
                new SqlParameter("@AccountHolder",req.AccountName??""),
                new SqlParameter("@WebsiteName", req.WebsiteName??""),
                new SqlParameter("@IsWebsite", req.IsWebsite),
                new SqlParameter("@IsRegisteredWithGST", req.IsRegisteredWithGST),
                new SqlParameter("@WhatsAppNumber", req.WhatsAppNumber??string.Empty),
                new SqlParameter("@CustomLoginID", req.CustomLoginID??string.Empty),
                new SqlParameter("@CompanyTypeID",req.CompanyTypeID),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One) {
                        res.CommonInt = Convert.ToInt32(dt.Rows[0]["WID"] is DBNull ? 0 : dt.Rows[0]["WID"]);
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateUser";
    }
}
