using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserByMobile : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserByMobile(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var _res = new WTWUserInfo();
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@MobileNo",req.CommonStr??"")
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.StatusCode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if ((dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0])) == 1)
                    {
                        _res.OutletName = dt.Rows[0]["_OutletName"] is DBNull ? "" : dt.Rows[0]["_OutletName"].ToString();
                        _res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        _res.RoleID = dt.Rows[0]["_RoleID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_RoleID"]);
                        _res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString();
                        _res.IsPrepaidB = dt.Rows[0]["_IsPrepaidB"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPrepaidB"]);
                        _res.IsUtilityB = dt.Rows[0]["_IsUtilityB"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsUtilityB"]);
                        _res.IsBankB = dt.Rows[0]["_IsBankB"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBankB"]);
                        _res.IsDoubleFactor = dt.Rows[0]["_IsDoubleFactor"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDoubleFactor"]);
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
            return _res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserByMobile";
    }
}
