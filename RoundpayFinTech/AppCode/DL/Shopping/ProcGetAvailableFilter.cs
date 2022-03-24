using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RoundpayFinTech.AppCode.Model.Shopping;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAvailableFilter : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAvailableFilter(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new FilterAndOptions();
            var Filters = new List<Filter>();
            var Options = new List<FilterOption>();
            SqlParameter[] param = {
                new SqlParameter("@PID",req.CommonInt),
                new SqlParameter("@PDetailID",req.CommonInt2)
            };
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Filters.Add(new Filter
                        {
                            ID = Convert.ToInt32(dr["_FilterID"]),
                            FilterName = Convert.ToString(dr["_Filter"])

                        });
                    }
                }
                dt = ds.Tables[1];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Options.Add(new FilterOption
                        {
                            FilterID = Convert.ToInt32(dr["_FilterID"]),
                            FilterOptionID = Convert.ToInt32(dr["_FilterOptionID"]),
                            Option = Convert.ToString(dr["_Option"]),
                            OptionalID=Convert.ToString(dr["_OptionalDetail"])
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
            res.Filter = Filters;
            res.FilterOption = Options;
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_getAvailableFilter";
    }
}
