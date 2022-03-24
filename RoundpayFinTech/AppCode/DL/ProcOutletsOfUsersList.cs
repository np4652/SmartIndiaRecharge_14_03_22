using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcOutletsOfUsersList : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcOutletsOfUsersList(IDAL dal) => _dal = dal;
        public string GetName() => "proc_OutletsOfUsersList";
        public async Task<object> Call(object obj)
        {
            var _req = (_OuletOfUsersListFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Name", _req.Name ?? ""),
                new SqlParameter("@UserID", _req.UserID == 0 ? 0:_req.UserID),
                new SqlParameter("@UserMobile", _req.Mobile_F ?? ""),
                new SqlParameter("@OutletMobile", _req.OutletMobile ?? ""),
                new SqlParameter("@OutletID", _req.OutletID == 0 ? 0:_req.OutletID),
                new SqlParameter("@TopRows", _req.TopRows == 0 ? 50 : _req.TopRows),
                new SqlParameter("@PAN", _req.PAN ?? ""),
                new SqlParameter("@Adhar", _req.Adhar ?? ""),
                new SqlParameter("@KycStatus", _req.KycStatus == 0 ? 0:_req.KycStatus),
                new SqlParameter("@VerifyStatus", _req.VerifyStatus == 0 ? 0:_req.VerifyStatus),
                new SqlParameter("@ApiId", _req.ApiId),
                new SqlParameter("@ApiStatusId", _req.ApiStatus),
                new SqlParameter("@ApiServiceId", _req.ServiceId),
                new SqlParameter("@ApiServiceStatus", _req.ServiceStatusId),
                new SqlParameter("@DeviceId", _req.DeviceId ?? "")
            };
            var _alist = new List<OutletsOfUsersList>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0][0].ToString() == "1")
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var item = new OutletsOfUsersList
                            {
                                ResultCode = ErrorCodes.One,
                                Msg = "Success",
                                _ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                _Prefix = dt.Rows[i]["_Prefix"].ToString(),
                                _UserID = Convert.ToInt32(dt.Rows[i]["_UserID"]),
                                DisplayUserID = dt.Rows[i]["DisplayUserID"].ToString(),
                                UserName = dt.Rows[i]["UserName"].ToString(),
                                UserMobile = dt.Rows[i]["UserMobile"] is DBNull ? "" : dt.Rows[i]["UserMobile"].ToString(),
                                _Name = Convert.ToString(dt.Rows[i]["_Name"]),
                                _Company = Convert.ToString(dt.Rows[i]["_Company"]),
                                _MobileNo = Convert.ToString(dt.Rows[i]["_MobileNo"]),
                                _EmailID = dt.Rows[i]["_EmailID"].ToString(),
                                _Pincode = dt.Rows[i]["_Pincode"] is DBNull ? "" : dt.Rows[i]["_Pincode"].ToString(),
                                _Address = dt.Rows[i]["_Address"].ToString(),
                                _PAN = Convert.ToString(dt.Rows[i]["_PAN"]),
                                _AADHAR = Convert.ToString(dt.Rows[i]["_AADHAR"]),
                                _VerifyStatus = Convert.ToInt32(dt.Rows[i]["_VerifyStatus"]),
                                VerifyStatus = dt.Rows[i]["VerifyStatus"].ToString(),
                                _IsActive = Convert.ToBoolean(dt.Rows[i]["_IsActive"]),
                                _OType = Convert.ToString(dt.Rows[i]["_OType"]),
                                _EntryBy = Convert.ToInt32(dt.Rows[i]["_EntryBy"]),
                                _EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "" : dt.Rows[i]["_EntryDate"].ToString(),
                                _ModifyDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "" : dt.Rows[i]["_ModifyDate"].ToString(),
                                _RoleID = Convert.ToInt32(dt.Rows[i]["_RoleID"]),
                                Role = dt.Rows[i]["Role"].ToString(),
                                _IsOutsider = Convert.ToBoolean(dt.Rows[i]["_IsOutsider"]),
                                _KYCStatus = Convert.ToInt32(dt.Rows[i]["_KYCStatus"]),
                                KYCStatus = dt.Rows[i]["KYCStatus"].ToString(),
                                _State = Convert.ToString(dt.Rows[i]["_State"]),
                                _City = dt.Rows[i]["_City"].ToString(),
                                _DOB = dt.Rows[i]["_DOB"] is DBNull ? "" : dt.Rows[i]["_DOB"].ToString(),
                                _shopType = dt.Rows[i]["_shopType"].ToString(),
                                _Qualification = Convert.ToString(dt.Rows[i]["_Qualification"]),
                                _Poupulation = Convert.ToString(dt.Rows[i]["_Poupulation"]),
                                _LocationType = Convert.ToString(dt.Rows[i]["_LocationType"]),
                                _Landmark = Convert.ToString(dt.Rows[i]["_Landmark"]),
                                _AlternateMobile = dt.Rows[i]["_AlternateMobile"].ToString(),
                                _latlong = Convert.ToString(dt.Rows[i]["_latlong"]),
                                _BankName = Convert.ToString(dt.Rows[i]["_BankName"]),
                                _IFSC = Convert.ToString(dt.Rows[i]["_IFSC"]),
                                _AccountNumber = Convert.ToString(dt.Rows[i]["_AccountNumber"]),
                                _AccountHolder = Convert.ToString(dt.Rows[i]["_AccountHolder"]),
                                BBPSStatus = dt.Rows[i]["BBPSStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["BBPSStatus"]),
                                AEPSStatus = dt.Rows[i]["AEPSStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["AEPSStatus"]),
                                PSAStatus = dt.Rows[i]["PSAStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["PSAStatus"]),
                                DMTStatus = dt.Rows[i]["DMTStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["DMTStatus"]),
                                _BBPSStatus = dt.Rows[i]["_BBPSStatus"].ToString(),
                                _AEPSStatus = dt.Rows[i]["_AEPSStatus"].ToString(),
                                _PSAStatus = dt.Rows[i]["_PSAStatus"].ToString(),
                                _DMTStatus = dt.Rows[i]["_DMTStatus"].ToString(),
                                _MATMStatus = dt.Rows[i]["matmStatus"] is DBNull ? "" : dt.Rows[i]["matmStatus"].ToString(),
                                MATMStatus = dt.Rows[i]["_MATMStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_MATMStatus"]),
                                DeviceId = dt.Rows[i]["DeviceId"] is DBNull ? "" : Convert.ToString(dt.Rows[i]["DeviceId"]),
                                IRCTCStaus = dt.Rows[i]["IRCTCStaus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["IRCTCStaus"]),
                                IRCTCID = dt.Rows[i]["_IRCTCID"] is DBNull ? "" : dt.Rows[i]["_IRCTCID"].ToString(),
                                IRCTCExpiry = dt.Rows[i]["_IRCTCExpiry"] is DBNull ? "" : dt.Rows[i]["_IRCTCExpiry"].ToString()
                            };
                            _alist.Add(item);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var item = new OutletsOfUsersList
                            {
                                ResultCode = ErrorCodes.One,
                                Msg = "Success",
                                _ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                _Prefix = dt.Rows[i]["_Prefix"].ToString(),
                                _UserID = Convert.ToInt32(dt.Rows[i]["_UserID"]),
                                DisplayUserID = dt.Rows[i]["DisplayUserID"].ToString(),
                                UserName = dt.Rows[i]["UserName"].ToString(),
                                UserMobile = dt.Rows[i]["UserMobile"] is DBNull ? "" : dt.Rows[i]["UserMobile"].ToString(),
                                _Name = Convert.ToString(dt.Rows[i]["_Name"]),
                                _Company = Convert.ToString(dt.Rows[i]["_Company"]),
                                _MobileNo = Convert.ToString(dt.Rows[i]["_MobileNo"]),
                                _EmailID = dt.Rows[i]["_EmailID"].ToString(),
                                _Pincode = dt.Rows[i]["_Pincode"] is DBNull ? "" : dt.Rows[i]["_Pincode"].ToString(),
                                _Address = dt.Rows[i]["_Address"].ToString(),
                                _PAN = Convert.ToString(dt.Rows[i]["_PAN"]),
                                _AADHAR = Convert.ToString(dt.Rows[i]["_AADHAR"]),
                                _VerifyStatus = Convert.ToInt32(dt.Rows[i]["_VerifyStatus"]),
                                VerifyStatus = dt.Rows[i]["VerifyStatus"].ToString(),
                                _IsActive = Convert.ToBoolean(dt.Rows[i]["_IsActive"]),
                                _OType = Convert.ToString(dt.Rows[i]["_OType"]),
                                _EntryBy = Convert.ToInt32(dt.Rows[i]["_EntryBy"]),
                                _EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "" : dt.Rows[i]["_EntryDate"].ToString(),
                                _ModifyDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "" : dt.Rows[i]["_ModifyDate"].ToString(),
                                _RoleID = Convert.ToInt32(dt.Rows[i]["_RoleID"]),
                                Role = dt.Rows[i]["Role"].ToString(),
                                _IsOutsider = Convert.ToBoolean(dt.Rows[i]["_IsOutsider"]),
                                _KYCStatus = Convert.ToInt32(dt.Rows[i]["_KYCStatus"]),
                                KYCStatus = dt.Rows[i]["KYCStatus"].ToString(),
                                _State = Convert.ToString(dt.Rows[i]["_State"]),
                                _City = dt.Rows[i]["_City"].ToString(),
                                _DOB = dt.Rows[i]["_DOB"] is DBNull ? "" : dt.Rows[i]["_DOB"].ToString(),
                                _shopType = dt.Rows[i]["_shopType"].ToString(),
                                _Qualification = Convert.ToString(dt.Rows[i]["_Qualification"]),
                                _Poupulation = Convert.ToString(dt.Rows[i]["_Poupulation"]),
                                _LocationType = Convert.ToString(dt.Rows[i]["_LocationType"]),
                                _Landmark = Convert.ToString(dt.Rows[i]["_Landmark"]),
                                _AlternateMobile = dt.Rows[i]["_AlternateMobile"].ToString(),
                                _latlong = Convert.ToString(dt.Rows[i]["_latlong"]),
                                _BankName = Convert.ToString(dt.Rows[i]["_BankName"]),
                                _IFSC = Convert.ToString(dt.Rows[i]["_IFSC"]),
                                _AccountNumber = Convert.ToString(dt.Rows[i]["_AccountNumber"]),
                                _AccountHolder = Convert.ToString(dt.Rows[i]["_AccountHolder"]),
                                BBPSStatus = dt.Rows[i]["BBPSStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["BBPSStatus"]),
                                AEPSStatus = dt.Rows[i]["AEPSStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["AEPSStatus"]),
                                PSAStatus = dt.Rows[i]["PSAStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["PSAStatus"]),
                                DMTStatus = dt.Rows[i]["DMTStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["DMTStatus"]),
                                _BBPSStatus = dt.Rows[i]["_BBPSStatus"].ToString(),
                                _AEPSStatus = dt.Rows[i]["_AEPSStatus"].ToString(),
                                _PSAStatus = dt.Rows[i]["_PSAStatus"].ToString(),
                                _DMTStatus = dt.Rows[i]["_DMTStatus"].ToString(),
                                ApiId = Convert.ToInt32(dt.Rows[i]["_APIID"]),
                                ApiName = dt.Rows[i]["ApiName"].ToString(),
                                ApiOutletId = dt.Rows[i]["_APIOutletID"].ToString(),
                                _MATMStatus = dt.Rows[i]["matmStatus"] is DBNull ? "" : dt.Rows[i]["matmStatus"].ToString(),
                                MATMStatus = dt.Rows[i]["_MATMStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_MATMStatus"]),
                                DeviceId = dt.Rows[i]["DeviceId"] is DBNull ? "" : Convert.ToString(dt.Rows[i]["DeviceId"]),
                                IRCTCStaus = dt.Rows[i]["IRCTCStaus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["IRCTCStaus"]),
                                IRCTCID = dt.Rows[i]["_IRCTCID"] is DBNull ? "" : dt.Rows[i]["_IRCTCID"].ToString(),
                                IRCTCExpiry = dt.Rows[i]["_IRCTCExpiry"] is DBNull ? "" : dt.Rows[i]["_IRCTCExpiry"].ToString()
                            };
                            _alist.Add(item);
                        }
                    }
                }
            }
            catch (Exception er)
            {

            }
            return _alist;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }

    public class ProcGetOutletsOfAPIUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetOutletsOfAPIUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIUserID",req.LoginID),
                new SqlParameter("@OutletMobile",req.CommonStr??string.Empty)
            };
            var res = new OutletsOfUsersList();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res._ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res._KYCStatus = dt.Rows[0]["_KYCStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_KYCStatus"]);
                    res._VerifyStatus = dt.Rows[0]["_VerifyStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_VerifyStatus"]);
                    res.BBPSStatus = dt.Rows[0]["BBPSStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["BBPSStatus"]);
                    res.AEPSStatus = dt.Rows[0]["AEPSStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["AEPSStatus"]);
                    res.PSAStatus = dt.Rows[0]["PSAStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["PSAStatus"]);
                    res.DMTStatus = dt.Rows[0]["DMTStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["DMTStatus"]);
                }
            }
            catch (Exception)
            {
            }
            return res;
        }


        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetOutletsOfAPIUser";
    }
    public class ProcRecentOutletsOfUsersList : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRecentOutletsOfUsersList(IDAL dal) => _dal = dal;
        public string GetName() => "proc_RecentBCAgentList";
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TopRow", _req.CommonInt)
            };
            var _alist = new List<OutletsOfUsersList>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new OutletsOfUsersList
                        {
                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            _Name = Convert.ToString(dt.Rows[i]["_Name"]),
                            _Company = Convert.ToString(dt.Rows[i]["_Company"]),
                            _EntryDate = Convert.ToString(dt.Rows[i]["_EntryDate"])
                        };
                        _alist.Add(item);
                    }
                }
            }
            catch (Exception er)
            {

            }

            return _alist;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }
    public class ProcOutletsOfUsersListByUserID : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcOutletsOfUsersListByUserID(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetOutletsOfUserListByUserID";
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID)
            };
            var _alist = new List<OutletsOfUsersList>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new OutletsOfUsersList
                        {
                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            _ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            _Company = Convert.ToString(dt.Rows[i]["_Company"])
                        };
                        _alist.Add(item);
                    }
                }
            }
            catch (Exception er)
            {

            }
            return _alist;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }

}
