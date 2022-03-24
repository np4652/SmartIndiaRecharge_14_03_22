using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFundRejectProcess : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFundRejectProcess(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (FundProcessReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Remark", _req.fundProcess.Remark ?? ""),
                new SqlParameter("@RequestMode", _req.fundProcess.RequestMode),                
                new SqlParameter("@PaymentID", _req.fundProcess.PaymentId),
                new SqlParameter("@IP", _req.CommonStr ?? ""),
                new SqlParameter("@Browser", _req.CommonStr2 ?? "")
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
            catch (Exception)
            {
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_FundRejectProcess";
    }
}