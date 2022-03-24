using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateKYCStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateKYCStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (KYCStatusReq)obj;
            SqlParameter[] param = 
            {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OutletID",req.OutletID),
                new SqlParameter("@KYCStatus",req.KYCStatus),
                new SqlParameter("@IP",req.IP??""),
                new SqlParameter("@Browser",req.Browser??""),
                new SqlParameter("@RequestModeID",req.RequestModeID)
            };
            var resp = new ResponseStatus {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (resp.Statuscode == 1)
                    {
                        resp.CommonStr = dt.Rows[0]["KYCStsName"] is DBNull ? "" : dt.Rows[0]["KYCStsName"].ToString();
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateKYCStatus";
    }
}
