using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetNumberSeries : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetNumberSeries(IDAL dal) => _dal = dal;
        public Task<object> Call(object obj) => throw new NotImplementedException();

        public async Task<object> Call()
        {
            var res = new List<NumberSeries>();
            try
            {
                DataTable dt = await _dal.GetAsync(GetName());
                foreach (DataRow row in dt.Rows)
                {
                    NumberSeries numberSeries = new NumberSeries
                    {
                        OID = row["OID"] is DBNull ? (short)0 : Convert.ToInt16(row["OID"]),
                        Series = row["Number"] is DBNull ? (short)0 : Convert.ToInt16(row["Number"]),
                        CircleCode = row["CircleCode"] is DBNull ? (short)0 : Convert.ToInt16(row["CircleCode"])
                    };
                    res.Add(numberSeries);
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public string GetName() => "select OID,Number,CircleCode from NumberList order by Number";
    }
}
