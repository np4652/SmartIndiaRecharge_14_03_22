using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
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
    public class ProcEmpResendPass : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEmpResendPass(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param =
             {
                new SqlParameter("@LoginId",req.LoginID),
                new SqlParameter("@Id",req.CommonInt)
            };
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count>0)
                {
                    _res.Statuscode = ErrorCodes.One;
                    _res.Password = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["_Password"]));
                    _res.Company = Convert.ToString(dt.Rows[0]["Company"]); 
                    _res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]); 
                    _res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountMobileNo"]); 
                    _res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountsEmailID"]); 
                    _res.SupportNumber = Convert.ToString(dt.Rows[0]["MobileSupport"]);
                    _res.Msg = "Password and other details fetched successfully";

                }
            }
            catch(Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_EmpResendPass";
    }
}
