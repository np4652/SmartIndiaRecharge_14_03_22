using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateUserB2C :IProcedure
    { 
        private readonly IDAL _dal;
        public ProcUpdateUserB2C(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GetEditUser)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@Name",req.Name??""),
                new SqlParameter("@OutletName",req.OutletName?? ""),
                new SqlParameter("@ProfilePic",req.ProfilePic?? ""),
                new SqlParameter("@PAN",req.PAN?? ""),
                new SqlParameter("@AADHAR",req.AADHAR?? ""),
                new SqlParameter("@Address",req.Address?? ""),
                new SqlParameter("@PinCode",req.Pincode?? ""),
                new SqlParameter("@IP",req.IP??""),
                new SqlParameter("@Browser",req.Browser??""),
                new SqlParameter("@DOB",req.DOB??""),
                new SqlParameter("@AlternateMobile",req.AlternateMobile??""),
                new SqlParameter("@Latlong",req.Latlong??""),
                 new SqlParameter("@MobileNo",req.MobileNo?? ""),
                new SqlParameter("@EmailID",req.EmailID?? "")

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
        public string GetName() => "proc_UpdateUserB2C";
    }
}
