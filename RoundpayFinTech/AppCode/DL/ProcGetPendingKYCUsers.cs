using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPendingKYCUsers : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetPendingKYCUsers(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            UserRequest _req = (UserRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LTID),
                new SqlParameter("@LoginID", _req.LoginID)
            };
            var resp = new List<GetEditUser>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var userReport = new GetEditUser
                        {
                            OutletID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                            Role = Convert.ToString(row["_Role"]),
                            Name = Convert.ToString(row["_Name"]),
                            OutletName = Convert.ToString(row["_Company"]),
                            MobileNo = Convert.ToString(row["_MobileNo"]),
                            KYCStatus = row["_KYCStatus"] is DBNull ? 1 : Convert.ToInt16(row["_KYCStatus"]),
                            PAN = Convert.ToString(row["_PAN"]),
                            AADHAR = Convert.ToString(row["_AADHAR"]),
                            Address = Convert.ToString(row["_Address"]),
                            StateName = Convert.ToString(row["_State"]),
                            City = Convert.ToString(row["_City"]),
                            Pincode = Convert.ToString(row["_PinCode"]),
                            Prefix = Convert.ToString(row["_Prefix"]),
                            Qualification = Convert.ToString(row["_Qualification"]),
                            ShopType = Convert.ToString(row["_ShopType"]),
                            LocationType = Convert.ToString(row["_LocationType"]),
                            Landmark = Convert.ToString(row["_Landmark"]),
                            BankName = Convert.ToString(row["_BankName"]),
                            IFSC = Convert.ToString(row["_IFSC"]),
                            AccountNumber = Convert.ToString(row["_AccountNumber"]),
                            AccountName = Convert.ToString(row["_AccountHolder"]),
                            DOB = Convert.ToString(row["_DOB"]),
                            IsRegisteredWithGST = row["_IsRegisteredWithGST"] is DBNull ? false : Convert.ToBoolean(row["_IsRegisteredWithGST"]),
                            GSTIN = Convert.ToString(row["_GSTIN"]),
                            PartnerName = Convert.ToString(row["PartnerName"])
                        };
                        resp.Add(userReport);
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
                    LoginTypeID = _req.LTID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetPendingKYCUsers";
    }
}
