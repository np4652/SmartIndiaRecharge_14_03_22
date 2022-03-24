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
    public class ProcUpdateDMRTransaction : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateDMRTransaction(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            //@TID int,@Status int,@LiveID varchar(50),@VendorID varchar(50),@Request nvarchar(max), @Response nvarchar(max)
            DMRTransactionResponse req = (DMRTransactionResponse)obj;
            SqlParameter[] param =
                {
                new SqlParameter("@TID", req.TID),
                new SqlParameter("@Status", req.Statuscode),
                new SqlParameter("@LiveID",req.LiveID??""),
                new SqlParameter("@VendorID",req.VendorID??string.Empty),
                new SqlParameter("@VendorID2",req.VendorID2??string.Empty),
                new SqlParameter("@Request",req.Request??string.Empty),
                new SqlParameter("@Response",req.Response??string.Empty),
                new SqlParameter("@BeneName",req.BeneName??string.Empty)
            };
            var _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Columns.Contains("Msg") ? dt.Rows[0]["Msg"].ToString() : "";
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
                    UserId = req.TID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UPdateDMRTransaction";
    }
}
