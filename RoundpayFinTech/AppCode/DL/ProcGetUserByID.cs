using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserByID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _UserRequset = (UserRequset)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _UserRequset.LoginID ),
                new SqlParameter("@LtID", _UserRequset.LoginTypeID),
                new SqlParameter("@UserID", _UserRequset.UserId),
                new SqlParameter("@MobileNo", _UserRequset.MobileNo ?? "")
            };
            var _res = new UserDetail
            {
                ResultCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.ResultCode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (_res.ResultCode == ErrorCodes.One)
                    {
                        _res.UserID = dt.Rows[0]["UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["UserID"]);
                        _res.MobileNo = dt.Rows[0]["MobileNo"] is DBNull?"": dt.Rows[0]["MobileNo"].ToString();
                        _res.Name = dt.Rows[0]["Name"] is DBNull?"": dt.Rows[0]["Name"].ToString();
                        _res.OutletName = dt.Rows[0]["OutletName"] is DBNull ? "" : dt.Rows[0]["OutletName"].ToString();
                        _res.EmailID = dt.Rows[0]["EmailID"] is DBNull ? "" : dt.Rows[0]["EmailID"].ToString();
                        _res.RoleID = dt.Rows[0]["RoleID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["RoleID"]);
                        _res.SlabID = dt.Rows[0]["SlabID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["SlabID"]);
                        _res.ReferalID = dt.Rows[0]["ReferalID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ReferalID"]);
                        _res.IsGSTApplicable = dt.Rows[0]["IsGSTApplicable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsGSTApplicable"]);
                        _res.IsTDSApplicable = dt.Rows[0]["IsTDSApplicable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsTDSApplicable"]);
                        _res.IsVirtual = dt.Rows[0]["IsVirtual"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsVirtual"]);
                        _res.IsWebsite = dt.Rows[0]["IsWebsite"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsWebsite"]);
                        _res.IsRealAPI = dt.Rows[0]["IsRealApi"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsRealApi"]);
                        _res.IsAdminDefined = dt.Rows[0]["IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsAdminDefined"]);
                        _res.IsSurchargeGST = dt.Rows[0]["ISSurchargeGST"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["ISSurchargeGST"]);
                        _res.Token = dt.Rows[0]["_Token"] is DBNull ? "" : dt.Rows[0]["_Token"].ToString();
                        _res.StateID = dt.Rows[0]["_stateID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_stateID"]);
                        _res.Address = dt.Rows[0]["_Address"] is DBNull ? "" : dt.Rows[0]["_Address"].ToString();
                        _res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? "" : dt.Rows[0]["_Pincode"].ToString();
                        _res.City = dt.Rows[0]["_City"] is DBNull ? "" : dt.Rows[0]["_City"].ToString();
                        _res.State = dt.Rows[0]["_StateName"] is DBNull ? "" : dt.Rows[0]["_StateName"].ToString();

                    }
                }
            }
            catch (Exception ex){}
            return _res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserByID";
    }
}
