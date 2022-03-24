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
    public class ProcGetFilter : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetFilter(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<Filter>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LoginID", req.LoginID)
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
							IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"], CultureInfo.InvariantCulture),
							IsEditable= dr["_IsEditable"] is DBNull ? false : Convert.ToBoolean(dr["_IsEditable"], CultureInfo.InvariantCulture),
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
		public string GetName() => @"select * from Master_Filter where (@LoginID<>1 and _IsActive=0) or @LoginID=1";
	}
}
