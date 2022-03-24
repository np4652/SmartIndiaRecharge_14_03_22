using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAreaUserWiseCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAreaUserWiseCU(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            ASAreaMaster _req = (ASAreaMaster)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@ID", _req.AreaID),
                new SqlParameter("@Area", _req.Area??string.Empty)
            };
            var _res = new ResponseStatus { 
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
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
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AreaUserWiseCU";
    }
}
