using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCategoryForUserIndex : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetCategoryForUserIndex(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (ProductFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.UserId),
                new SqlParameter("@Lt", req.LoginTypeId),
                new SqlParameter("@CategoryID", req.CategoryID),
                new SqlParameter("@SubCategoryID1", req.SubCategoryID1),
                new SqlParameter("@SubCategoryID2", req.SubCategoryID2),
            };
            var res = new List<CategoriesForIndex>();
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt = ds.Tables[0];
                    DataTable dtS = new DataTable();
                    dtS = ds.Tables[1];

                    DataTable dtOfferImg = new DataTable();
                    dtOfferImg = ds.Tables[2];

                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new CategoriesForIndex
                        {
                            MainCatId = Convert.ToInt32(dr["_ID"]),
                            MainCatName = Convert.ToString(dr["_CategoryName"]),
                            SubCategories = new List<ShoppingSubCategoryLvl2>(),
                            OfferImgPath = new List<string>()
                        };
                        var dataSubCatDef = new ShoppingSubCategoryLvl2
                        {
                            SubCategoryIDLvl2 = 0,
                            SubCategoryNameLvl2 = "All"
                        };
                        data.SubCategories.Add(dataSubCatDef);

                        foreach (DataRow drS in dtS.Select("_CategoryID = " + data.MainCatId.ToString()))
                        {
                            var dataSubCat = new ShoppingSubCategoryLvl2
                            {
                                SubCategoryIDLvl2 = Convert.ToInt32(drS["_SubcategoryID2"]),
                                SubCategoryNameLvl2 = Convert.ToString(drS["_Name"])
                            };
                            data.SubCategories.Add(dataSubCat);
                        }
                        var pathList = new List<string>();
                        foreach (DataRow dtI in dtOfferImg.Select("_CategoryID = " + data.MainCatId.ToString()))
                        {
                            data.OfferImgPath.Add(Convert.ToString(dtI["_FileName"]));
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetCategoriesForUserIndex";
    }
}
