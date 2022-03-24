using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLogBillerReqResp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcLogBillerReqResp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID",req.CommonInt),
                new SqlParameter("@OID",req.CommonInt2),
                new SqlParameter("@Request",req.CommonStr??string.Empty),
                new SqlParameter("@Response",req.CommonStr2??string.Empty)
            };
            try
            {
                _dal.Execute(GetName(),param);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 0,
                    UserId = req.CommonInt2
                });
            }
            return true;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "insert into Log_BillerReqResp(_APIID,_OID,_Request,_Response,_EntryDate)values(@APIID,@OID,@Request,@Response,GETDATE())";
    }
}
