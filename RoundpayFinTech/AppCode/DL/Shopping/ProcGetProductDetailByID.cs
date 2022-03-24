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
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetProductDetailByID : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetProductDetailByID(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            var response = new AddProductModal();
            var ProductDetail = new ProductDetail();
            var ProductFilterDetail = new List<ProductFilterDetail>();

            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ProductDetailID",req.CommonInt)
            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        ProductDetail.ProductDetailID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        ProductDetail.ProductID = Convert.ToInt32(dt.Rows[0]["_ProductID"]);
                        ProductDetail.CategoryID = Convert.ToInt32(dt.Rows[0]["_CategoryID"]);
                        ProductDetail.SubCategoryID1 = Convert.ToInt32(dt.Rows[0]["_SubCategoryID1"]);
                        ProductDetail.SubCategoryID2 = Convert.ToInt32(dt.Rows[0]["_SubCategoryID2"]);

                        ProductDetail.ProductCode = Convert.ToString(dt.Rows[0]["_ProductCode"]);
                        ProductDetail.Batch = Convert.ToString(dt.Rows[0]["_Batch"]);
                        ProductDetail.BrandID = Convert.ToInt32(dt.Rows[0]["_BrandID"]);
                        ProductDetail.VendorID = Convert.ToInt32(dt.Rows[0]["_VendorID"]);
                        ProductDetail.MRP = Convert.ToInt32(dt.Rows[0]["_MRP"]);
                        ProductDetail.Discount = Convert.ToInt32(dt.Rows[0]["_Discount"]);
                        ProductDetail.DiscountType = Convert.ToBoolean(dt.Rows[0]["_DiscountType"]);
                        ProductDetail.Commission = Convert.ToInt32(dt.Rows[0]["_Commission"]);
                        ProductDetail.CommissionType = Convert.ToBoolean(dt.Rows[0]["_CommissionType"]);
                        ProductDetail.DiscountCount = Convert.ToInt32(dt.Rows[0]["_DiscountCount"]);
                        ProductDetail.shippingDiscount = Convert.ToInt32(dt.Rows[0]["_shippingDiscount"]);
                        ProductDetail.Description = Convert.ToString(dt.Rows[0]["_Description"]);
                        ProductDetail.Specification = Convert.ToString(dt.Rows[0]["_Specification"]);
                        ProductDetail.ShippingMode = Convert.ToInt32(dt.Rows[0]["_ShippingMode"]);
                        ProductDetail.ShippingCharges = Convert.ToInt32(dt.Rows[0]["_ShippingCharges"]);
                        ProductDetail.WeightInKG = Convert.ToDecimal(dt.Rows[0]["_Weight_InKG"]);
                        ProductDetail.ReturnApplicable = dt.Rows[0]["_ReturnApplicable"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ReturnApplicable"]);
                        ProductDetail.IsTrending = dt.Rows[0]["_IsTrending"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsTrending"]);
                        ProductDetail.SellingPrice = dt.Rows[0]["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_SellingPrice"]);
                        ProductDetail.AdminProfit = dt.Rows[0]["_AdminCommission"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_AdminCommission"]);
                        ProductDetail.VendorPrice = dt.Rows[0]["_VendorPrice"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_VendorPrice"]);
                        ProductDetail.B2BDiscount = dt.Rows[0]["_B2BDiscount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BDiscount"]);
                        ProductDetail.B2BDiscountType = dt.Rows[0]["_B2BDiscountType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_B2BDiscountType"]);
                        ProductDetail.B2BSellingPrice = dt.Rows[0]["_B2BSellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BSellingPrice"]);
                        ProductDetail.B2BCommission = dt.Rows[0]["_B2BCommission"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BCommission"]);
                        ProductDetail.B2BCommissionType = dt.Rows[0]["_B2BCommissionType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_B2BCommissionType"]);
                        ProductDetail.B2BAdminProfit = dt.Rows[0]["_B2BAdminCommission"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BAdminCommission"]);
                        ProductDetail.B2BVendorPrice = dt.Rows[0]["_B2BVendorPrice"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BVendorPrice"]);
                        ProductDetail.B2BShippingMode = dt.Rows[0]["_B2BShippingMode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_B2BShippingMode"]);
                        ProductDetail.B2BShippingCharges = dt.Rows[0]["_B2BShippingCharges"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BShippingCharges"]);
                        ProductDetail.B2BDiscountCount = dt.Rows[0]["_B2BDiscountCount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BDiscountCount"]);
                        ProductDetail.B2BshippingDiscount = dt.Rows[0]["_B2BShippingDiscount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_B2BShippingDiscount"]);
                        ProductDetail.AdditionalTitle = dt.Rows[0]["_AdditionalTitle"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_AdditionalTitle"]);
                    }
                    response.ProductDetail = ProductDetail;


                    dt = ds.Tables[1];
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            ProductFilterDetail.Add(new ProductFilterDetail
                            {
                                FilterID = Convert.ToInt32(dr["_FilterID"]),
                                FilterOptionID = Convert.ToInt32(dr["_FilterOptionID"]),
                            });
                        }
                    }
                    response.FilterDetail = ProductFilterDetail;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetProductDetailByID";
    }
}