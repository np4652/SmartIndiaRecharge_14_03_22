using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMappedFilter : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetMappedFilter(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<Filter>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				//new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@CID", req.CommonInt)
			};
			try
			{
				DataTable dt = _dal.Get(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						res.Add(new Filter
						{
							ID = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
							FilterName = Convert.ToString(dr["_Filter"], CultureInfo.InvariantCulture),
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
		public string GetName() => @"select f._ID,f._Filter,ISNULL(m._IsActive,0)  _IsActive from Master_Filter f left join tbl_Filter_CategoryMapping m on M._FilterID = f._ID and M._CategoryID=@CID and m._IsActive=1";
	}
}
