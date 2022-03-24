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
    public class ProcGetColors : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetColors(IDAL dal) => _dal = dal;

		public object Call()
		{
			var res = new List<Colors>();
			try
			{
				DataTable dt = _dal.Get(GetName());
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						res.Add(new Colors
						{
							Name = Convert.ToString(dr["_Name"], CultureInfo.InvariantCulture),
							Code = Convert.ToString(dr["_Code"], CultureInfo.InvariantCulture),
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
		public string GetName() => @"select * from Master_Colors";
	}
}
