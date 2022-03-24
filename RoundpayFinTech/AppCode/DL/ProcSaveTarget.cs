using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode
{
    public class ProcSaveTarget : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveTarget(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (TargetModelReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.Detail.SlabID),
                new SqlParameter("@OID",req.Detail.OID),
                new SqlParameter("@Target",req.Detail.Target),
                new SqlParameter("@Comm",req.Detail.Comm),
                new SqlParameter("@AmtType",req.Detail.AmtType),
                new SqlParameter("@IsEarned",req.Detail.IsEarned),
                new SqlParameter("@IsGift",req.Detail.IsGift),
                new SqlParameter("@RoleID",req.Detail.RoleID),
                new SqlParameter("@TargetTypeID",req.Detail.TargetTypeID),
                new SqlParameter("@HikePer",req.Detail.HikePer),
                new SqlParameter("@IsHikeOnEarned",req.Detail.IsHikeOnEarned),
                new SqlParameter("@IP",req.CommonStr??string.Empty),
                new SqlParameter("@Browser",req.CommonStr2??string.Empty),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_SaveTarget";
    }
}
