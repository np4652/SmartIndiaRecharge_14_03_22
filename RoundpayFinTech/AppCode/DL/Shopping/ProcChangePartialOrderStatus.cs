using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcChangePartialOrderStatus : IProcedure
	{
		private readonly IDAL _dal;

		public ProcChangePartialOrderStatus(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new OrderStatusResp
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			var req = (ChangeOrderStatus)obj;
			SqlParameter[] param = {
				new SqlParameter("@LT", req.LT),
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@OrderDetailID", req.OrderDetailID),
				new SqlParameter("@Status", req.StatusID)
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					res.Statuscode = Convert.ToInt32(dt.Rows[0][0], CultureInfo.InvariantCulture);
					res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
					if (req.StatusID == (int)OrderStatus.DeliveryAssigned && res.Status == ErrorCodes.One)
					{
						res.deliveryAlerts = new List<DeliveryAlert>();
						foreach (DataRow dr in dt.Rows)
						{
							DeliveryAlert da = new DeliveryAlert
							{
								DPId = Convert.ToInt32(dr["_DPId"], CultureInfo.InvariantCulture),
								Token = Convert.ToString(dr["_Token"], CultureInfo.InvariantCulture),
								OrderDetailId = Convert.ToInt32(dr["OrderDetailID"], CultureInfo.InvariantCulture),
								VendorLat = Convert.ToString(dr["VendorLat"], CultureInfo.InvariantCulture),
								VendorLong = Convert.ToString(dr["VendorLong"], CultureInfo.InvariantCulture)
							};
							res.deliveryAlerts.Add(da);
						}
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
		public string GetName() => "Proc_ChangePartialOrderStatus";
	}
}
