using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetProductDetail : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetProductDetail(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<ProductDetail>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@ProductID", req.CommonInt),
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ProductDetail
                        {
                            ProductDetailID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            ProductID = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductCode = Convert.ToString(dr["_ProductCode"]),                            
                            VendorID = dr["_VendorID"] is DBNull ? 0 : Convert.ToInt32(dr["_VendorID"]),
                            VendorName = Convert.ToString(dr["_VendorName"]),
                            BrandID = dr["_BrandID"] is DBNull ? 0 : Convert.ToInt32(dr["_BrandID"]),
                            BrandName= Convert.ToString(dr["_BrandName"]),
                            Quantity = dr["_Quantity"] is DBNull ? 0 : Convert.ToInt32(dr["_Quantity"]),
                            Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommissionType = dr["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_Commission"]),
                            Description = Convert.ToString(dr["_Description"]),
                            IsDeleted = dr["_IsDeleted"] is DBNull ? false : Convert.ToBoolean(dr["_IsDeleted"]),
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
        //public string GetName() => @"select  SUM(Case(_TrType) when 1 then _Qty when 0 then _Qty * -1 end) _Quantity ,s._VariantId into #tempStock
        //                            from tbl_ProductStockDetails s(nolock) inner join tbl_ProductDetail p(nolock) on s._VariantId=p._ID  
        //                            where p._ProductID=@ProductID group by _VariantId
        //                            select p.*,v._VendorName,b._BrandName,s._Quantity 
        //                            from tbl_ProductDetail p inner join tbl_Vendors v on v._ID=p._VendorID 
        //                            	 Inner Join Master_Brand b on b._ID=p._BrandID
        //                            	 Inner Join #tempStock s on s._VariantId=p._id
        //                            where p._ProductID=@ProductID and p._EntryBy=@LoginID";

        public string GetName() => @"select  SUM(Case(_TrType) when 1 then _Qty when 0 then _Qty * -1 end) _Quantity ,s._VariantId into #tempStock
                                    from tbl_ProductStockDetails s(nolock) inner join tbl_ProductDetail p(nolock) on s._VariantId=p._ID  
                                    where p._ProductID=@ProductID group by _VariantId
                                    select p.*,v._VendorName,b._BrandName,s._Quantity 
                                    from tbl_ProductDetail p inner join tbl_Vendors v on v._ID=p._VendorID 
                                    	 Inner Join Master_Brand b on b._ID=p._BrandID
                                    	 Inner Join #tempStock s on s._VariantId=p._id
                                    where p._ProductID=@ProductID and (p._EntryBy=@LoginID or @LoginID=1)";
    }
}
