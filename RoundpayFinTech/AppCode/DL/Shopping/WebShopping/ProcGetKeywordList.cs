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
    public class ProcGetKeywordList : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetKeywordList(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<KeywordList>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WebsiteId", req.CommonStr2),
                new SqlParameter("@SearchKeyword", req.CommonStr),
                new SqlParameter("@Event", "K")
            };
            int id = 1;
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new KeywordList
                        {
                            KeywordId = dr["KeywordId"] is DBNull ? 0 : Convert.ToInt32(dr["KeywordId"]),
                            Keyword = dr["Keyword"] is DBNull ? "" : dr["Keyword"].ToString(),
                            SubcategoryName = dr["SubcategoryName"] is DBNull ? "" : dr["SubcategoryName"].ToString(),
                            Image = dr["Image"] is DBNull ? "" : dr["Image"].ToString(),
                            SubcategoryId = dr["SubCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubCategoryId"]),
                            ProductDetailId = dr["ProductDetailId"] is DBNull ? 0 : Convert.ToInt32(dr["ProductDetailId"]),
                            ProductId = dr["ProductId"] is DBNull ? 0 : Convert.ToInt32(dr["ProductId"])

                        };
                        string Domain = req.CommonStr1;
                        StringBuilder sb = new StringBuilder(Domain);
                        sb.Append("/");
                        sb.Append(DOCType.ShoppingImagePath.Replace("{0}", ""));
                        sb.Append(data.Image);
                        string path = sb.ToString();
                        data.Image = path;
                        string Domain1 = req.CommonStr1;
                        StringBuilder sb1 = new StringBuilder();
                        if (data.ProductDetailId > 0)
                        {
                            sb1.Append(DOCType.ProductImage);
                            sb1.Append(data.ProductId);
                            string pathProduct = sb1.ToString();
                            DirectoryInfo d = new DirectoryInfo(pathProduct);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailId + "_*_*");
                            if (Files.Length > 0)
                            {
                                data.ProductImage = string.Concat(Domain1, "/", pathProduct, "/", Files[0].Name ?? "");
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetKeywordList";
    }
}
