using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetBranddetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBranddetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@CategoryID",req.CommonInt)
            };
            var res = new List<Brand>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new Brand
                        {        
                            BrandId = Convert.ToInt32(dr["_BrandId"]),
                            CategoryID = Convert.ToInt32(dr["_CategoryID"]),
                            BrandName = Convert.ToString(dr["_BrandName"]),
                            CategoryName = Convert.ToString(dr["_CategoryName"]),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"], CultureInfo.InvariantCulture)
                        });
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetBranddetail";
    }
}