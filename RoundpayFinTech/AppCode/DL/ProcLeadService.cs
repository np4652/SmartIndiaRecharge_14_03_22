using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLeadService:IProcedure
    {
        private readonly IDAL _dal;
        public ProcLeadService(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (LeadServiceRequest)obj;
            SqlParameter[] param =
            {
                 new SqlParameter("@LT",_req.LT),
                new SqlParameter("@ID",_req.ID),
                new SqlParameter("@Name",_req.Name),
                new SqlParameter("@Mobile",_req.Mobile),
                new SqlParameter("@Email",_req.Email),
                new SqlParameter("@Age",_req.Age),
                new SqlParameter("@PAN",_req.PAN),
                new SqlParameter("@LoanTypeID",_req.LoanTypeID),
                new SqlParameter("@InsuranceTypeID",_req.InsuranceTypeID),
                new SqlParameter("@Amount",_req.Amount),
                new SqlParameter("@CustomerTypeID",_req.CustomerTypeID),
                new SqlParameter("@RequiredFor",_req.RequiredFor),
                new SqlParameter("@Comments",_req.Comments),
                new SqlParameter("@Remark",_req.Remark),
                new SqlParameter("@EntryBy",_req.EntryBy),
                new SqlParameter("@ModifyBy",_req.ModifyBy),
                new SqlParameter("@RequestModeID",_req.RequestModeID),
                new SqlParameter("@RequestIP",_req.RequestIP),
                new SqlParameter("@Browser",_req.Browser),
                new SqlParameter("@OID",_req.OID),
                new SqlParameter("@BankID",_req.BankID),
                new SqlParameter("@HaveLoan",_req.HaveLoan),
                new SqlParameter("@OccupationType",_req.OccupationType),
                new SqlParameter("@PinCode",_req.PinCode)

            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    //_resp.CommonInt = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]);

                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.EntryBy
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_LeadService";
    }
}
