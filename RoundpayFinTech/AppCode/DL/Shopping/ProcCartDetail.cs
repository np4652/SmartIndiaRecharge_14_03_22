using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcCartDetail : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcCartDetail(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<CartDetail>();
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new CartDetail
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            UserID = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            ProductID = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductDetailID = dr["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductDetailID"]),
                            ProductCode = Convert.ToString(dr["_ProductCode"]),
                            Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Description = Convert.ToString(dr["_Description"]),
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            Quantity = Convert.ToInt32(dr["_Quantity"]),
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                        }
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
                    LoginTypeID = 1,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_CartDetail";
    }

    public class ProcCartDetailExternal : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcCartDetailExternal(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<CartDetail>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@CartDet", req.CommonStr)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new CartDetail
                        {
                            ID = 0, //dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            UserID = 0, //dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            ProductID = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductDetailID = dr["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductDetailID"]),
                            ProductCode = Convert.ToString(dr["_ProductCode"]),
                            Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Description = Convert.ToString(dr["_Description"]),
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            Quantity = Convert.ToInt32(dr["_Quantity"]),
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                        }
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
                    LoginTypeID = 1,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetCartDetailForExt";
    }
    public class ProcRecentViewExternal : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcRecentViewExternal(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<RecentViewModel>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@CartDet", req.CommonStr)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new RecentViewModel
                        {

                            ProductId = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            POSId = dr["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductDetailID"]),
                            ProductDetailID = dr["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductDetailID"]),
                            // ProductCode = Convert.ToString(dr["_ProductCode"]),
                            //  Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            //  DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            //    Description = Convert.ToString(dr["_Description"]),
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            //  RemainingQuantity = Convert.ToInt32(dr["_Quantity"]),
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            Title = dr["AdditionalTitle"] is DBNull ? "" : Convert.ToString(dr["AdditionalTitle"])
                        };
                        string Domain = req.CommonStr1;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DOCType.ProductImage);
                        sb.Append(data.ProductId);
                        string path = sb.ToString();
                        DirectoryInfo d = new DirectoryInfo(path);
                        FileInfo[] Files = d.GetFiles(data.POSId.ToString() + "_*");
                        FileInfo[] BigFiles = d.GetFiles(data.POSId.ToString() + "_*-1x*");
                        if (Files.Length > 0)
                        {
                            data.SmallImage = string.Concat(Domain, "/", path, "/", Files[0].Name);

                        }
                        if (BigFiles.Length > 0)
                        {
                            data.FrontImage = string.Concat(Domain, "/", path, "/", BigFiles[0].Name);
                        }
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
                    LoginTypeID = 1,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetRecentViewForExt";
    }



    public class ProcWishlistDetail : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcWishlistDetail(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<CartDetail>();
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new CartDetail
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            UserID = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            ProductID = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductDetailID = dr["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductDetailID"]),
                            ProductCode = Convert.ToString(dr["_ProductCode"]),
                            Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Description = Convert.ToString(dr["_Description"]),
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            Quantity = 0,//Convert.ToInt32(dr["_Quantity"]),
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                        }
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
                    LoginTypeID = 1,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_WishlistDetail";
    }

    public class ProcWishlistDetailExternal : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcWishlistDetailExternal(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<CartDetail>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@CartDet", req.CommonStr)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new CartDetail
                        {
                            ID = 0, //dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            UserID = 0, //dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            ProductID = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductDetailID = dr["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductDetailID"]),
                            ProductCode = Convert.ToString(dr["_ProductCode"]),
                            Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Description = Convert.ToString(dr["_Description"]),
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            Quantity = 0, //Convert.ToInt32(dr["_Quantity"]),
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                        }
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
                    LoginTypeID = 1,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetWishlistDetailForExt";
    }

    public class ProcUpdateECommLocalSession : IProcedure
    {
        private readonly IDAL _dal;

        public ProcUpdateECommLocalSession(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus()
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.NORESPONSE
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@CartDet", req.CommonStr),
                new SqlParameter("@WishList", req.CommonStr2)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0][1].ToString();
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
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_UpdateECommLocalSession";
    }
}
