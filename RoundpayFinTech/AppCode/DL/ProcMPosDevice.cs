using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMPosDevice : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMPosDevice(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MPosDeviceInventoryModel)obj;

            var res = new List<MPosDeviceInventoryModel>();
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LoginID", _req.LoginID),
                    new SqlParameter("@LT", _req.LoginTypeID),
                    new SqlParameter("@Id", _req.ID)
                };
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var resItem = new MPosDeviceInventoryModel
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            VendorId = Convert.ToInt32(dt.Rows[i]["_DeviceVendor"]),
                            VendorName = dt.Rows[i]["VendorName"].ToString(),
                            DeviceModelId = dt.Rows[i]["_DeviceModel"] is DBNull? 0 : Convert.ToInt32(dt.Rows[i]["_DeviceModel"]),
                            DeviceModelName = dt.Rows[i]["DeviceName"] is DBNull ? "" : dt.Rows[i]["DeviceName"].ToString(),
                            DeviceSerial = dt.Rows[i]["_DeviceSerialNo"].ToString(),
                            UserId = Convert.ToInt32(dt.Rows[i]["_UserID"]),
                            UserName = dt.Rows[i]["UserName"] is DBNull ? string.Empty : dt.Rows[i]["UserName"].ToString(),
                            AssignedDate = dt.Rows[i]["_AssignedDate"] is DBNull ? string.Empty : dt.Rows[i]["_AssignedDateCus"].ToString(),
                            OutletId = dt.Rows[i]["_OutletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OutletID"]),
                            OutletName = dt.Rows[i]["OutletName"] is DBNull ? string.Empty : dt.Rows[i]["OutletName"].ToString(),
                            MappedDate = dt.Rows[i]["_MappedDate"] is DBNull ? string.Empty : dt.Rows[i]["_MappedDateCus"].ToString(),
                            IsActive = dt.Rows[i]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsActive"])
                        };
                        res.Add(resItem);
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetMPosDevice";
    }

    public class ProcMPosDeviceCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMPosDeviceCU(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MPosDeviceInventoryModel)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LoginID", _req.LoginID),
                    new SqlParameter("@LT", _req.LoginTypeID),
                    new SqlParameter("@Id", _req.ID),
                    new SqlParameter("@VendorId", _req.VendorId),
                    new SqlParameter("@ModelId", _req.DeviceModelId),
                    new SqlParameter("@DeviceSerial", _req.DeviceSerial)
                };
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_MPosDeviceCU";
    }

    public class ProcMPosDeviceAssignment : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMPosDeviceAssignment(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MPosDeviceInventoryModel)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LoginID", _req.LoginID),
                    new SqlParameter("@LT", _req.LoginTypeID),
                    new SqlParameter("@Id", _req.ID),
                    new SqlParameter("@UserId", _req.UserId),
                    new SqlParameter("@OutletId", _req.OutletId),
                    new SqlParameter("@IP", _req.IPAddress),
                    new SqlParameter("@Browser", _req.Browser),
                };
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_MPosDeviceAssignMap";
    }

    public class ProcMPosDeviceToggle : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMPosDeviceToggle(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MPosDeviceInventoryModel)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                string query = "update tbl_MPOSDeviceInventory set _IsActive = " + (_req.IsActive == true ? "1" : "0") + " where _id = " + _req.ID.ToString() + "; select 1 as StatusCode, 'Success' as Msg";
                DataTable dt = _dal.Get(query);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "";
    }

    public class ProcMPosDeviceUnAssignUnMap : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMPosDeviceUnAssignUnMap(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MPosDeviceInventoryModel)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                string query = "declare @dt datetime = getdate(); update tbl_MPOSDeviceInventory set _ModifyDate = @dt, _OutletID = 0, _MappedDate = @dt";
                if (_req.DeAssign) { query += " ,_UserID = 1, _AssignedDate =@dt "; }
                query += " where _id= " + _req.ID.ToString() + "; select 1 as StatusCode, 'Success' as Msg";
                DataTable dt = _dal.Get(query);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "";
    }
}
