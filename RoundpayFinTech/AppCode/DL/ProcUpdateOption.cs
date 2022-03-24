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
    public class ProcUpdateOption : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateOption(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (OperatorOptionalReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@OPId", req.OID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@Name", req.DisplayName??""),
                new SqlParameter("@Remark", req.Remark??""),
                new SqlParameter("@Islist", req.IsList),
                new SqlParameter("@Ismulti", req.IsMultiSelection),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@OptionalType", req.OptionalType)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
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
                    LoginTypeID = 1,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_UpdateOption";
        }
    }
}
