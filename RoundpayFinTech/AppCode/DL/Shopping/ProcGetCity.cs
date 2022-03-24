using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetCity : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetCity(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<City>();
            var req = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@StateID", req),
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new City
                        {
                            CityID = Convert.ToInt32(dr["_ID"]),
                            CityName = Convert.ToString(dr["_City"])
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
        public string GetName() => @"select * from Master_City order by _ID";
    }
}
