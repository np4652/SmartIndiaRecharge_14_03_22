using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcEKYCByAadhar : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEKYCByAadhar(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (EKYCByAadharProcReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@Profile",req.Profile??string.Empty),
                new SqlParameter("@FullName",req.FullName??string.Empty),
                new SqlParameter("@AadhaarNo",req.AadhaarNo??string.Empty),
                new SqlParameter("@DOB",req.DOB??string.Empty),
                new SqlParameter("@Gender",req.Gender??string.Empty),
                new SqlParameter("@Country",req.Country??string.Empty),
                new SqlParameter("@District",req.District??string.Empty),
                new SqlParameter("@State",req.State??string.Empty),
                new SqlParameter("@PostOffice",req.PostOffice??string.Empty),
                new SqlParameter("@Location",req.Location??string.Empty),
                new SqlParameter("@VTC",req.VTC??string.Empty),
                new SqlParameter("@SubDistrict",req.SubDistrict??string.Empty),
                new SqlParameter("@Street",req.Street??string.Empty),
                new SqlParameter("@House",req.House??string.Empty),
                new SqlParameter("@Landmark",req.Landmark??string.Empty),
                new SqlParameter("@Pincode",req.Pincode??string.Empty),
                new SqlParameter("@HasImage",req.HasImage),
                new SqlParameter("@ParentName",req.ParentName??string.Empty),
                new SqlParameter("@IsMobileVerified",req.IsMobileVerified),
                new SqlParameter("@ShareCode",req.ShareCode??string.Empty),
                new SqlParameter("@DirectorName",req.DirectorName??string.Empty),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@IsExternal",req.IsExternal),
                new SqlParameter("@ChildUserID",req.ChildUserID),
                new SqlParameter("@APIStatus",req.APIStatus)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonInt = dt.Rows[0]["_EKYCID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EKYCID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_EKYCByAadhar";
    }
}
