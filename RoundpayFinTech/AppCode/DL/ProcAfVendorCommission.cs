using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAfVendorCommission : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAfVendorCommission(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int VendorID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@VendorID", VendorID)
            };
            var res = new List<AffiliateVendorCommission>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new AffiliateVendorCommission
                        {
                            OID = Convert.ToInt32(dr["_OID"]),
                            Operator = Convert.ToString(dr["_Operator"]),
                            Commission = Convert.ToDecimal(dr["_Comm"]),
                            AmtType = Convert.ToInt32(dr["_AmtType"])
                        };
                        res.Add(data);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }


        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetAfVendorCommission";
    }
}
