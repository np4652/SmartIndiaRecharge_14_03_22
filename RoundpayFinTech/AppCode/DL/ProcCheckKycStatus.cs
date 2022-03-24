using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCheckKycStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCheckKycStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@UserID",req.LoginID)
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.CommonInt = Convert.ToInt32(dt.Rows[0][0]);
                    resp.CommonInt2 = Convert.ToInt32(dt.Rows[0][1]);
                    resp.Statuscode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
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
            return resp;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "select ISNULL(l._KYCStatus,1),u._RoleID from tbl_UsersLogin l ,tbl_Users u where u._Id=l._UserId and l._UserId=@UserID";
    }
}
