using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveBillerAxisBank : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveBillerAxisBank(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (SaveBillerAxisBankRequest)obj;
            var tp_BillerAxisBankUpdate = new DataTable();
            tp_BillerAxisBankUpdate.Columns.Add("BillerID", typeof(string));
            tp_BillerAxisBankUpdate.Columns.Add("Name", typeof(string));
            tp_BillerAxisBankUpdate.Columns.Add("Type", typeof(string));
            tp_BillerAxisBankUpdate.Columns.Add("ExactNess", typeof(string));
            if (req.billerList.Count > 0)
            {
                foreach (var item in req.billerList)
                {
                    tp_BillerAxisBankUpdate.Rows.Add(new object[] { item.billerId, item.name, item.type, item.exactness });
                }
            }

            SqlParameter[] param = {
                new SqlParameter("@OpTypeID",req.OpTypeID),
                new SqlParameter("@tp_BillerAxisBankUpdate",tp_BillerAxisBankUpdate)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Biller Updated successfully";
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_SaveBillerAxisBank";
    }
}
