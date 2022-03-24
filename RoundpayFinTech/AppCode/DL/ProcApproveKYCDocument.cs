using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcApproveKYCDocument : IProcedure
    {
        private readonly IDAL _dal;
        public ProcApproveKYCDocument(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {           
            var req = (DocTypeMaster)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", req.LoginId),
                 new SqlParameter("@DocID", req.ID),
                 new SqlParameter("@Status", req.VerifyStatus),
                 new SqlParameter("@Remark", req.DRemark??""),
                 new SqlParameter("@LT", req.LoginTypeID),
                 new SqlParameter("@IP", req.DocName??""),
                 new SqlParameter("@Browser", req.DocUrl??""),
        };
        var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt16(dt.Rows[0][0].ToString());
                    _res.Msg = dt.Rows[0][1].ToString();
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
                    UserId = req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ApproveKYC";
    }
}
