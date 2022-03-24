using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetFilterWithOption : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetFilterWithOption(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<FilterWithOptions>();
			var mappedFilters = new List<Filter>();
			var FilterOptions = new List<FilterOption>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@SubCategoryId2", req.CommonInt),
				new SqlParameter("@SubCategoryId1", req.CommonInt2),
                new SqlParameter("@CategoryID", req.CommonInt3),
                new SqlParameter("@filters", req.CommonStr)
            };
			try
			{
				DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
				if (ds.Tables.Count > 0) {
					DataTable dt = new DataTable();
					dt = ds.Tables[0];
					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							var data = new Filter
							{
								ID = Convert.ToInt32(dr["_Id"]),
								FilterName = Convert.ToString(dr["_Filter"]),
							};
							mappedFilters.Add(data);
						}
					}
					dt = ds.Tables[1];
					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							var data = new FilterOption
							{
								FilterID = Convert.ToInt32(dr["_FilterID"]),
								ID = Convert.ToInt32(dr["_Id"]),
								Option = Convert.ToString(dr["_Option"]),
								OptionalID = Convert.ToString(dr["_OptionalDetail"]),
							};
							FilterOptions.Add(data);
						}
					}
					foreach (var item in mappedFilters)
					{
						res.Add(new FilterWithOptions
						{
							FilterID = item.ID,
							FilterName = item.FilterName,
							FilterOption = FilterOptions.Where(x => x.FilterID == item.ID)
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
		public string GetName() => "Proc_GetFilterWithOption";
	}
}
