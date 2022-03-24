using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcEditUserDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEditUserDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new GetEditUser
            {
                ResultCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID",req.CommonInt)
            };
            try
            {
                var Roles = new List<RoleMaster>();
                var Slabs = new List<SlabMaster>();
                var Card = new List<Cards>();
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var roleMaster = new RoleMaster
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            Role = dr["_Role"].ToString(),
                            Ind = Convert.ToInt32(dr["_Ind"])
                        };
                        Roles.Add(roleMaster);
                    }
                }
                res.Roles = Roles;
                if (ds.Tables.Count > 1)
                {
                    res.ResultCode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    res.EmailID = ds.Tables[1].Rows[0]["_EmailID"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_EmailID"].ToString();
                    res.MobileNo = ds.Tables[1].Rows[0]["_MobileNo"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_MobileNo"].ToString();
                    res.Name = ds.Tables[1].Rows[0]["_Name"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_Name"].ToString();
                    res.OutletName = ds.Tables[1].Rows[0]["_OutletName"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_OutletName"].ToString();
                    res.RoleID = Convert.ToInt32(ds.Tables[1].Rows[0]["_RoleID"]);
                    res.AADHAR = ds.Tables[1].Rows[0]["_AADHAR"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_AADHAR"].ToString();
                    res.PAN = ds.Tables[1].Rows[0]["_PAN"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_PAN"].ToString();
                    res.Address = ds.Tables[1].Rows[0]["_Address"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_Address"].ToString();
                    res.GSTIN = ds.Tables[1].Rows[0]["_GSTIN"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_GSTIN"].ToString();
                    res.UserID = ds.Tables[1].Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(ds.Tables[1].Rows[0]["_ID"]);
                    res.ReferalID = ds.Tables[1].Rows[0]["_ReferalID"] is DBNull?0: Convert.ToInt32(ds.Tables[1].Rows[0]["_ReferalID"]);
                    res.Pincode = ds.Tables[1].Rows[0]["_PinCode"] is DBNull? string.Empty: ds.Tables[1].Rows[0]["_PinCode"].ToString();
                    res.City = ds.Tables[1].Rows[0]["_City"] is DBNull?string.Empty: ds.Tables[1].Rows[0]["_City"].ToString();
                    res.IsGSTApplicable = ds.Tables[1].Rows[0]["_IsGSTApplicable"] is DBNull?false: Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsGSTApplicable"]);
                    res.IsTDSApplicable = ds.Tables[1].Rows[0]["_IsTDSApplicable"] is DBNull?false: Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsTDSApplicable"]);
                    res.DMRModelID = ds.Tables[1].Rows[0]["_DMRModelID"] is DBNull ? 0 : Convert.ToInt16(ds.Tables[1].Rows[0]["_DMRModelID"]);
                    res.StateID = ds.Tables[1].Rows[0]["_StateID"] is DBNull?0: Convert.ToInt32(ds.Tables[1].Rows[0]["_StateID"]);
                    res.StateName = ds.Tables[1].Rows[0]["StateName"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["StateName"].ToString();
                    res.CommRate = Convert.ToDecimal(ds.Tables[1].Rows[0]["_CommRate"]);
                    res.LoginID = req.LoginID;
                    res.Role = ds.Tables[1].Rows[0]["_Role"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_Role"].ToString();
                    res.IsDoubleFactor = ds.Tables[1].Rows[0]["_IsDoubleFactor"] is DBNull ? false : Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsDoubleFactor"]);
                    res.IsRealAPI = ds.Tables[1].Rows[0]["_IsRealApi"] is DBNull ? false : Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsRealApi"]);
                    res.Qualification = ds.Tables[1].Rows[0]["_Qualification"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_Qualification"].ToString();
                    res.Poupulation = ds.Tables[1].Rows[0]["_Poupulation"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_Poupulation"].ToString();
                    res.LocationType = ds.Tables[1].Rows[0]["_LocationType"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_LocationType"].ToString();
                    res.Landmark = ds.Tables[1].Rows[0]["_Landmark"] is DBNull ? " " : ds.Tables[1].Rows[0]["_Landmark"].ToString();
                    res.AlternateMobile = ds.Tables[1].Rows[0]["_AlternateMobile"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_AlternateMobile"].ToString();
                    res.Latlong = ds.Tables[1].Rows[0]["_Latlong"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_Latlong"].ToString();
                    res.ShopType = ds.Tables[1].Rows[0]["_ShopType"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_ShopType"].ToString();
                    res.KYCStatus = ds.Tables[1].Rows[0]["_KYCStatus"] is DBNull ? 0 : Convert.ToInt32(ds.Tables[1].Rows[0]["_KYCStatus"]);
                    res.OutletID = ds.Tables[1].Rows[0]["_OutletID"] is DBNull ? 0 : Convert.ToInt32(ds.Tables[1].Rows[0]["_OutletID"]);
                    res.AccountName = ds.Tables[1].Rows[0]["_AccountHolder"] is DBNull ? string.Empty : (ds.Tables[1].Rows[0]["_AccountHolder"].ToString());
                    res.AccountNumber = ds.Tables[1].Rows[0]["_AccountNumber"] is DBNull ? string.Empty : (ds.Tables[1].Rows[0]["_AccountNumber"].ToString());
                    res.WhatsAppNumber = ds.Tables[1].Rows[0]["_WhatsappNo"] is DBNull ? string.Empty : (ds.Tables[1].Rows[0]["_WhatsappNo"].ToString());
                    res.BranchName = ds.Tables[1].Rows[0]["_BranchName"] is DBNull ? string.Empty : (ds.Tables[1].Rows[0]["_BranchName"].ToString());
                    res.BankName = ds.Tables[1].Rows[0]["_BankName"] is DBNull ? string.Empty : (ds.Tables[1].Rows[0]["_BankName"].ToString());
                    res.IFSC = ds.Tables[1].Rows[0]["_IFSC"] is DBNull ? string.Empty : (ds.Tables[1].Rows[0]["_IFSC"].ToString());
                    res.DOB = ds.Tables[1].Rows[0]["DOB"] is DBNull ? string.Empty : (ds.Tables[1].Rows[0]["DOB"].ToString());
                    res.IsWebsite = ds.Tables[1].Rows[0]["_IsWebsite"] is DBNull ? false : Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsWebsite"]);
                    res.WebsiteName = ds.Tables[1].Rows[0]["_WebsiteName"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_WebsiteName"].ToString();
                    res.IsBankUpdateAvailable = ds.Tables[1].Rows[0]["_IsBankUpdateAvailable"] is DBNull ? false : Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsBankUpdateAvailable"]);
                    res.IsRegisteredWithGST = ds.Tables[1].Rows[0]["_IsRegisteredWithGST"] is DBNull ? false : Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsRegisteredWithGST"]);
                    res.CustomLoginID = ds.Tables[1].Rows[0]["_CustomLoginID"] is DBNull ? string.Empty : ds.Tables[1].Rows[0]["_CustomLoginID"].ToString();
                    res.IsSwitchIMPStoNEFT = ds.Tables[1].Rows[0]["_IsnNEFTRouting"] is DBNull ? false : Convert.ToBoolean(ds.Tables[1].Rows[0]["_IsnNEFTRouting"]);
                    res.EKYCID = ds.Tables[1].Rows[0]["_EKYCID"] is DBNull ? 0 : Convert.ToInt32(ds.Tables[1].Rows[0]["_EKYCID"]);
                }
                StringBuilder profileImage = new StringBuilder();
                profileImage.Append(DOCType.ProfileImagePath);
                profileImage.Append(req.LoginID.ToString());
                profileImage.Append(".png");
                if (File.Exists(profileImage.ToString()))
                {
                    res.ProfilePic = profileImage.Clear().AppendFormat("/Image/Profile/{0}.png", req.LoginID.ToString()).ToString();
                }
                if (ds.Tables.Count > 2)
                {
                    foreach (DataRow dr in ds.Tables[2].Rows)
                    {
                        var slabMaster = new SlabMaster
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            Slab = dr["_Slab"].ToString()
                        };
                        Slabs.Add(slabMaster);
                    }
                }


                if (ds.Tables.Count > 3)
                {
                    foreach (DataRow dr in ds.Tables[3].Rows)
                    {
                        var cardDetail = new Cards
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            CardNumber = dr["_AccountNo"].ToString(),
                            ValidFrom = Convert.ToDateTime(dr["_Validfrom"]).ToString("MM/yy"),
                            ValidThru = Convert.ToDateTime(dr["_ValidThru"]).ToString("MM/yy")
                        };
                        Card.Add(cardDetail);
                    }
                }
                res.Cards = Card;

            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
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
        public string GetName() => "proc_EditUserDetail";

        public List<DMRModel> GetDMRModels()
        {
            string Query = "select _ID,_Name From MASTER_DMR_MODEL where _IsActive=1";
            var res = new List<DMRModel>();
            try
            {
                DataTable dt = _dal.Get(Query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new DMRModel
                        {
                            ID = item["_ID"] is DBNull ? 0 : Convert.ToInt16(item["_ID"]),
                            Name = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString()
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return res;
        }
    }
}
