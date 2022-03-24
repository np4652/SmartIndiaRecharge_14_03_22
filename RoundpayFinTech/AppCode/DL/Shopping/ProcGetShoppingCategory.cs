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
    public class ProcGetShoppingCategory : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetShoppingCategory(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            int LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            var res = new List<ShoppingCategory>();
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ShoppingCategory
                        {
                            CategoryID = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
                            CategoryName = dr["_CategoryName"] is DBNull ? "" : dr["_CategoryName"].ToString(),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"],CultureInfo.InvariantCulture),
                            IsNextLevelExists = dr["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dr["_IsNextLevel"], CultureInfo.InvariantCulture)
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "select * from Master_ShoppingCategory where (@LoginID<>1 and _IsActive=1) or @LoginID=1";
    }

    public class ProcGetShoppingCategoryById : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetShoppingCategoryById(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@lt", req.LoginTypeID),
                new SqlParameter("@id", req.CommonInt)
            };
            var res = new List<ShoppingCategory>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ShoppingCategory
                        {
                            CategoryID = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
                            CategoryName = dr["_CategoryName"] is DBNull ? "" : dr["_CategoryName"].ToString(),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"], CultureInfo.InvariantCulture),
                            IsNextLevelExists = dr["_IsNextLevel"] is DBNull ? false : Convert.ToBoolean(dr["_IsNextLevel"], CultureInfo.InvariantCulture),
                            ProductCount = dr["ProductCount"] is DBNull ? 0 : Convert.ToInt32(dr["ProductCount"], CultureInfo.InvariantCulture),
                            ImagePath = "Image/icon/Shopping/" + Convert.ToString(dr["_ID"]) + ".png"
                        };
                        StringBuilder builder = new StringBuilder();
                        builder.Append(DOCType.ShoppingImagePath.Replace("{0}", Convert.ToString(dr["_ID"], CultureInfo.InvariantCulture)));
                        builder.Append(".png");
                        data.ImagePath = (File.Exists(builder.ToString())) ? builder.ToString() : null;
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
        public string GetName() => "proc_GetCategories";
    }
}
