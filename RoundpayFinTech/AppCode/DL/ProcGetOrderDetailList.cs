using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetOrderDetailList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetOrderDetailList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@Mode",req.CommonStr),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var res = new List<OrderDetailList>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new OrderDetailList
                        {
                           OrderId = dr["_OrderID"] is DBNull ? 0 :Convert.ToInt32(dr["_OrderID"]),
                            Quantity = dr["_Quantity"] is DBNull ? 0 : Convert.ToInt32(dr["_Quantity"]),
                            UserId = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            TotalCost = dr["_TotalCost"] is DBNull ? 0 : Convert.ToDecimal(dr["_TotalCost"]),
                           EntryDate = dr["_EntryDate"] is DBNull ? string.Empty : Convert.ToString(dr["_EntryDate"]),
                            ProductId = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductDetailId = dr["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductDetailID"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            ProductName = dr["_ProductName"] is DBNull ? string.Empty : Convert.ToString(dr["_ProductName"]),
                           OrderDetailId = dr["OrderDetailId"] is DBNull ? 0 : Convert.ToInt32(dr["OrderDetailId"]),
                            VendorName = dr["_VendorName"] is DBNull ? string.Empty : Convert.ToString(dr["_VendorName"]),
                           IsPaid = dr["_IsPaid"] is DBNull ? false : Convert.ToBoolean(dr["_IsPaid"]),
                            OrderStatus = dr["_Status"] is DBNull ? 0 : Convert.ToInt32(dr["_Status"]),
                           IsOrderClosed = dr["_IsOrderClosed"] is DBNull ? false : Convert.ToBoolean(dr["_IsOrderClosed"])
                        });
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
        public string GetName() => "Proc_GetOrderDetailList";
    }
}