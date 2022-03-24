using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetShoppingSubCategoryLvl2ByID : IProcedure
	{
		private readonly IDAL _dal;
		public ProcGetShoppingSubCategoryLvl2ByID(IDAL dal) => _dal = dal;
		public object Call(object obj)
		{
			var res = new ShoppingSubCategoryLvl2();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LT", req.LoginTypeID),
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@Id", req.CommonInt)
			};
			try
			{
				DataTable dt = _dal.Get(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					res.SubCategoryID = Convert.ToInt32(dt.Rows[0]["_subcategoryID1"], CultureInfo.InvariantCulture);
					res.SubCategoryName = dt.Rows[0]["_SubCategoryName"] is DBNull ? "" : dt.Rows[0]["_SubCategoryName"].ToString();
					res.SubCategoryIDLvl2 = Convert.ToInt32(dt.Rows[0]["_ID"], CultureInfo.InvariantCulture);
					res.SubCategoryNameLvl2 = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
					res.IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"], CultureInfo.InvariantCulture);
					res.IsNextLevelExists = dt.Rows[0]["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsNextLevel"], CultureInfo.InvariantCulture);
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
		public string GetName() => @"select sb1._ID _subcategoryID1,sb1._Name _SubCategoryName,sb.* from [Master_SubCategoryLvl_2] sb,[Master_SubCategoryLvl_1] sb1
									 where sb._SubcategoryID=sb1._ID and sb._ID= @Id";
	}
}
