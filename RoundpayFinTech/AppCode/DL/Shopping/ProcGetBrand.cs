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
    public class ProcGetBrand : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetBrand(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<Brand>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@CategoryID", req.CommonInt),
				new SqlParameter("@SubCategoryId", req.CommonInt2)
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var data = new Brand
						{
							BrandId = Convert.ToInt32(dr["_ID"]),
							BrandName=Convert.ToString(dr["_BrandName"])
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
		public string GetName() => "Proc_GetBrands";
	}

	public class ProcGetBrandById : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetBrandById(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<Brand>();
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@BrandId", req.CommonInt)
			};
			try
			{
				DataTable dt = _dal.Get(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var data = new Brand
						{
							BrandId = Convert.ToInt32(dr["_ID"]),
							BrandName = Convert.ToString(dr["_BrandName"]),
							CategoryID = Convert.ToInt32(dr["_CategoryID"]),
							CategoryName= Convert.ToString(dr["_CategoryName"]),
							IsActive = Convert.ToBoolean(dr["_IsActive"])
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
		public string GetName() => @"SELECT b._ID, b._BrandName, b._CategoryID, c._CategoryName, b._IsActive FROM Master_Brand b left outer join tbl_shoppingmainCategory c on c._ID = b._CategoryID where (b._ID=@BrandId or @BrandId = 0)";
	}
}
