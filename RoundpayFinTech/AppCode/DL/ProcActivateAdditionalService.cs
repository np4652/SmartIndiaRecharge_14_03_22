using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcActivateAdditionalService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcActivateAdditionalService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ActAddonSerReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@OutletID", req.OutletID),
                new SqlParameter("@OpTypeID", req.OpTypeID),
                new SqlParameter("@OID", req.OID),
                new SqlParameter("@IP", req.IP),
                new SqlParameter("@Browser", req.Browser),
                new SqlParameter("@UID", req.UID)
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = GetName(),
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }

            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_ActivateAdditionalService";
    }
}
