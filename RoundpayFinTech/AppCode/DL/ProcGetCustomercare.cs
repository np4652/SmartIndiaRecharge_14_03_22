using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCustomercare : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCustomercare(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            List<CustomerCareDetails> _res = new List<CustomerCareDetails>();
            SqlParameter[] param ={
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
             };
            try
            {
                DataTable dt = _dal.Get("Select _ID, _Name from Tbl_customercare");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CustomerCareDetails Cus = new CustomerCareDetails();
                    Cus.CustomercareID = Convert.ToInt32(dt.Rows[i]["_ID"]);
                    Cus.CustomerCareName = dt.Rows[i]["_Name"].ToString();
                    _res.Add(Cus);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
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

        public string GetName()
        {
            return "";
        }
    }
}
