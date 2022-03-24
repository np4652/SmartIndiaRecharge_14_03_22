using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetShoppingSettings : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetShoppingSettings(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ShoppingSettings();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    res.DefaultMenuLevel = Convert.ToInt16(dt.Rows[0]["_DefaultMenuLevel"]);
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
        public string GetName() => "proc_getShoppingSettings";
    }
}
