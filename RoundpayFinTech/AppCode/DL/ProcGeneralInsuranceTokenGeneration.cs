using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGeneralInsuranceTokenGeneration : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGeneralInsuranceTokenGeneration(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (TokenGenerationModel)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@OutletID",req.OutletID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@TransactionID",req.TransactionID??string.Empty),
                new SqlParameter("@RequestMode",req.RequestMode),
                new SqlParameter("@APIRequestID",req.APIRequestID??string.Empty),
                new SqlParameter("@VendorID",req.VendorID??string.Empty)
            };
            var res = new GeneralInsuranceDBResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                AgentID= req.OutletID.ToString(),
                Token=req.TransactionID
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.GenerateTokenURL = Convert.ToString(dt.Rows[0]["_GenerateTokenURL"]);
                        res.RechType = Convert.ToString(dt.Rows[0]["_RechType"]);
                        
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
                    LoginTypeID = req.RequestMode,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GeneralInsuranceTokenGeneration";
    }
}
