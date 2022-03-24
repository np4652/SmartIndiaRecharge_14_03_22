using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;


namespace RoundpayFinTech.AppCode.DL.Shopping
{
	public class ProcGetShoppingCategoryByIDNew : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetShoppingCategoryByIDNew(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			Menu res = new Menu();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LT", req.LoginTypeID),
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@CategoryID", req.CommonInt)
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					res.MainCategoryID = Convert.ToInt32(dt.Rows[0]["MainCategoryID"], CultureInfo.InvariantCulture);
					res.Name = dt.Rows[0]["Name"] is DBNull ? "" : dt.Rows[0]["Name"].ToString();
					res.Active = dt.Rows[0]["Active"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["Active"]);
					res.CommissionType = dt.Rows[0]["CommissionType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["CommissionType"]);
					res.MainCategoryImage = dt.Rows[0]["MainCategoryImage"] is DBNull ? "" : dt.Rows[0]["MainCategoryImage"].ToString();
					res.icon = dt.Rows[0]["Icone"] is DBNull ? "" : dt.Rows[0]["Icone"].ToString();
					res.Commission= Convert.ToInt32(dt.Rows[0]["Commission"], CultureInfo.InvariantCulture);
					res.IconeType= dt.Rows[0]["IconeType"] is DBNull ? "" : dt.Rows[0]["IconeType"].ToString().Trim();
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
		public string GetName() => "proc_GetShoppingCategoryByIDNew";
	}
}
