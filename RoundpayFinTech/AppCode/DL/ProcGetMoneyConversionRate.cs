using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Coin;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMoneyConversionRate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMoneyConversionRate(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@OID",req.CommonInt)
            };
            var res = new List<MoneyConversionRate>{};
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new MoneyConversionRate { 
                            FromSymbol= item["_FromSymbol"] is DBNull? string.Empty : item["_FromSymbol"].ToString(),
                            ToSymbol= item["_ToSymbol"] is DBNull?string.Empty:item["_ToSymbol"].ToString(),
                            FromSymbolID= item["_FromSymbolID"] is DBNull?0:Convert.ToInt32(item["_FromSymbolID"]),
                            ToSymbolID = item["_ToSymbolID"] is DBNull ? 0 : Convert.ToInt32(item["_ToSymbolID"]),
                            Rate = item["_Rate"] is DBNull ? 0M : Convert.ToDecimal(item["_Rate"]),
                            Ind = item["_Ind"] is DBNull ? 0 : Convert.ToInt32(item["_Ind"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetMoneyConversionRate";
    }
}
