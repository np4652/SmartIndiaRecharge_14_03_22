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
	public class ProcGetShoppingCategoryByID : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetShoppingCategoryByID(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			ShoppingCategory res = new ShoppingCategory();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LT", req.LoginTypeID),
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@CategoryID", req.CommonInt)
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0 )
				{
					res.CategoryID = Convert.ToInt32(dt.Rows[0]["_ID"], CultureInfo.InvariantCulture);
					res.CategoryName = dt.Rows[0]["_CategoryName"] is DBNull ? "" : dt.Rows[0]["_CategoryName"].ToString();
					res.IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]);
					res.IsNextLevelExists = dt.Rows[0]["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsNextLevel"]);
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
		public string GetName() => "proc_GetShoppingCategoryByID";
	}
}
