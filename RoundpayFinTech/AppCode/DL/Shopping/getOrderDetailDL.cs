using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class getOrderDetailDL : IProcedure
    {
        private readonly IDAL _dal;

        public getOrderDetailDL(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<OrderDetailList>();
            var req = (OrderModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@Mode", req.OrderDetailMode),
                new SqlParameter("@OrderId", req.OrderId)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new OrderDetailList
                        {
                            OrderId = Convert.ToInt32(dr["_OrderID"]),
                            OrderDetailId = Convert.ToInt32(dr["OrderDetailId"]),
                            ProductDetailId = Convert.ToInt32(dr["_ProductDetailID"]),
                            ProductId = Convert.ToInt32(dr["_ProductID"]),
                            MRP = Convert.ToDecimal(dr["_MRP"]),
                            Discount = Convert.ToDecimal(dr["_Discount"]),
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            Quantity = Convert.ToInt32(dr["_Quantity"]),
                            IsPaid = dr["_IsPaid"] is DBNull ? false : Convert.ToBoolean(dr["_IsPaid"]),
                            IsOrderClosed = dr["_IsOrderClosed"] is DBNull ? false : Convert.ToBoolean(dr["_IsOrderClosed"]),
                            OrderStatus = dr["_Status"] is DBNull ? 0 : Convert.ToInt32(dr["_Status"]),
                            VendorName = Convert.ToString(dr["_VendorName"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductId.ToString());
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailId.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_getOrderHistory";
    }
}
