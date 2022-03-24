using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
	public class ProcGetShoppingCategoryLvl1ByIDNew : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetShoppingCategoryLvl1ByIDNew(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			ShoppingCategoryLvl1 res = new ShoppingCategoryLvl1();
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
					res.Id = Convert.ToInt32(dt.Rows[0]["_categoryID"], CultureInfo.InvariantCulture);
					res.CategoryName = dt.Rows[0]["_CategoryName"] is DBNull ? "" : dt.Rows[0]["_CategoryName"].ToString();
					res.Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
					res.mainCategoryID = Convert.ToInt32(dt.Rows[0]["_MainCategoryId"], CultureInfo.InvariantCulture);
			     	res.IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"], CultureInfo.InvariantCulture);
					res.Commission = Convert.ToInt32(dt.Rows[0]["_Commission"], CultureInfo.InvariantCulture); ;
					res.CommissionType = dt.Rows[0]["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_CommissionType"], CultureInfo.InvariantCulture); ;
					res.icon = dt.Rows[0]["_Icone"] is DBNull ? "" : dt.Rows[0]["_Icone"].ToString();
					res.IconeType = dt.Rows[0]["_IconeType"] is DBNull ? "" : dt.Rows[0]["_IconeType"].ToString();
					res.image = dt.Rows[0]["_BannerImage"] is DBNull ? "" : dt.Rows[0]["_BannerImage"].ToString();
				}
				StringBuilder sb = new StringBuilder(DOCType.ECommFEImagePath);
				sb.Append(res.image);
				//sb.Append(((byte)listItem.ImgType).ToString());

				//sb.Append("_");
				//sb.Append(listItem.ID.ToString());
				//sb.Append(".*");
				string path = sb.ToString();
				if (File.Exists(path))
				{
					res.image = path;
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
		public string GetName() => "select C.*, MC._CategoryName from tbl_ShoppingCategory C left outer join tbl_ShoppingMainCategory MC on  mc._ID=c._mainCategoryId where _CategoryID=@Id";
	}
}
