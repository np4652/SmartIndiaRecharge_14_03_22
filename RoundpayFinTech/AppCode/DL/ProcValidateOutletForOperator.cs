using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcValidateOutletForOperator : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValidateOutletForOperator(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OutletID", _req.CommonInt),
                new SqlParameter("@OID",_req.CommonInt2),
                new SqlParameter("@OPID", _req.CommonStr??""),
                new SqlParameter("@PartnerID", _req.CommonInt3)
            };
            var _res = new ValidateAPIOutletResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    _res.ErrorCode = dt.Rows[0]["ErrorCode"] is DBNull ? _res.ErrorCode : Convert.ToInt16(dt.Rows[0]["ErrorCode"]);
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString().Trim();
                        _res.OutletName = dt.Rows[0]["_Company"] is DBNull ? string.Empty : dt.Rows[0]["_Company"].ToString().Trim();
                        _res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                        _res.EmailID = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                        _res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? "0" : dt.Rows[0]["_Pincode"].ToString();
                        _res.Address = dt.Rows[0]["_Address"] is DBNull ? string.Empty : dt.Rows[0]["_Address"].ToString();
                        _res.PAN = dt.Rows[0]["_PAN"] is DBNull ? string.Empty : dt.Rows[0]["_PAN"].ToString();
                        _res.AADHAR = dt.Rows[0]["_AADHAR"] is DBNull ? string.Empty : dt.Rows[0]["_AADHAR"].ToString();
                        _res.OutletVerifyStatus = dt.Rows[0]["_VerifyStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_VerifyStatus"]);
                        _res.IsOutletActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]);
                        _res.IsOutsider = dt.Rows[0]["_IsOutsider"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOutsider"]);
                        _res.KYCStatus = dt.Rows[0]["_KYCStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_KYCStatus"]);
                        _res.StateID = dt.Rows[0]["_StateID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_StateID"]);
                        _res.CityID = dt.Rows[0]["_CityID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_CityID"]);
                        _res.StateName = dt.Rows[0]["_State"] is DBNull ? string.Empty : dt.Rows[0]["_State"].ToString();
                        _res.DistrictID = dt.Rows[0]["_CityID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_CityID"]);
                        _res.City = dt.Rows[0]["_City"] is DBNull ? string.Empty : dt.Rows[0]["_City"].ToString();
                        _res.DOB = dt.Rows[0]["_DOB"] is DBNull ? string.Empty : dt.Rows[0]["_DOB"].ToString();
                        _res.ShopType = dt.Rows[0]["_ShopType"] is DBNull ? string.Empty : dt.Rows[0]["_ShopType"].ToString();
                        _res.Qualification = dt.Rows[0]["_Qualification"] is DBNull ? string.Empty : dt.Rows[0]["_Qualification"].ToString();
                        _res.Poupulation = dt.Rows[0]["_Poupulation"] is DBNull ? string.Empty : dt.Rows[0]["_Poupulation"].ToString();
                        _res.LocationType = dt.Rows[0]["_LocationType"] is DBNull ? string.Empty : dt.Rows[0]["_LocationType"].ToString();
                        _res.Landmark = dt.Rows[0]["_Landmark"] is DBNull ? " " : dt.Rows[0]["_Landmark"].ToString();
                        _res.AlternateMobile = dt.Rows[0]["_AlternateMobile"] is DBNull ? string.Empty : dt.Rows[0]["_AlternateMobile"].ToString();
                        _res.Latlong = dt.Rows[0]["_Latlong"] is DBNull ? string.Empty : dt.Rows[0]["_Latlong"].ToString();
                        _res.BankName = dt.Rows[0]["_BankName"] is DBNull ? string.Empty : (dt.Rows[0]["_BankName"].ToString());
                        _res.IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : (dt.Rows[0]["_IFSC"].ToString());
                        _res.AccountNumber = dt.Rows[0]["_AccountNumber"] is DBNull ? string.Empty : (dt.Rows[0]["_AccountNumber"].ToString());
                        _res.AccountName = dt.Rows[0]["_AccountHolder"] is DBNull ? string.Empty : (dt.Rows[0]["_AccountHolder"].ToString());
                        _res.IsOutletFound = dt.Rows[0]["_IsOutletFound"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOutletFound"]);
                        _res.IsAPIOutletFound = dt.Rows[0]["_IsAPIOutletFound"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAPIOutletFound"]);
                        _res.APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                        _res.APIOutletVerifyStatus = dt.Rows[0]["_APIVerifyStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIVerifyStatus"]);
                        _res.APIOutletDocVerifyStatus = dt.Rows[0]["_DocVerifyStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_DocVerifyStatus"]);
                        _res.BBPSID = dt.Rows[0]["_BBPSID"] is DBNull ? string.Empty : dt.Rows[0]["_BBPSID"].ToString();
                        _res.BBPSStatus = dt.Rows[0]["_BBPSStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_BBPSStatus"]);
                        _res.AEPSID = dt.Rows[0]["_AEPSID"] is DBNull ? string.Empty : dt.Rows[0]["_AEPSID"].ToString();
                        _res.AEPSStatus = dt.Rows[0]["_AEPSStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_AEPSStatus"]);
                        _res.PANRequestID = dt.Rows[0]["_PANRequestID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PANRequestID"]);
                        _res.PANID = dt.Rows[0]["_PANID"] is DBNull ? string.Empty : dt.Rows[0]["_PANID"].ToString();
                        _res.PANStatus = dt.Rows[0]["_PANStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_PANStatus"]);
                        _res.DMTID = dt.Rows[0]["_DMTID"] is DBNull ? string.Empty : dt.Rows[0]["_DMTID"].ToString();
                        _res.DMTStatus = dt.Rows[0]["_DMTStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_DMTStatus"]);
                        _res.RailID = dt.Rows[0]["_RailID"] is DBNull ? string.Empty : dt.Rows[0]["_RailID"].ToString();
                        _res.RailStatus = dt.Rows[0]["_RailStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_RailStatus"]);
                        _res.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        _res.APIID = dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIID"]);
                        _res.IsOutletRequired = dt.Rows[0]["_IsOutletRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOutletRequired"]);
                        _res.OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]);
                        _res.SCode = dt.Rows[0]["_SCode"] is DBNull ? string.Empty : dt.Rows[0]["_SCode"].ToString();
                        _res.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        _res.IsBBPS = dt.Rows[0]["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]);
                        _res.ServiceTypeID = dt.Rows[0]["_ServiceTypeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ServiceTypeID"]);
                        _res.OPID = dt.Rows[0]["_OPID"] is DBNull ? string.Empty : dt.Rows[0]["_OPID"].ToString();
                        _res.OPTypeID = dt.Rows[0]["_OpTypeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_OpTypeID"]);
                        _res.AdminRejectRemark = dt.Rows[0]["_AdminRejectRemark"] is DBNull ? string.Empty : dt.Rows[0]["_AdminRejectRemark"].ToString();
                        _res.UserID = _req.LoginID;
                        _res.OutletID = _req.CommonInt;
                        _res.FixedOutletID = dt.Rows[0]["_FixedOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_FixedOutletID"].ToString();
                        _res.IsOutletManual = dt.Rows[0]["_IsOutletManual"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOutletManual"]);
                        _res.SenderLimit = dt.Rows[0]["_SenderLimit"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SenderLimit"]);
                        _res.MaxLimitPerTransaction = dt.Rows[0]["_MaxLimitPerTransaction"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_MaxLimitPerTransaction"]);
                        _res.CBA = dt.Rows[0]["_CBA"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CBA"]);
                        _res.VID = dt.Rows[0]["_VID"] is DBNull ? string.Empty : dt.Rows[0]["_VID"].ToString();
                        _res.UIDToken = dt.Rows[0]["_UIDToken"] is DBNull ? string.Empty : dt.Rows[0]["_UIDToken"].ToString();
                        _res.BranchName = dt.Rows[0]["_BranchName"] is DBNull ? string.Empty : dt.Rows[0]["_BranchName"].ToString();
                        _res.APIOpCode = dt.Rows[0]["_APIOpCode"] is DBNull ? string.Empty : dt.Rows[0]["_APIOpCode"].ToString();
                        _res.APIType = dt.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIType"]);
                        _res.APIGroupCode = dt.Rows[0]["_APIGroupCode"] is DBNull ? string.Empty : dt.Rows[0]["_APIGroupCode"].ToString();
                        _res.WebsiteName = dt.Rows[0]["_WebsiteName"] is DBNull ? string.Empty : dt.Rows[0]["_WebsiteName"].ToString();
                        _res.APIOutletPassword = dt.Rows[0]["_APIOutletPassword"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletPassword"].ToString();
                        _res.APIOutletPIN = dt.Rows[0]["_APIOutletPIN"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletPIN"].ToString();
                        _res.PartnerID = dt.Rows[0]["_PartnerID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PartnerID"]);
                        _res.EKYCID = dt.Rows[0]["_EKYCID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EKYCID"]);
                        _res.APIEKYCStatus = dt.Rows[0]["_APIEKYCStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIEKYCStatus"]);
                        _res.TwoWayAuthDate = dt.Rows[0]["_TwoWayAuthDate"] is DBNull ? string.Empty : dt.Rows[0]["_TwoWayAuthDate"].ToString();
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ValidateOutletForOperator";
    }
}