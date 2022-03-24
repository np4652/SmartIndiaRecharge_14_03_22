using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDocumentOnboard: IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDocumentOnboard(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@UserID", _req.LoginID),
                new SqlParameter("@OutletID", _req.CommonInt),
            };
            var _alist = new List<KYCDoc>();            
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new KYCDoc
                        {
                            DOCURL = Convert.ToString(dt.Rows[i]["docurl"]),
                            DocTypeID = Convert.ToInt32( dt.Rows[i]["doctypeid"]),
                        };
                        _alist.Add(item);
                    }
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
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetDocumentOnboard";
    }
}
