using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetDeliveryPersonnel : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetDeliveryPersonnel(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new DeliveryPersonnelList
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (CommonRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginId),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@ActiveOnly", req.CommonBool)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    res.DeliveryPersonnels = new List<DeliveryPersonnel>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new DeliveryPersonnel
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            Name = Convert.ToString(dr["_Name"]),
                            Mobile = Convert.ToString(dr["_Mobile"]),
                            DOB = dr["_DOB"] is DBNull ? "" : Convert.ToString(dr["_DOB"]),
                            Address = dr["_Address"] is DBNull ? "" : Convert.ToString(dr["_Address"]),
                            Area = dr["_Area"] is DBNull ? "" : Convert.ToString(dr["_Area"]),
                            CityId = dr["_CityId"] is DBNull ? 0 : Convert.ToInt32(dr["_CityId"]),
                            Pincode = dr["_Pincode"] is DBNull ? "" : Convert.ToString(dr["_Pincode"]),
                            Location = dr["_Location"] is DBNull ? "" : Convert.ToString(dr["_Location"]),
                            VehicleNumber = dr["_VehicalNum"] is DBNull ? "" : Convert.ToString(dr["_VehicalNum"]),
                            Aadhar = dr["_Aadhar"] is DBNull ? "" : Convert.ToString(dr["_Aadhar"]),
                            DLId = dr["_DLId"] is DBNull ? "" : Convert.ToString(dr["_DLId"]),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
                            Password = dr["_Password"] is DBNull ? "" : Convert.ToString(dr["_Password"])
                        };
                        //string path = DOCType.ProductImagePath.Replace("{0}", data.ProductId.ToString());
                        //if (Directory.Exists(path))
                        //{
                        //    DirectoryInfo d = new DirectoryInfo(path);
                        //    FileInfo[] Files = d.GetFiles(data.ProductDetailId.ToString() + "_*");
                        //    foreach (FileInfo file in Files)
                        //    {
                        //        data.ImgUrl = file.Name;
                        //        break;
                        //    }
                        //}
                        res.DeliveryPersonnels.Add(data);
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
                    UserId = req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetDeliveryPersonnel";
    }

    public class ProcAUDeliveryPersonnel : IProcedure
    {
        private readonly IDAL _dal;

        public ProcAUDeliveryPersonnel(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (AUDeliverPersonnel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@ID", req.ID),
                new SqlParameter("@Name", req.Name),
                new SqlParameter("@Mobile", req.Mobile),
                //new SqlParameter("@DOB", req.DOB),
                new SqlParameter("@Address", req.Address),
                new SqlParameter("@Area", req.Area),
                new SqlParameter("@CityId", req.CityId),
                new SqlParameter("@Pincode", req.Pincode),
                //new SqlParameter("@Location", req.Location),
                new SqlParameter("@VehicalNum", req.VehicleNumber),
                new SqlParameter("@Aadhar", req.Aadhar),
                new SqlParameter("@DLId", req.DLId),
                new SqlParameter("@IsActive", req.IsActive),
                new SqlParameter("@Password", req.Password)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0][1]);
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
        public string GetName() => "Proc_AUDeliveryPersonnel";
    }

    public class ProcUpdateDeliveryPersonnelStatus : IProcedure
    {
        private readonly IDAL _dal;

        public ProcUpdateDeliveryPersonnelStatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (AUDeliverPersonnel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@ID", req.ID),
                new SqlParameter("@IsActive", req.IsActive)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0][1]);
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
        public string GetName() => "Proc_UpdateDeliveryPersonnelStatus";
    }

    public class ProcUpdateDeliveryPersonnelStatusLocation : IProcedure
    {
        private readonly IDAL _dal;

        public ProcUpdateDeliveryPersonnelStatusLocation(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@ID", req.CommonInt),
                new SqlParameter("@status", req.CommonInt2),
                new SqlParameter("@orderDetailId", req.CommonInt3),
                new SqlParameter("@lat", req.CommonStr ?? ""),
                new SqlParameter("@long", req.CommonStr2 ?? "")
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0][1]);
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
        public string GetName() => "Proc_UpdateDeliveryPersonnelStatusLocation";
    }

    public class ProcLoginDeliveryPersonnel : IProcedure
    {
        private readonly IDAL _dal;

        public ProcLoginDeliveryPersonnel(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new LoginDeliveryPersonnel
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (LoginDeliveryPersonnelReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.Username),
                new SqlParameter("@Password", req.Password),
                new SqlParameter("@token", req.Token),
                new SqlParameter("@IP", req.IP),
                new SqlParameter("@Browser", req.Browser),
                new SqlParameter("@RequestMode", req.RequestMode),
                new SqlParameter("@LoginTypeID", req.LoginTypeID),
                new SqlParameter("@IMEI", req.IMEI)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        res.SessId = Convert.ToInt32(dt.Rows[0]["SessId"]);
                        res.SessionKey = Convert.ToString(dt.Rows[0]["SessionKey"]);
                        res.CookieExpire = Convert.ToString(dt.Rows[0]["CookieExpires"]);
                        res.ID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        res.Name = Convert.ToString(dt.Rows[0]["_Name"]);
                        res.Mobile = Convert.ToString(dt.Rows[0]["_Mobile"]);
                        res.Address = dt.Rows[0]["_Address"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Address"]);
                        res.Area = dt.Rows[0]["_Area"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Area"]);
                        res.CityId = dt.Rows[0]["_CityId"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CityId"]);
                        res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Pincode"]);
                        res.VehicleNumber = dt.Rows[0]["_VehicalNum"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_VehicalNum"]);
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
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_LoginDeliveryPersonnel";
    }

    public class ProcUpdateDeliveryPersonnelToken : IProcedure
    {
        private readonly IDAL _dal;

        public ProcUpdateDeliveryPersonnelToken(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@SessId", req.CommonInt),
                new SqlParameter("@token", req.CommonStr ?? ""),
                new SqlParameter("@SessionKey", req.CommonStr2 ?? "")
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0][1]);
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
        public string GetName() => "Proc_UpdateDeliveryPersonnelToken";
    }

    public class ProcGetOrderDetailForDelivery : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetOrderDetailForDelivery(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new OrderDeliveryResp
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@orderDetailId", req.CommonInt)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        res.Id = Convert.ToInt32(dt.Rows[0]["_Id"]);
                        res.DPId = Convert.ToInt32(dt.Rows[0]["_DPId"]);
                        res.DPLat = Convert.ToString(dt.Rows[0]["_DPLat"]);
                        res.DPLong = Convert.ToString(dt.Rows[0]["_DPLong"]);
                        res.IsPicked = Convert.ToBoolean(dt.Rows[0]["_IsPicked"]);
                        res.IsDelivered = Convert.ToBoolean(dt.Rows[0]["_IsDelivered"]);
                        res.OrderId = Convert.ToInt32(dt.Rows[0]["_OrderID"]);
                        res.OrderDetailId = Convert.ToInt32(dt.Rows[0]["_OrderDetailId"]);
                        res.ProductName = Convert.ToString(dt.Rows[0]["_ProductName"]);
                        res.Quantity = Convert.ToInt32(dt.Rows[0]["_Quantity"]);
                        res.CustomerAddressId = Convert.ToInt32(dt.Rows[0]["_CustomerAddId"]);
                        res.CustomerAddress = Convert.ToString(dt.Rows[0]["CustomerAddress"]);
                        res.CustomerArea = dt.Rows[0]["CustomerArea"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerArea"]);
                        res.CustomerPinCode = dt.Rows[0]["CustomerPinCode"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerPinCode"]);
                        res.CustomerMobile = dt.Rows[0]["CustomerMobile"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerMobile"]);
                        res.CustomerName = dt.Rows[0]["CustomerName"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerName"]);
                        res.CustomerLandmark = dt.Rows[0]["CustomerLandmark"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerLandmark"]);
                        res.CustomerLat = dt.Rows[0]["CustomerLat"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerLat"]);
                        res.CustomerLong = dt.Rows[0]["CustomerLong"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerLong"]);
                        res.VendorOutlet = dt.Rows[0]["VendorOutlet"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorOutlet"]);
                        res.VendorMobile = dt.Rows[0]["VendorMobile"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorMobile"]);
                        res.VendorAddress = dt.Rows[0]["VendorAddress"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorAddress"]);
                        res.VendorLat = dt.Rows[0]["VendorLat"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorLat"]);
                        res.VendorLong = dt.Rows[0]["VendorLong"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorLong"]);
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
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetOrderDetailForDelivery";
    }

    public class ProcDeliveryDashboard : IProcedure
    {
        private readonly IDAL _dal;

        public ProcDeliveryDashboard(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new DeliveryDashboard
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError,
                OrderList = new List<OrderDeliveryResp>()
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID)
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        res.UserId = req.LoginID;
                        res.Name = Convert.ToString(dt.Rows[0]["_Name"]);
                        res.Mobile = Convert.ToString(dt.Rows[0]["_Mobile"]);
                        res.IsAssigned = Convert.ToBoolean(dt.Rows[0]["_IsAssigned"]);
                        res.IsAvailable = Convert.ToBoolean(dt.Rows[0]["_IsAvailable"]);
                        DataTable data = ds.Tables[1];
                        if (data.Rows.Count > 0)
                        {
                            foreach (DataRow dr in data.Rows)
                            {
                                var item = new OrderDeliveryResp
                                {
                                    Id = Convert.ToInt32(dr["_Id"]),
                                    DPId = Convert.ToInt32(dr["_DPId"]),
                                    DPLat = Convert.ToString(dr["_DPLat"]),
                                    DPLong = Convert.ToString(dr["_DPLong"]),
                                    IsPicked = Convert.ToBoolean(dr["_IsPicked"]),
                                    IsDelivered = Convert.ToBoolean(dr["_IsDelivered"]),
                                    OrderId = Convert.ToInt32(dr["_OrderID"]),
                                    OrderDetailId = Convert.ToInt32(dr["_OrderDetailId"]),
                                    ProductName = Convert.ToString(dr["_ProductName"]),
                                    Quantity = Convert.ToInt32(dr["_Quantity"]),
                                    CustomerAddressId = Convert.ToInt32(dr["_CustomerAddId"]),
                                    CustomerAddress = Convert.ToString(dr["CustomerAddress"]),
                                    CustomerArea = dr["CustomerArea"] is DBNull ? "" : Convert.ToString(dr["CustomerArea"]),
                                    CustomerPinCode = dr["CustomerPinCode"] is DBNull ? "" : Convert.ToString(dr["CustomerPinCode"]),
                                    CustomerMobile = dr["CustomerMobile"] is DBNull ? "" : Convert.ToString(dr["CustomerMobile"]),
                                    CustomerName = dr["CustomerName"] is DBNull ? "" : Convert.ToString(dr["CustomerName"]),
                                    CustomerLandmark = dr["CustomerLandmark"] is DBNull ? "" : Convert.ToString(dr["CustomerLandmark"]),
                                    CustomerLat = dr["CustomerLat"] is DBNull ? "" : Convert.ToString(dr["CustomerLat"]),
                                    CustomerLong = dr["CustomerLong"] is DBNull ? "" : Convert.ToString(dr["CustomerLong"]),
                                    VendorOutlet = dr["VendorOutlet"] is DBNull ? "" : Convert.ToString(dr["VendorOutlet"]),
                                    VendorMobile = dr["VendorMobile"] is DBNull ? "" : Convert.ToString(dr["VendorMobile"]),
                                    VendorAddress = dr["VendorAddress"] is DBNull ? "" : Convert.ToString(dr["VendorAddress"]),
                                    VendorLat = dr["VendorLat"] is DBNull ? "" : Convert.ToString(dr["VendorLat"]),
                                    VendorLong = dr["VendorLong"] is DBNull ? "" : Convert.ToString(dr["VendorLong"])
                                };
                                res.OrderList.Add(item);
                            }
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetDeliveryDashboard";
    }

    public class ProcGetDPLocationHistory : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetDPLocationHistory(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new DPLocationList
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@DPId", req.CommonInt),
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    res.Locations = new List<DPLocationHistory>();
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var item = new DPLocationHistory
                            {
                                Id = Convert.ToInt32(dr["_Id"]),
                                Lat = Convert.ToString(dr["_Lat"]),
                                Long = Convert.ToString(dr["_Long"]),
                                EntryDate = Convert.ToString(dr["_EntryDate"]),
                                DeliveryTypeID = Convert.ToInt32(dr["_DeliveryType"])
                            };
                            res.Locations.Add(item);
                        }
                    }
                    else
                    {
                        res.Locations.Add(new DPLocationHistory());
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
        public string GetName() => "Proc_GetDPLocationHistory";
    }

    public class ProcGetDeliveryOrderDetail : IProcedure // Fetch delivery order list for web
    {
        private readonly IDAL _dal;

        public ProcGetDeliveryOrderDetail(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<OrderDeliveryResp>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@orderDetailId", req.CommonInt),
                new SqlParameter("@userId", req.CommonInt2)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new OrderDeliveryResp
                        {
                            Id = Convert.ToInt32(dt.Rows[0]["_Id"]),
                            DPId = Convert.ToInt32(dt.Rows[0]["_DPId"]),
                            DPName = Convert.ToString(dt.Rows[0]["dpName"]),
                            DPMobile = Convert.ToString(dt.Rows[0]["dpMobile"]),
                            DPLat = Convert.ToString(dt.Rows[0]["_DPLat"]),
                            DPLong = Convert.ToString(dt.Rows[0]["_DPLong"]),
                            IsPicked = Convert.ToBoolean(dt.Rows[0]["_IsPicked"]),
                            IsDelivered = Convert.ToBoolean(dt.Rows[0]["_IsDelivered"]),
                            OrderId = Convert.ToInt32(dt.Rows[0]["_OrderID"]),
                            OrderDetailId = Convert.ToInt32(dt.Rows[0]["_OrderDetailId"]),
                            ProductName = Convert.ToString(dt.Rows[0]["_ProductName"]),
                            Quantity = Convert.ToInt32(dt.Rows[0]["_Quantity"]),
                            CustomerAddressId = Convert.ToInt32(dt.Rows[0]["_CustomerAddId"]),
                            CustomerAddress = Convert.ToString(dt.Rows[0]["CustomerAddress"]),
                            CustomerArea = dt.Rows[0]["CustomerArea"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerArea"]),
                            CustomerPinCode = dt.Rows[0]["CustomerPinCode"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerPinCode"]),
                            CustomerMobile = dt.Rows[0]["CustomerMobile"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerMobile"]),
                            CustomerName = dt.Rows[0]["CustomerName"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerName"]),
                            CustomerLandmark = dt.Rows[0]["CustomerLandmark"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerLandmark"]),
                            CustomerLat = dt.Rows[0]["CustomerLat"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerLat"]),
                            CustomerLong = dt.Rows[0]["CustomerLong"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["CustomerLong"]),
                            VendorOutlet = dt.Rows[0]["VendorOutlet"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorOutlet"]),
                            VendorMobile = dt.Rows[0]["VendorMobile"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorMobile"]),
                            VendorAddress = dt.Rows[0]["VendorAddress"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorAddress"]),
                            VendorLat = dt.Rows[0]["VendorLat"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorLat"]),
                            VendorLong = dt.Rows[0]["VendorLong"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["VendorLong"]),
                        };
                        res.Add(item);
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
        public string GetName() => "GetDeliveryOrderDetail";
    }
}
