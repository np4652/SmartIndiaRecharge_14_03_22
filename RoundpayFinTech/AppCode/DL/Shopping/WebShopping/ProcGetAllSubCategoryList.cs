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
    public class ProcGetAllSubCategoryList : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetAllSubCategoryList(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<SubCategoryProcResp>();
            // var MainCategoriesProduct = new List<MainCategoriesProduct>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@CategoryID", req.CommonInt)
            };
            int id = 1;
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new SubCategoryProcResp
                        {

                             SubCategoryId = dr["subCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["subCategoryId"]),
                            SubcategoryName= dr["subcategoryName"] is DBNull ? "" : dr["subcategoryName"].ToString(),
                            Image =dr["image"] is DBNull ? "" : dr["image"].ToString(),
                            MainCategoryName =dr["MainCategoryName"] is DBNull ? "" : dr["MainCategoryName"].ToString(),
                            MainCategoryID = dr["MainCategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["MainCategoryID"]),
                            CategoryName= dr["CategoryName"] is DBNull ? "" : dr["CategoryName"].ToString(),
                            CategoryID = dr["CategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["CategoryID"])
                        };
                        string Domain = req.CommonStr;
                        StringBuilder sb = new StringBuilder(Domain);
                        sb.Append("/");
                        sb.Append(DOCType.ShoppingImagePath.Replace("{0}", ""));
                        sb.Append(data.Image);
                        string path = sb.ToString();
                        data.Image = path;
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
        public string GetName() => "proc_getAllSubCategoryList";
    }
}
