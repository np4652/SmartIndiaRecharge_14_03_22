using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetMasterProductByID : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetMasterProductByID(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var data = new MasterProduct();
            var PID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@PID", PID)
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    data.ProductID = Convert.ToInt32(dr["_ID"]);
                    data.ProductName = Convert.ToString(dr["_ProductName"]);
                    data.CategoryID = Convert.ToInt32(dr["_CategoryID"]);
                    data.SubCategoryID1 = Convert.ToInt32(dr["_SubCategoryID1"]);
                    data.SubCategoryID2 = Convert.ToInt32(dr["_SubCategoryID2"]);
                    data.Description = Convert.ToString(dr["_Description"]);
                    data.WalletDeductionPerc = Convert.ToDecimal(dr["_WalletDeductionPerc"]);
                    data.Keyword= Convert.ToString(dr["Keyword1"]); ;
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
            return data;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => @"declare @Keyword varchar(500)
select @Keyword= STUFF(
                 (SELECT ',' + mk.Keyword from Master_Product mp inner join
tbl_KeywordMapping kmm on mp._ID=kmm.ProductId
left outer join tbl_ShoppingKeyword mk on mk.Id=kmm.KeywordId where _ID=@PID FOR XML PATH ('')), 1, 1, ''
               ) 
select top 1 *,@Keyword Keyword  from Master_Product mp left join
tbl_KeywordMapping kmm on mp._ID=kmm.ProductId
left outer join tbl_ShoppingKeyword mk on mk.Id=kmm.KeywordId where _ID=@PID";
    }
}
