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
    public class procGetCustomerType:IProcedure
    {
        private readonly IDAL _dal;
        public procGetCustomerType(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            List<CustomerTypes> customerTypelist = new List<CustomerTypes>();
            SqlParameter[] param =
            {
                new SqlParameter("@LT",_req.LoginTypeID)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                //int i = 0;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        CustomerTypes loantype = new CustomerTypes()
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            CustomerType = dt.Rows[i]["CustomerType"].ToString()
                        };
                        customerTypelist.Add(loantype);
                    }


                    return customerTypelist;
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
            return "proc_GetCustomerType";
        }

        object IProcedure.Call()
        {
            throw new NotImplementedException();
        }
    }
}
