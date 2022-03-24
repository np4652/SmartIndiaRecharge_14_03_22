using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcEKYCByGSTIN : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEKYCByGSTIN(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (EKYCByGSTINProcReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@LegalName",req.LegalName??string.Empty),
                new SqlParameter("@TradeName",req.TradeName??string.Empty),
                new SqlParameter("@AuthorisedSignatory",req.AuthorisedSignatory??string.Empty),
                new SqlParameter("@EmailID",req.EmailID??string.Empty),
                new SqlParameter("@MobileNo",req.MobileNo??string.Empty),
                new SqlParameter("@GSTIN",req.GSTIN??string.Empty),
                new SqlParameter("@Address",req.Address??string.Empty),
                new SqlParameter("@StateJurisdiction",req.StateJurisdiction??string.Empty),
                new SqlParameter("@CentralJurisdiction",req.CentralJurisdiction??string.Empty),
                new SqlParameter("@RegisterDate",req.RegisterDate??string.Empty),
                new SqlParameter("@AgreegateturnOver",req.AgreegateturnOver??string.Empty),
                new SqlParameter("@PANNumber",req.PANNumber??string.Empty),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@IsSkip",req.IsSkip),
                new SqlParameter("@IsExternal",req.IsExternal),
                new SqlParameter("@ChildUserID",req.ChildUserID),
                new SqlParameter("@APIStatus",req.APIStatus),
                new SqlParameter("@CompanyTypeID",req.CompanyTypeID)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One) {
                        res.CommonInt = dt.Rows[0]["_EKYCID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EKYCID"]);
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
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_EKYCByGSTIN";
    }
}
