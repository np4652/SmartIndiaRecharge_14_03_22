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
    public class ProcGetShoppingSubCategoryLvl1ByID : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetShoppingSubCategoryLvl1ByID(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			ShoppingSubCategoryLvl1 res = new ShoppingSubCategoryLvl1();
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
					res.CategoryID = Convert.ToInt32(dt.Rows[0]["_categoryID"], CultureInfo.InvariantCulture);
					res.CategoryName = dt.Rows[0]["_CategoryName"] is DBNull ? "" : dt.Rows[0]["_CategoryName"].ToString();
					res.SubCategoryID = Convert.ToInt32(dt.Rows[0]["_ID"], CultureInfo.InvariantCulture);
					res.SubCategoryName = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
					res.IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"],CultureInfo.InvariantCulture);
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
		public string GetName() => "select s._ID _categoryID,s._CategoryName,sb.* from [Master_SubCategoryLvl_1] sb,Master_ShoppingCategory s where s._ID=sb._CategoryID and sb._ID=@Id";
	}
}
