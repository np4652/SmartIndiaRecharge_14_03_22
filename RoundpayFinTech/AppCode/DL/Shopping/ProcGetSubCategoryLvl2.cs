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
using System.Globalization;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSubCategoryLvl2 : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetSubCategoryLvl2(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<ShoppingSubCategoryLvl2>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@SubCategoryId", req.CommonInt)
            };
           
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                       
                        var data = new ShoppingSubCategoryLvl2
                        {
                            CategoryID = Convert.ToInt32(dr["_categoryID"], CultureInfo.InvariantCulture),
                            CategoryName = dr["_CategoryName"] is DBNull ? "" : dr["_CategoryName"].ToString(),
                            SubCategoryID = Convert.ToInt32(dr["_subcategoryID1"], CultureInfo.InvariantCulture),
                            SubCategoryName = dr["_SubCategoryName1"] is DBNull ? "" : dr["_SubCategoryName1"].ToString(),
                            SubCategoryIDLvl2 = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
                            SubCategoryNameLvl2 = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"], CultureInfo.InvariantCulture),
                            IsNextLevelExists = dr["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dr["_IsNextLevel"], CultureInfo.InvariantCulture),
                            Commission = dr["_MinimumCommission"] is DBNull ? 0 : Convert.ToDecimal(dr["_MinimumCommission"], CultureInfo.InvariantCulture),
                            CommissionType = dr["_CommType"] is DBNull ? false : Convert.ToBoolean(dr["_CommType"], CultureInfo.InvariantCulture)
                        };
                        StringBuilder builder = new StringBuilder();
                        builder.Append(DOCType.ShoppingImagePath.Replace("{0}", "S2_"));
                        builder.Append(Convert.ToString(dr["_ID"], CultureInfo.InvariantCulture));
                        builder.Append(".png");
                        data.ImagePath = (File.Exists(builder.ToString())) ? builder.ToString() : null;
                        res.Add(data);
                    };
                    
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
public string GetName() => @"select s._ID _categoryID,s._CategoryName,sb1._ID _subcategoryID1,sb1._Name _SubCategoryName1,sb.*
									 from Master_SubCategoryLvl_2 sb 
									 	 Inner join Master_SubCategoryLvl_1 sb1 on sb._SubCategoryID=sb1._ID
									 	 Inner Join Master_ShoppingCategory s On s._ID=sb1._CategoryID
									 where ((@LoginID<>1 and sb._IsActive= 1) or @LoginID = 1) and(sb1._ID = @SubCategoryId or @SubCategoryId = 0)";
	}

    public class ProcGetSubCategoryLvl2ById : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetSubCategoryLvl2ById(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<ShoppingSubCategoryLvl2>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@lt", req.LoginTypeID),
                new SqlParameter("@SubCategoryId", req.CommonInt),
                new SqlParameter("@id", req.CommonInt2)
            };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ShoppingSubCategoryLvl2
                        {
                            CategoryID = Convert.ToInt32(dr["_categoryID"], CultureInfo.InvariantCulture),
                            CategoryName = dr["_CategoryName"] is DBNull ? "" : dr["_CategoryName"].ToString(),
                            SubCategoryID = Convert.ToInt32(dr["_subcategoryID1"], CultureInfo.InvariantCulture),
                            SubCategoryName = dr["_SubCategoryName1"] is DBNull ? "" : dr["_SubCategoryName1"].ToString(),
                            SubCategoryIDLvl2 = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
                            SubCategoryNameLvl2 = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"], CultureInfo.InvariantCulture),
                            IsNextLevelExists = dr["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dr["_IsNextLevel"], CultureInfo.InvariantCulture),
                            Commission = dr["_MinimumCommission"] is DBNull ? 0 : Convert.ToDecimal(dr["_MinimumCommission"], CultureInfo.InvariantCulture),
                            CommissionType = dr["_CommType"] is DBNull ? false : Convert.ToBoolean(dr["_CommType"], CultureInfo.InvariantCulture),
                            ProductCount = dr["ProductCount"] is DBNull ? 0 : Convert.ToInt32(dr["ProductCount"], CultureInfo.InvariantCulture),
                            ImagePath = "Image/icon/Shopping/S2_" + Convert.ToString(dr["_ID"], CultureInfo.InvariantCulture) + ".png"
                        };
                        //string Subcategory = "S_2";
                        //StringBuilder sb = new StringBuilder();
                        //sb.Append(DOCType.ShoppingImagePath.Replace("{0}", Subcategory));
                        //sb.Append("_");
                        //sb.Append(data.SubCategoryIDLvl2.ToString());
                        //sb.Append(".png");
                        //data.FilePath = sb.ToString();
                        res.Add(data);

                    };

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
        public string GetName() => "proc_GetSubCategory2";
    }
}
