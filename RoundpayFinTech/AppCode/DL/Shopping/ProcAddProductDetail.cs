using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAddProductDetail : IProcedure
	{
		private readonly IDAL _dal;

		public ProcAddProductDetail(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			var req = (ProductDetail)obj;
			SqlParameter[] param = {
				new SqlParameter("@LT", req.LoginTypeID),
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@ProductID", req.ProductID),
				new SqlParameter("@ProductDetailID", req.ProductDetailID),
				new SqlParameter("@VendorID", req.VendorID),
				new SqlParameter("@BrandID", req.BrandID),
				new SqlParameter("@ProductCode", req.ProductCode??""),
				new SqlParameter("@Batch", req.Batch??""),
				new SqlParameter("@MRP", req.MRP),
				new SqlParameter("@Discount", req.Discount),
				new SqlParameter("@DiscountType", req.DiscountType),
				new SqlParameter("@SellingPrice", req.SellingPrice),
				new SqlParameter("@VendorPrice", req.VendorPrice),
				new SqlParameter("@AdminProfit", req.AdminProfit),
				new SqlParameter("@Quantity", req.Quantity),
				new SqlParameter("@Description", req.Description??""),
				new SqlParameter("@Specification", req.Specification??""),
				new SqlParameter("@filterDetail", req.FilterDetail),
				new SqlParameter("@Commission", req.Commission),
				new SqlParameter("@CommissionType", req.CommissionType),
				new SqlParameter("@ShippingMode", req.ShippingMode),
				new SqlParameter("@ShippingCharges", req.ShippingCharges),
				new SqlParameter("@DiscountCount", req.DiscountCount),
				new SqlParameter("@shippingDiscount", req.shippingDiscount),
				new SqlParameter("@Weight", req.WeightInKG),
				new SqlParameter("@ReturnApplicable", req.ReturnApplicable),
				new SqlParameter("@IsTrending", req.IsTrending),
				new SqlParameter("@B2BDiscount", req.B2BDiscount),
				new SqlParameter("@B2BDiscountType", req.B2BDiscountType),
				new SqlParameter("@B2BSellingPrice", req.B2BSellingPrice),
				new SqlParameter("@B2BVendorPrice", req.B2BVendorPrice),
				new SqlParameter("@B2BAdminProfit", req.B2BAdminProfit),
				new SqlParameter("@B2BCommission", req.B2BCommission),
				new SqlParameter("@B2BCommissionType", req.B2BCommissionType),
				new SqlParameter("@B2BShippingMode", req.B2BShippingMode),
				new SqlParameter("@B2BShippingCharges", req.B2BShippingCharges),
				new SqlParameter("@B2BDiscountCount", req.B2BDiscountCount),
				new SqlParameter("@B2BshippingDiscount", req.B2BshippingDiscount),
				new SqlParameter("@AdditionalTitle", req.AdditionalTitle ?? "")
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					res.Statuscode = Convert.ToInt32(dt.Rows[0][0], CultureInfo.InvariantCulture);
					res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
					if(res.Statuscode==ErrorCodes.One)
						res.CommonInt = Convert.ToInt32(dt.Rows[0]["ProductDetailID"], CultureInfo.InvariantCulture);
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
		public string GetName() => "proc_AddProductDetail";
	}
}
