using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcBillFetchSummary : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcBillFetchSummary(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var UserID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", UserID),
               // new SqlParameter("@WID", req.CommonInt2)
                
            };
            var resp = new BillFetchSummary();
            try
            {
                var dt = await _dal.GetAsync(GetName(), param);
                foreach (DataRow row in dt.Rows)
                {
                    resp = new BillFetchSummary
                    {
                        TotalFetchBill = Convert.ToInt32(row["_TotalFetchBill"]),
                        TotalSuccess = Convert.ToInt32(row["_TotalSuccess"]),
                        TotalFailed = Convert.ToInt32(row["_TotalFailed"]),
                        TotalPaid = Convert.ToInt32(row["_TotalPaid"]),
                    };
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
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public async Task<object> Call() => throw new NotImplementedException();

        public string GetName() => @"declare @TotalFetchBill int,@TotalSuccess int,@TotalFailed int, @TotalPaid int
                                    SELECT @TotalFetchBill=COUNT(1) FROM Log_FetchBill(nolock) where cast(_EntryDate as date)=  cast(getdate() as date)
                                    SELECT @TotalSuccess=COUNT(1) FROM Log_FetchBill(nolock) where cast(_EntryDate as date)=  cast(getdate() as date) and _Status = 1
                                    SELECT @TotalFailed=COUNT(1) FROM Log_FetchBill(nolock) where cast(_EntryDate as date)=  cast(getdate() as date) and _Status = 2
                                    SELECT @TotalPaid=COUNT(1) FROM Log_FetchBill(nolock) where cast(_EntryDate as date)=  cast(getdate() as date) and _IsProcessed = 1
                                    select @TotalFetchBill _TotalFetchBill,@TotalSuccess _TotalSuccess , @TotalFailed _TotalFailed , @TotalPaid _TotalPaid";
    }
}
