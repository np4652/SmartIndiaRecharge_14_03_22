using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcShoppingComm : IProcedure
    {
        private readonly IDAL _dal;

        public ProcShoppingComm(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@productDetailId", req.CommonInt),
                new SqlParameter("@commDistributionType", req.CommonInt2),
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0][1]);
                    if(res.Status == 1)
                    {
                        res.CommonStr = Convert.ToString(dt.Rows[0][2]);
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetRetailerShoppingCommission";
    }
}
