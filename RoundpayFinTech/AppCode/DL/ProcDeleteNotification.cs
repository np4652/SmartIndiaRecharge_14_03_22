using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDeleteNotification : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeleteNotification(IDAL dal) {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var ID = (int)obj;
            var res = new ResponseStatus {
                Statuscode=ErrorCodes.One,
                Msg=ErrorCodes.SUCCESS
            };
            try
            {
                var _ = _dal.GetByProcedure(GetName(),new SqlParameter[]{ new SqlParameter("@ID",ID)});
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog {
                    ClassName=GetType().Name,
                    FuncName="Call",
                    Error=ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ErrorCodes.UNSUCCESS;
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_DeleteNotification";
        }
    }
}
