using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateCircleSwitch : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateCircleSwitch(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CircleReq)obj;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            SqlParameter[] param ={
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.circle.OID),
                new SqlParameter("@APIID", _req.circle.APIID),
                new SqlParameter("@CircleID", _req.circle.ID),
                new SqlParameter("@IsActive", _req.circle.IsActive),
                new SqlParameter("@IP",_req.CommonStr),
                new SqlParameter("@Browser", _req.CommonStr2),
                new SqlParameter("@MaxCount", _req.CommonInt)
            };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
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
        public string GetName() => "proc_UpdateCircleSwitch";
    }
}