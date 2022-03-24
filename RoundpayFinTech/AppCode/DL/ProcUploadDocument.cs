using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUploadDocument : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUploadDocument(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (DocumentReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@URL", _req.URL??string.Empty),
                new SqlParameter("@DocType", _req.DocType),
                new SqlParameter("@ChildUserID", _req.ChildUserID),
                new SqlParameter("@IP", _req.IP??""),
                new SqlParameter("@Browser", _req.Browser??""),
                new SqlParameter("@APIUserOutletID", _req.APIUserOutletID),
            };
            var _res = new ResponseStatus()
            {
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
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UploadDocument";
    }
}