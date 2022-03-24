using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcChangeEmpSts : IProcedure
    {
        private readonly IDAL _dal;
        public ProcChangeEmpSts(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param =
             {
                new SqlParameter("@Id",req.CommonInt),
                new SqlParameter("@Is",req.CommonInt2)
            };

            var _res = new ResponseStatus
               {
                Statuscode= ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = ErrorCodes.TempError;
                }
            }
            catch(Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "update tbl_employee set _IsActive=@Is  where _ID=@Id ";
    }
}
