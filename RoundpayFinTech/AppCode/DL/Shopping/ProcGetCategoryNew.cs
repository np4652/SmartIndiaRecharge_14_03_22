using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;


namespace RoundpayFinTech.AppCode.DL.Shopping
{
   
	public class ProcGetCategoryNew : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetCategoryNew(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<ShoppingSubCategoryLvl1>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@CategoryId", req.CommonInt)
			};
			try
			{
				string a = GetName();
				DataTable dt = _dal.Get(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var data = new ShoppingSubCategoryLvl1
						{
							CategoryID = Convert.ToInt32(dr["_categoryID"], CultureInfo.InvariantCulture),
							CategoryName = dr["_CategoryName"] is DBNull ? "" : dr["_CategoryName"].ToString(),
							SubCategoryID = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
							SubCategoryName = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
							IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
							IsNextLevelExists = dr["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dr["_IsNextLevel"]),
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
		public string GetName() => "select s._ID _categoryID,s._CategoryName,sb.* from tbl_ShoppingCategory sb,tbl_shoppingMainCategory s where s._ID=sb._MainCategoryId and ((@LoginID<>1 and sb._IsActive=1) or @LoginID=1) and (sb._CategoryId=@CategoryId or @CategoryId=0)";
	}

	public class ProcGetCategoryByIdNew : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetCategoryByIdNew(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<CategoryList>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@loginID", req.LoginID),
				new SqlParameter("@lt", req.LoginTypeID),
				new SqlParameter("@catId", req.CommonInt),
				new SqlParameter("@Id", req.CommonInt2)
			};
			try
			{
				string a = GetName();
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var data = new CategoryList
						{
							mainCategoryID = Convert.ToInt32(dr["_MainCategoryId"], CultureInfo.InvariantCulture),
							CategoryName = dr["_CategoryName"] is DBNull ? "" : dr["_CategoryName"].ToString(),
							SubCategoryId = Convert.ToInt32(dr["_CategoryID"], CultureInfo.InvariantCulture),
							Name = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
							IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
							//IsNextLevelExists = dr["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dr["_IsNextLevel"]),
							ProductCount = dr["ProductCount"] is DBNull ? 0 : Convert.ToInt32(dr["ProductCount"], CultureInfo.InvariantCulture),
							image = "Image/icon/Shopping/S1_" + Convert.ToString(dr["_BannerImage"], CultureInfo.InvariantCulture) + ".png"
						};
						StringBuilder builder = new StringBuilder();
						builder.Append(DOCType.ShoppingImagePath.Replace("{0}", "S1_"));
						builder.Append(Convert.ToString(dr["_CategoryID"], CultureInfo.InvariantCulture));
						builder.Append(".png");
						data.image = (File.Exists(builder.ToString())) ? builder.ToString() : null;
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
		public string GetName() => "proc_GetCategoryNew";
	}
}
