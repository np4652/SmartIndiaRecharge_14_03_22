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
    public class ProcGetErrorCodeDescByAPIGroup : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetErrorCodeDescByAPIGroup(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            req.CommonStr2 = (req.CommonStr2 ?? string.Empty).Trim(',').Replace(" ", "").ToUpper();
            SqlParameter[] param = {
                new SqlParameter("@APIGroupCode",req.CommonStr??string.Empty),
                new SqlParameter("@APIErrorCode",req.CommonStr2)
            };
            var res = new ErrorCodeDetail();
            try
            {
                var dt = _dal.GetByProcedureAdapter(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Code = dt.Rows[0]["_Code"] is DBNull ? string.Empty : dt.Rows[0]["_Code"].ToString();
                    res.Error = dt.Rows[0]["_Error"] is DBNull ? string.Empty : dt.Rows[0]["_Error"].ToString();
                    res.Status = dt.Rows[0]["_Status"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Status"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetErrorCodeDescByAPIGroup";
    }
}
