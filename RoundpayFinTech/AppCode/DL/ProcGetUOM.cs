using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUOM : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetUOM(IDAL dal) => _dal = dal;

		public object Call()
		{
			var res = new List<UOM>();
			try
			{
				DataTable dt = _dal.Get(GetName());
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						res.Add(new UOM
						{
							ID = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
							Uom = Convert.ToString(dr["_Name"], CultureInfo.InvariantCulture),							
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
					LoginTypeID = 1,
					UserId = 1
				};
				var _ = new ProcPageErrorLog(_dal).Call(errorLog);
			}
			return res;
		}

		public object Call(object obj) => throw new NotImplementedException();
		public string GetName() => @"select * from Master_UOM where _IsActive=1";
	}
}
