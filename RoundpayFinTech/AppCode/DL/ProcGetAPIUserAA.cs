using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPIUserAA : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPIUserAA(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (AAUserReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TopRows",req.TopRows),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@MobileNo",req.MobileNo??"")
            };
            var _resp = new AAUserData();
            var res = new List<AAUserList>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) 
                {
                    _resp.StatusCode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_resp.StatusCode == ErrorCodes.One) 
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new AAUserList
                            {
                                UserID = item["_UserID"] is DBNull ? 0 : Convert.ToInt32(item["_UserID"]),
                                OutletName = item["_OutletName"] is DBNull ? string.Empty : item["_OutletName"].ToString(),
                                MobileNo = item["_MobileNo"] is DBNull ? string.Empty : item["_MobileNo"].ToString(),
                                Email = item["_EmailID"] is DBNull ? string.Empty : item["_EmailID"].ToString(),
                                PAN = item["_PAN"] is DBNull ? string.Empty : item["_PAN"].ToString(),
                                AADHAR = item["_AADHAR"] is DBNull ? string.Empty : item["_AADHAR"].ToString(),
                                Agreementstatus = item["_Agreementstatus"] is DBNull ? false : Convert.ToBoolean(item["_Agreementstatus"]),
                                AgreementApprovedBy = item["_AgreementApprovedBy"] is DBNull ? 0 : Convert.ToInt32(item["_AgreementApprovedBy"]),
                                AgreementRemark = item["_AgreementRemark"] is DBNull ? string.Empty : item["_AgreementRemark"].ToString(),
                            });
                        }
                        _resp.AAUser = res;
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
                    UserId = req.LoginID
                });
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetAPIUserAA";
        
    }
}
