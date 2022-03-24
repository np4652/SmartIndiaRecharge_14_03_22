using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping.WebShopping
{
    public class ProcGetAllFilterList : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetAllFilterList(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<FilterOptionList>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@FilterTypeId", req.CommonInt),
                new SqlParameter("@FilterType", req.CommonStr),
                new SqlParameter("@WebsiteId", req.CommonStr2)
            };
            int id = 1;
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new FilterOptionList
                        {
                            FilterName = dr["filterName"] is DBNull ? "" : dr["filterName"].ToString(),
                            FilterId = dr["filterId"] is DBNull ? 0 : Convert.ToInt32(dr["filterId"]),
                            OptionName = dr["optionname"] is DBNull ? "" : dr["optionname"].ToString(),
                            FilterOptionTypeId = dr["FilterOptionTypeId"] is DBNull ? 0 : Convert.ToInt32(dr["FilterOptionTypeId"]),
                            CategoryId = dr["CategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["CategoryId"])
                           // POSID = dr["POSID"] is DBNull ? 0 : Convert.ToInt32(dr["POSID"])
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetAllFilterList";
    }
}
