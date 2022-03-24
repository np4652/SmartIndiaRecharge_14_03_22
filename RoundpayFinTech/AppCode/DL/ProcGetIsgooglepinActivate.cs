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
    public class ProcGetIsgooglepinActivate : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetIsgooglepinActivate(IDAL dal)
        {
            _dal = dal;
        }

        public object Call() => throw new NotImplementedException();

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.CommonInt),
                new SqlParameter("@Mobileno",req.CommonStr),
            };
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            //bool res = false;
            try
            {
                DataTable dt = _dal.Get("Select _Is_Google_2FA_Enable, _RoleID , isnull(_AccountSecretKey,'')  _AccountSecretKey , u._ID from Tbl_userslogin ul inner join Tbl_Users u on u._ID = ul._UserID left join tbl_TwoFactAuth_Credentials tf  on  tf._UserID = u._ID where u._ID = @UserID or u._MobileNo=@Mobileno", param);
                if (dt.Rows.Count > 0)
                {

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        _res.Statuscode = ErrorCodes.One;
                        _res.CommonBool = Convert.ToBoolean(dt.Rows[0]["_Is_Google_2FA_Enable"]);
                        _res.CommonInt = Convert.ToInt32(dt.Rows[0]["_RoleID"]);
                        _res.CommonInt2 = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        _res.CommonStr = dt.Rows[0]["_AccountSecretKey"] is DBNull ? "" : dt.Rows[0]["_AccountSecretKey"].ToString();
                    }

                }
            }
            catch (Exception ex)
            {
            }
            return _res;
        }

        public string GetName() => throw new NotImplementedException();

    }

}
