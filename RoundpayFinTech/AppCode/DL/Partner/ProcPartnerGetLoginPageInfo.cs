using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Partner
{
    public class ProcPartnerGetLoginPageInfo : IProcedure
    {
        private readonly IDAL _dal;
        public ProcPartnerGetLoginPageInfo(IDAL dal) => _dal = dal;
        
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@PartnerId", _req.CommonInt)
            };
            PartnerAEPSResponseModel res = new PartnerAEPSResponseModel()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NORESPONSE
            };
            try
            {
                string query = "SELECT _OutletName, _EmailID, _MobileNo, _Address FROM tbl_Partner WHERE _UserID = " + _req.CommonInt;
                var dt = _dal.Get(query);
                if (dt != null && dt.Rows.Count > 0)
                {
                    res.StatusCode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    res.CompanyName = dt.Rows[0]["_OutletName"].ToString();
                    res.CompanyEmail = dt.Rows[0]["_EmailID"].ToString();
                    res.CompanyMobile = dt.Rows[0]["_MobileNo"].ToString();
                    res.CompanyAddress = dt.Rows[0]["_Address"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "SELECT _OutletName, _EmailID, _MobileNo, _Address FROM tbl_Partner WHERE _UserID = 231";
    }
}
