using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Data;
using System.Data.SqlClient;
namespace RoundpayFinTech.AppCode.DL
{
    public class procGetLoanType:IProcedure
    {
        private readonly IDAL _dal;
        public procGetLoanType(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            List<LoanTypes> loantypeList = new List<LoanTypes>();
            SqlParameter[] param =
            {
                new SqlParameter("@LT",_req.LoginTypeID)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                //int i = 0;
                if (dt.Rows.Count>0)
                {
                    for(int i=0;i<dt.Rows.Count;i++)
                    {
                        LoanTypes loantype = new LoanTypes()
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            LoanType = dt.Rows[i]["_LoanType"].ToString()
                        };
                        loantypeList.Add(loantype);
                    }
                    
               
                    return loantypeList;
                }
            }
            catch (Exception ex)
            {

                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return new LoanTypes();
        }
        public string GetName()
        {
            return "proc_GetLoanType";
        }

        object IProcedure.Call()
        {
            throw new NotImplementedException();
        }
    }
}
