using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetMasterProduct : IProcedure
	{
		private readonly IDAL _dal;

		public ProcGetMasterProduct(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<MasterProduct>();
			var req = (MasterProduct)obj;
			SqlParameter[] param = {
				new SqlParameter("@CategoryID", req.CategoryID),
				new SqlParameter("@SubCategoryID1", req.SubCategoryID1),
				new SqlParameter("@SubCategoryID2", req.SubCategoryID2)
			};
			try
			{
				DataTable dt = _dal.Get(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var data = new MasterProduct
						{
							ProductID = Convert.ToInt32(dr["_ID"]),
							ProductName = Convert.ToString(dr["_ProductName"]),
							Description = Convert.ToString(dr["_Description"]),
							WalletDeductionPerc = dr["_WalletDeductionPerc"] is DBNull ? 0 : Convert.ToDecimal(dr["_WalletDeductionPerc"]),
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
		public string GetName() => @"select _ID,_ProductName,_Description,_WalletDeductionPerc from Master_Product where (_CategoryID=@CategoryID or @CategoryID=0) and (_SubCategoryID1=@SubCategoryID1 or @SubCategoryID1=0) and (_SubCategoryID2=@SubCategoryID2 or @SubCategoryID2=0)";
	}
}
