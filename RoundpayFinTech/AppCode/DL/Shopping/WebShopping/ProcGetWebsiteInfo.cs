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
using System.Text;
using System.IO;

namespace RoundpayFinTech.AppCode.DL.Shopping.WebShopping
{
    public class ProcGetWebsiteInfo : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetWebsiteInfo(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WebsiteId", req.CommonStr)
            };
            var res = new GetAllMenu();
            var maincategoryMenus = new List<Menu>();
            var subCategoryMenus = new List<CategoryList>();
            var BannerProductsFront = new List<ProductsList>();
            var BannerProductsRight = new List<ProductsList>();
            var websiteSettings = new List<WebSite>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(true);
                if (ds.Tables.Count > 0)
                {
                    DataTable dtmaincategoryMenus = ds.Tables[0]; //Table1 maincategoryMenus
                    DataTable dtsubCategoryMenus = ds.Tables[1];//Table2 subCategoryMenus
                    DataTable dtBannerProductsFront = ds.Tables[2];//Table3 BannerProductsFront
                    DataTable dtBannerProductsRight = ds.Tables[3];//Table4 BannerProductsRight
                    DataTable dtwebsiteSettings = ds.Tables[4];//Table5 websiteSettings
                    #region 1 Table1 maincategoryMenus
                    if (dtmaincategoryMenus.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtmaincategoryMenus.Rows)
                        {
                            var data = new Menu
                            {
                                MainCategoryID = dr["MainCategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["MainCategoryID"]),
                                Name = dr["Name"] is DBNull ? "" : dr["Name"].ToString(),
                                Active = dr["Active"] is DBNull ? false : Convert.ToBoolean(dr["Active"]),
                                MainCategoryImage = dr["MainCategoryImage"] is DBNull ? "" : dr["MainCategoryImage"].ToString(),
                                icon = dr["Icon"] is DBNull ? "" : dr["Icon"].ToString(),
                                IconeType = dr["IconeType"] is DBNull ? "" : dr["IconeType"].ToString(),
                                Commission = dr["Commission"] is DBNull ? 0 : Convert.ToInt32(dr["Commission"]),
                                CommissionType = dr["CommissionType"] is DBNull ? true : Convert.ToBoolean(dr["CommissionType"])
                            };
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DOCType.ShoppingImagePath.Replace("{0}", ""));
                            string path = sb.ToString();
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles( data.MainCategoryID + "*");
                            if (Files.Length > 0)
                            {
                                data.MainCategoryImage = string.Concat(Domain, "/", path, "/", Files[0].Name ?? "");
                            }
                            maincategoryMenus.Add(data);
                        }
                        res.maincategoryMenus = maincategoryMenus;
                    }
                    #endregion
                    #region 2 Table2 subCategoryMenus
                    if (dtsubCategoryMenus.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtsubCategoryMenus.Rows)
                        {
                            var data = new CategoryList
                            {

                                ParentId = dr["ParentId"] is DBNull ? 0 : Convert.ToInt32(dr["ParentId"]),
                                CategoryId = dr["SubCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubCategoryId"]),
                                SubCategoryId = dr["SubCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubCategoryId"]),
                                //   ProductCount = dr["ProductCount"] is DBNull ? 0 : Convert.ToInt32(dr["ProductCount"]),
                                mainCategoryID = dr["mainCategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["mainCategoryID"]),
                                Name = dr["Name"] is DBNull ? "" : dr["Name"].ToString(),
                                IsActive = dr["IsActive"] is DBNull ? false : Convert.ToBoolean(dr["IsActive"]),
                                // CategoryName = dr["CategoryName"] is DBNull ? "" : dr["CategoryName"].ToString(),
                                //image = dr["image"] is DBNull ? "" : dr["image"].ToString(),
                                icon = dr["icone"] is DBNull ? "" : dr["icone"].ToString(),
                                IconeType = dr["IconeType"] is DBNull ? "" : dr["IconeType"].ToString(),
                                Commission = dr["Commission"] is DBNull ? 0 : Convert.ToInt32(dr["Commission"]),
                                CommissionType = dr["CommissionType"] is DBNull ? true : Convert.ToBoolean(dr["CommissionType"])
                            };
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DOCType.ShoppingImagePath.Replace("{0}", ""));
                            string path = sb.ToString();
                            DirectoryInfo d = new DirectoryInfo(path);
                           FileInfo[] Files = d.GetFiles("S1_"+data.CategoryId+"*");
                            if (Files.Length > 0)
                            {
                                data.image = string.Concat(Domain, "/", path, "/", Files[0].Name ?? "");
                            }
                            subCategoryMenus.Add(data);
                        }
                        res.subCategoryMenus = subCategoryMenus;
                    }
                    #endregion 
                    #region 3 Table3 BannerProductsFront
                    if (dtBannerProductsFront.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtBannerProductsFront.Rows)
                        {
                            var data = new ProductsList
                            {

                                BannerImage = dr["BannerImage"] is DBNull ? "" : dr["BannerImage"].ToString()
                                //RedirectUrl = dr["RedirectUrl"] is DBNull ? "" : dr["RedirectUrl"].ToString()

                            };
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder(Domain);
                            sb.Append("/");
                            sb.Append(DOCType.ECommFEImage);
                            sb.Append(data.BannerImage);
                            string path = sb.ToString();
                            data.BannerImage = path;
                            BannerProductsFront.Add(data);
                        }
                        res.BannerProductsFront = BannerProductsFront;
                    }
                    #endregion
                    # region 4 Table4 BannerProductsRight
                    if (dtBannerProductsRight.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtBannerProductsRight.Rows)
                        {
                            var data = new ProductsList
                            {
                                BannerImage = dr["BannerImage"] is DBNull ? "" : dr["BannerImage"].ToString()
                                //RedirectUrl = dr["RedirectUrl"] is DBNull ? "" : dr["RedirectUrl"].ToString()
                            };
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder(Domain);
                            sb.Append("/");
                            sb.Append(DOCType.ECommFEImage);
                            sb.Append(data.BannerImage);
                            string path = sb.ToString();
                            data.BannerImage = path;
                            BannerProductsRight.Add(data);
                        }
                        res.BannerProductsRight = BannerProductsRight;
                    }
                    #endregion
                    #region 5 Table5 websiteSettings
                    if (dtwebsiteSettings.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtwebsiteSettings.Rows)
                        {
                            var data = new WebSite
                            {
                                WebsiteId = dr["WebsiteId"] is DBNull ? "" : dr["WebsiteId"].ToString(),
                                WhiteLabelId = dr["WhiteLabelId"] is DBNull ? 0 : Convert.ToInt32(dr["WhiteLabelId"]),
                                IsRecharge = dr["IsRecharge"] is DBNull ? true : Convert.ToBoolean(dr["IsRecharge"]),
                                Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                                MobileNo = dr["MobileNo"] is DBNull ? "" : dr["MobileNo"].ToString(),
                                EmailId = dr["EmailId"] is DBNull ? "" : dr["EmailId"].ToString(),
                                Address = dr["Address"] is DBNull ? "" : dr["Address"].ToString(),
                                BrandName = dr["BrandName"] is DBNull ? "" : dr["BrandName"].ToString()
                            };
                            websiteSettings.Add(data);
                        }
                        res.websiteSettings = websiteSettings;
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetWebsiteInfoforShopping";
    }
}
