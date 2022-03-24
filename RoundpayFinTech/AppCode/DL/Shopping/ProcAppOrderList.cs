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
using System.Text;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcAppOrderList : IProcedure
    {
        private readonly IDAL _dal;

        public ProcAppOrderList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<AppOrderModel>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@OrderId", req.CommonInt)
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    DataTable dtOrder = ds.Tables[0];
                    DataTable dtOrderDetail = ds.Tables[1];
                    if (dtOrder.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtOrder.Rows)
                        {
                            List<AppOrderDetailModel> orderDetail = new List<AppOrderDetailModel>();
                            SAddress address = new SAddress();
                            foreach (DataRow dtRow in dtOrderDetail.Select("_OrderID=" + row["_Id"].ToString()))
                            {
                                string ImgUrl = null;
                                string path = DOCType.ProductImagePath.Replace("{0}", dtRow["_ProductID"].ToString());
                                if (Directory.Exists(path))
                                {
                                    DirectoryInfo d = new DirectoryInfo(path);
                                    FileInfo[] Files = d.GetFiles(dtRow["_ProductDetailID"].ToString() + "_*");
                                    foreach (FileInfo file in Files)
                                    {
                                        ImgUrl = file.Name;
                                        break;
                                    }
                                }
                                orderDetail.Add(new AppOrderDetailModel
                                {
                                    OrderDetailId = Convert.ToInt32(dtRow["_Id"]),
                                    OrderId = Convert.ToInt32(dtRow["_OrderID"]),
                                    MRP = Convert.ToDecimal(dtRow["_MRP"]),
                                    Discount = Convert.ToDecimal(dtRow["_Discount"]),
                                    DiscountType = Convert.ToBoolean(dtRow["_DiscountType"]),
                                    Quantity = Convert.ToInt32(dtRow["_Quantity"]),
                                    Status = Convert.ToInt32(dtRow["_Status"]),
                                    IsPaid = (Convert.ToInt32(dtRow["_IsPaid"]) == 0) ? false : true,
                                    IsOrderClosed = (Convert.ToInt32(dtRow["_IsOrderClosed"]) == 0) ? false : true,
                                    SellingPrice = dtRow["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dtRow["_SellingPrice"]),
                                    ShippingCharge = dtRow["_ShippingCharge"] is DBNull ? 0 : Convert.ToDecimal(dtRow["_ShippingCharge"]),
                                    ShippingMode = dtRow["_ShippingMode"] is DBNull ? 0 : Convert.ToInt32(dtRow["_ShippingMode"]),
                                    ProductId = dtRow["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dtRow["_ProductID"]),
                                    ProductDetailId = dtRow["_ProductDetailID"] is DBNull ? 0 : Convert.ToInt32(dtRow["_ProductDetailID"]),
                                    ProductName = dtRow["_ProductName"] is DBNull ? "" : Convert.ToString(dtRow["_ProductName"]),
                                    ProductImage = ImgUrl
                                });
                            }
                            if (req.CommonInt > 0)
                            {
                                DataTable dtShipAddress = ds.Tables[2];
                                if(dtShipAddress.Rows.Count > 0)
                                {
                                    address = new SAddress
                                    {
                                        ID = Convert.ToInt32(dtShipAddress.Rows[0]["_id"]),
                                        UserId = Convert.ToInt32(dtShipAddress.Rows[0]["_UserID"]),
                                        Address = dtShipAddress.Rows[0]["_Address"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["_Address"]),
                                        Landmark = dtShipAddress.Rows[0]["_LandMark"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["_LandMark"]),
                                        MobileNo = dtShipAddress.Rows[0]["_MobileNo"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["_MobileNo"]),
                                        CityID = dtShipAddress.Rows[0]["_CityID"] is DBNull ? 0 : Convert.ToInt32(dtShipAddress.Rows[0]["_CityID"]),
                                        CustomerName = dtShipAddress.Rows[0]["_CustomerName"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["_CustomerName"]),
                                        Title = dtShipAddress.Rows[0]["_Title"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["_Title"]),
                                        StateID = dtShipAddress.Rows[0]["_StateID"] is DBNull ? 0 : Convert.ToInt32(dtShipAddress.Rows[0]["_StateID"]),
                                        IsDefault = Convert.ToBoolean(dtShipAddress.Rows[0]["_IsDefault"]),
                                        Area = dtShipAddress.Rows[0]["_City"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["_Area"]),
                                        City = dtShipAddress.Rows[0]["_City"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["_City"]),
                                        State = dtShipAddress.Rows[0]["StateName"] is DBNull ? "" : Convert.ToString(dtShipAddress.Rows[0]["StateName"])
                                    };
                                }
                            }
                            res.Add(new AppOrderModel
                            {
                                StatusCode = Convert.ToInt32(row["StatusCode"]),
                                Msg = row["Msg"].ToString(),
                                OrderId = Convert.ToInt32(row["_Id"]),
                                UserId = Convert.ToInt32(row["_UserID"]),
                                Quantity = Convert.ToInt32(row["_Quantity"]),
                                TotalCost = Convert.ToDecimal(row["_TotalCost"]),
                                OrderDate = row["EntryDate"].ToString(),
                                Status = Convert.ToInt32(row["_Status"]),
                                RequestMode = Convert.ToInt32(row["_RequestMode"]),
                                TotalShipping = Convert.ToDecimal(row["_TotalShipping"]),
                                TotalMRP = Convert.ToDecimal(row["TotalMRP"]),
                                TotalDiscount = Convert.ToDecimal(row["TotalDiscount"]),
                                TotalRequestedAmount = Convert.ToDecimal(row["TotalRequestedAmount"]),
                                TotalDebit = Convert.ToDecimal(row["TotalDebit"]),
                                OrderDetailList = orderDetail,
                                ShippingAddress = address
                            });
                        }
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
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAppOrderList";
    }
}
