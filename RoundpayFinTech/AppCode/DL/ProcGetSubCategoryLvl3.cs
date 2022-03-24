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
    public class ProcGetSubCategoryLvl3 : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetSubCategoryLvl3(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<ShoppingSubCategoryLvl3>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@SubCategoryId", req.CommonInt)
			};
			try
			{
				DataTable dt = _dal.Get(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var data = new ShoppingSubCategoryLvl3
						{
							SubCategoryIDLvl2 = Convert.ToInt32(dr["SubCategoryIDLvl2"], CultureInfo.InvariantCulture),
							SubCategoryNameLvl2 = Convert.ToString(dr["SubCategoryNameLvl2"],CultureInfo.InvariantCulture),
							SubCategoryIDLvl3 = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
							SubCategoryNameLvl3 = Convert.ToString(dr["_Name"],CultureInfo.InvariantCulture),
							GroupID = Convert.ToInt32(dr["_GroupID"], CultureInfo.InvariantCulture),
							IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"], CultureInfo.InvariantCulture)
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

		public object Call() => throw new NotImplementedException();
		public string GetName() => @"select sb1._ID SubCategoryIDLvl2,sb1._Name SubCategoryNameLvl2,sb.*
									 from Master_SubCategoryLvl_3 sb,Master_SubCategoryLvl_2 sb1
									 where sb._SubCategoryID=sb1._ID And(sb1._ID = @SubCategoryId or @SubCategoryId = 0)";
	}
}
