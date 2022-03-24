using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRegisterOutlet : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRegisterOutlet(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var request = (OutletProcModal)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",request.UserID),
                new SqlParameter("@Name",request.OutletName??string.Empty),
                new SqlParameter("@Company",request.data.Company??string.Empty),
                new SqlParameter("@MobileNo",request.data.MobileNo??string.Empty),
                new SqlParameter("@EmailID",request.data.EmailID??string.Empty),
                new SqlParameter("@Pincode",request.data.Pincode??string.Empty),
                new SqlParameter("@Address",request.data.Address??string.Empty),
                new SqlParameter("@PAN",request.data.PAN??string.Empty),
                new SqlParameter("@AADHAR",request.data.AADHAR??string.Empty),
                new SqlParameter("@OType",string.Empty),
                new SqlParameter("@IsOutsider",request.IsOutsider),
                new SqlParameter("@StateID",request.data.StateID),
                new SqlParameter("@State",string.Empty),
                new SqlParameter("@CityID",request.data.CityID),
                new SqlParameter("@City",string.Empty),
                new SqlParameter("@CRoleID",request.RoleID),
                new SqlParameter("@DOB",request.data.DOB??string.Empty),
                new SqlParameter("@ShopType",request.data.ShopType??string.Empty),
                new SqlParameter("@Qualification",request.data.Qualification??string.Empty),
                new SqlParameter("@Poupulation",request.data.Poupulation??string.Empty),
                new SqlParameter("@LocationType",request.data.LocationType??string.Empty),
                new SqlParameter("@Landmark",request.data.Landmark??string.Empty),
                new SqlParameter("@AlternateMobile",request.data.AlternateMobile??string.Empty),
                new SqlParameter("@Latlong",(request.data.Lattitude??string.Empty)+","+(request.data.Longitude??string.Empty)),
                new SqlParameter("@BankName",request.data.BankName??string.Empty),
                new SqlParameter("@IFSC",request.data.IFSC??string.Empty),
                new SqlParameter("@AccountNumber",request.data.AccountNumber??string.Empty),
                new SqlParameter("@AccountHolder",request.data.AccountHolder??string.Empty),
                new SqlParameter("@KYCStatus",request.KYCStatus),
                new SqlParameter("@OutletID",request.data.OutletID),
                new SqlParameter("@BranchName",request.data.BranchName??string.Empty)
            };
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    response.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    response.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    response.CommonInt = dt.Rows[0]["OutletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["OutletID"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return response;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_RegisterOutlet";
    }
}
