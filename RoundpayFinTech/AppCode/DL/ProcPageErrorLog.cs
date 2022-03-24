using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPageErrorLog : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPageErrorLog(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            ErrorLog _req = (ErrorLog)obj;
            SqlParameter[] param = {
                new SqlParameter("@ClsName", _req.ClassName),
                new SqlParameter("@FnName", _req.FuncName),
                new SqlParameter("@UserID", _req.UserId),
                new SqlParameter("@Error", _req.Error),
                new SqlParameter("@LoginTypeID", _req.LoginTypeID)
            };
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt16(dt.Rows[0][0].ToString());
                    _res.Msg = dt.Rows[0][1].ToString();
                }
            }
            catch (Exception er)
            { }
            return _res;
        }
        public Task<object> Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "Proc_PageErrorLog";
        }
    }
    public class ProcLogAPITokenGeneration : IProcedure
    {
        private IDAL _dal;
        public ProcLogAPITokenGeneration(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APICode",req.str),
                new SqlParameter("@Req",req.CommonStr??string.Empty),
                new SqlParameter("@Resp",req.CommonStr2??string.Empty)
            };
            try
            {
                _dal.Execute(GetName(), param);
                return true;
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return false;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "insert into Log_APITokenGeneration(_APICode,_Request,_Response,_EntryDate)values(@APICode,@Req,@Resp,getdate())";
    }
}
