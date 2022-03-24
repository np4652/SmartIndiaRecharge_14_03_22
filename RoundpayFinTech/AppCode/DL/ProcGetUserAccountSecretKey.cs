using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserAccountSecretKey : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetUserAccountSecretKey(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _loginDetail = (LoginDetail)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _loginDetail.LoginID),
                new SqlParameter("@Password", _loginDetail.Password),
                new SqlParameter("@Prefix", _loginDetail.Prefix),
            };

            var _lr = new LoginResponse();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _lr.ResultCode = Convert.ToInt32(dt.Rows[0][0]);
                    _lr.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_lr.ResultCode == ErrorCodes.One)
                    {
                        _lr.AccountSecretKey = dt.Rows[0]["_AccountSecretKey"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AccountSecretKey"]);
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
                    LoginTypeID = _loginDetail.LoginTypeID,
                    UserId = _loginDetail.LoginID
                });
            }
            return _lr;
        }

        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "";
    }
}
