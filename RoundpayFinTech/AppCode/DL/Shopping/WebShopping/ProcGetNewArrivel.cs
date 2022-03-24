using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Fintech.AppCode.StaticModel;
using System.IO;
using System.Text;


namespace RoundpayFinTech.AppCode.DL.Shopping.WebShopping
{
    public class ProcGetNewArrivel : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetNewArrivel(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var newArrivals = new List<NewArrivalList>();
            var onSale = new List<NewArrivalList>();
            var bestSellerList = new List<NewArrivalList>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WebsiteId", req.CommonStr)

            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(true);
                if (ds.Tables.Count > 0)
                {
                    DataTable dtnewArrivals = ds.Tables[0]; //Table1 maincategoryMenus
                    DataTable dtonSale = ds.Tables[1];//Table2 subCategoryMenus
                    DataTable dtbestSellerList = ds.Tables[2];//Table3 BannerProductsFront
                    #region 1 Table1 newArrivals
                    if (dtnewArrivals.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtnewArrivals.Rows)
                        {
                            var data = new NewArrivalList
                            {
                                SubCategoryId = dr["SubCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubCategoryId"]),
                                SubCategoryName = dr["SubCategoryName"] is DBNull ? "" : dr["SubCategoryName"].ToString(),
                                ProductName = dr["ProductName"] is DBNull ? "" : dr["ProductName"].ToString(),
                                POSId = dr["POSId"] is DBNull ? 0 : Convert.ToInt32(dr["POSId"]),
                                AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : dr["AdditionalTitle"].ToString(),
                                Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                                IsCartAdded = dr["IsCartAdded"] is DBNull ? 0 : Convert.ToInt32(dr["IsCartAdded"]),
                                MRP = dr["MRP"] is DBNull ? 0 : Convert.ToInt32(dr["MRP"]),
                                SellingPrice = dr["SellingPrice"] is DBNull ? 0 : Convert.ToInt32(dr["SellingPrice"]),
                                Discount = dr["Discount"] is DBNull ? 0 : Convert.ToInt32(dr["Discount"]),
                                AffiliateShareLink = req.CommonStr2,
                                ShareLink = req.CommonStr2,
                                RemainingQuantity = dr["remainingQuantity"] is DBNull ? 0 : Convert.ToInt32(dr["remainingQuantity"]),
                                FrontImage = dr["FrontImage"] is DBNull ? "" : dr["FrontImage"].ToString(),
                                SmallImage = dr["SmallImage"] is DBNull ? "" : dr["SmallImage"].ToString()
                            };
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DOCType.ProductImage);
                            sb.Append(data.FrontImage);
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

                            newArrivals.Add(data);
                        }
                    }
                    #endregion
                    #region 2 Table2 onSale
                    if (dtonSale.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtonSale.Rows)
                        {
                            var data = new NewArrivalList
                            {
                                SubCategoryId = dr["SubCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubCategoryId"]),
                                SubCategoryName = dr["SubCategoryName"] is DBNull ? "" : dr["SubCategoryName"].ToString(),
                                ProductName = dr["ProductName"] is DBNull ? "" : dr["ProductName"].ToString(),
                                POSId = dr["POSId"] is DBNull ? 0 : Convert.ToInt32(dr["POSId"]),
                                AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : dr["AdditionalTitle"].ToString(),
                                Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                                IsCartAdded = dr["IsCartAdded"] is DBNull ? 0 : Convert.ToInt32(dr["IsCartAdded"]),
                                MRP = dr["MRP"] is DBNull ? 0 : Convert.ToInt32(dr["MRP"]),
                                SellingPrice = dr["SellingPrice"] is DBNull ? 0 : Convert.ToInt32(dr["SellingPrice"]),
                                Discount = dr["Discount"] is DBNull ? 0 : Convert.ToInt32(dr["Discount"]),
                                AffiliateShareLink = req.CommonStr2,
                                ShareLink = req.CommonStr2,
                                RemainingQuantity = dr["remainingQuantity"] is DBNull ? 0 : Convert.ToInt32(dr["remainingQuantity"]),
                                FrontImage = dr["FrontImage"] is DBNull ? "" : dr["FrontImage"].ToString(),
                                SmallImage = dr["SmallImage"] is DBNull ? "" : dr["SmallImage"].ToString()
                            };
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DOCType.ProductImage);
                            sb.Append(data.FrontImage);
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

                            onSale.Add(data);
                        }
                    }
                    #endregion
                    #region 3 Table3 bestSellerList
                    if (dtbestSellerList.Rows.Count > 0)
                    {
                       foreach (DataRow dr in dtbestSellerList.Rows)
                        {
                            var data = new NewArrivalList
                            {
                                SubCategoryId = dr["SubCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubCategoryId"]),
                                SubCategoryName = dr["SubCategoryName"] is DBNull ? "" : dr["SubCategoryName"].ToString(),
                                ProductName = dr["ProductName"] is DBNull ? "" : dr["ProductName"].ToString(),
                                POSId = dr["POSId"] is DBNull ? 0 : Convert.ToInt32(dr["POSId"]),
                                AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : dr["AdditionalTitle"].ToString(),
                                Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                                IsCartAdded = dr["IsCartAdded"] is DBNull ? 0 : Convert.ToInt32(dr["IsCartAdded"]),
                                MRP = dr["MRP"] is DBNull ? 0 : Convert.ToInt32(dr["MRP"]),
                                SellingPrice = dr["SellingPrice"] is DBNull ? 0 : Convert.ToInt32(dr["SellingPrice"]),
                                Discount = dr["Discount"] is DBNull ? 0 : Convert.ToInt32(dr["Discount"]),
                                AffiliateShareLink = req.CommonStr2,
                                ShareLink = req.CommonStr2,
                                RemainingQuantity = dr["remainingQuantity"] is DBNull ? 0 : Convert.ToInt32(dr["remainingQuantity"]),
                                FrontImage = dr["FrontImage"] is DBNull ? "" : dr["FrontImage"].ToString(),
                                SmallImage = dr["SmallImage"] is DBNull ? "" : dr["SmallImage"].ToString()
                            };
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DOCType.ProductImage);
                            sb.Append(data.FrontImage);
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

                            bestSellerList.Add(data);
                        }
                    }
                    #endregion
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
            return new NewArrivalOnSaleProducts
            {
                NewArrivals = newArrivals ?? new List<NewArrivalList>(),
                OnSale = onSale ?? new List<NewArrivalList>(),
                BestSellerList = bestSellerList ?? new List<NewArrivalList>()
            };
        }

        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_NewArrivel";
    }
}
