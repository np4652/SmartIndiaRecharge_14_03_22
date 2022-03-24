using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserMNPClaim : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserMNPClaim(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (MNPClaimReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@MobileNo",req.MNPMobile),
                new SqlParameter("@ReferenceID",req.ReferenceID),
            };
            var res = new ResponseStatus
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
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
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UserMNPClaim";
    }
}
