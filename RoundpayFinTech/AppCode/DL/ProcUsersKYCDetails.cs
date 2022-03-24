using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUsersKYCDetails: IProcedure
    {
        private readonly IDAL _dal;
        public ProcUsersKYCDetails(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (DocTypeMaster)obj;

            var list = new List<DocTypeMaster>();
            var GetEditUserlist = new List<GetEditUser>();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginId),
                new SqlParameter("@UserId", req.UserId)
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = ds.Tables[0];
                DataTable dtkycdetails = ds.Tables[1];
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        if (dtkycdetails.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtkycdetails.Rows)
                            {
                                var userReport = new DocTypeMaster
                                {
                                    OutletID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
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
                                    PartnerName = Convert.ToString(row["PartnerName"]),
                                    KycData = "KYCDETAILS"
                                };
                                list.Add(userReport);
                            }
                        }
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var item = new DocTypeMaster
                            {
                                StatusCode = Convert.ToInt32(dt.Rows[0][0]),
                                ID = Convert.ToInt32(dt.Rows[i]["_ID"] is DBNull ? 0 : dt.Rows[i]["_ID"]),
                                DocTypeID = Convert.ToInt16(dt.Rows[i]["DocTypeID"] is DBNull ? 0 : dt.Rows[i]["DocTypeID"]),
                                DocName = dt.Rows[i]["_DocName"] is DBNull ? "" : dt.Rows[i]["_DocName"].ToString(),
                                DocUrl = dt.Rows[i]["_DocURL"] is DBNull ? "" : dt.Rows[i]["_DocURL"].ToString(),
                                VerifyStatus = dt.Rows[i]["_IsVerified"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_IsVerified"].ToString()),
                                EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "Not Yet" : dt.Rows[i]["_EntryDate"].ToString(),
                                ModifyDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "Not Yet" : dt.Rows[i]["_ModifyDate"].ToString(),
                                Remark = dt.Rows[i]["_Remark"] is DBNull ? "" : dt.Rows[i]["_Remark"].ToString(),
                                UserId = Convert.ToInt32(dt.Rows[i]["_UserId"] is DBNull ? 0 : dt.Rows[i]["_UserId"]),
                                OutletID = Convert.ToInt32(dt.Rows[i]["OutletID"] is DBNull ? 0 : dt.Rows[i]["OutletID"]),
                                KYCStatus = Convert.ToInt16(dt.Rows[i]["KYCStatus"] is DBNull ? 0 : dt.Rows[i]["KYCStatus"]),
                            };
                            item.IsOptional = Convert.ToBoolean(dt.Rows[i]["_IsOptional"] is DBNull ? false : dt.Rows[i]["_IsOptional"]);
                            list.Add(item);
                        }

                    }
                    else
                    {
                        list.Add(new DocTypeMaster
                        {
                            StatusCode = ErrorCodes.Minus1,
                            Msg = dt.Rows[0]["Msg"].ToString()
                        });
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginId
                });
            }

            return list;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UsersKYCDetails";
    }
}
