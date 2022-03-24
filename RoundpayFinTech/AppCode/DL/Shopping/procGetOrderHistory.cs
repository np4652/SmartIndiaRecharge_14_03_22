using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class procGetOrderHistory : IProcedure
    {
        private readonly IDAL _dal;

		public procGetOrderHistory(IDAL dal) => _dal = dal;       





		public object Call(object obj)
		{
			var res = new List<OrderList>();
			var req = (OrderModel)obj;
			SqlParameter[] param = {
				new SqlParameter("@LoginId", req.LoginID),
				new SqlParameter("@Mode", req.OrderMode),
				new SqlParameter("@Top", req.TopRows),
				new SqlParameter("@MobileNo", req.MobileNo),
				new SqlParameter("@StatusID", req.StatusID)
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						StringBuilder sb = new StringBuilder();
						sb.Append("{Address} ,State:{State} ,City:{City}, PIN:{PIN} ");
						sb.Replace("{Address}", Convert.ToString(dr["DeliveryAddress"]));
						sb.Replace("{State}", Convert.ToString(dr["State"]));
						sb.Replace("{City}", Convert.ToString(dr["City"]));
						sb.Replace("{PIN}", Convert.ToString(dr["PIN"]));
						var data = new OrderList
						{
							OrderId = Convert.ToInt32(dr["OrderId"]),
							//ProductName = Convert.ToString(dr["ProductName"]),
							Quantity = Convert.ToInt32(dr["Quantity"]),
							CustomerName = Convert.ToString(dr["CustomerName"]),
							MobileNo = Convert.ToString(dr["MobileNo"]),
							DeliveryAddress = sb.ToString(),
							TotalCost = dr["TotalCost"] is DBNull ? 0 :  Convert.ToDecimal(dr["TotalCost"]),
							TotalShipping = dr["_TotalShipping"] is DBNull?0: Convert.ToDecimal(dr["_TotalShipping"]),
							Status = dr["Status"] is DBNull ? 1 : Convert.ToInt32(dr["Status"]),
							RetailerName=Convert.ToString(dr["_RetailerName"]),
							OutletName=Convert.ToString(dr["_outletName"]),
							RetailerMobile = Convert.ToString(dr["_RetailerMobile"]),							
							Opening = dr["Opening"] is DBNull?0: Convert.ToDecimal(dr["Opening"]),							
							Deduction = dr["Deduction"] is DBNull ? 0 : Convert.ToDecimal(dr["Deduction"]),							
							Closing = dr["Closing"] is DBNull ? 0 : Convert.ToDecimal(dr["Closing"]),							
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
					LoginTypeID = req.LT,
					UserId = req.LoginID
				};
				var _ = new ProcPageErrorLog(_dal).Call(errorLog);
			}
			return res;
		}

		public object Call() => throw new NotImplementedException();
		public string GetName() => "proc_getOrderHistory";
	}
}
