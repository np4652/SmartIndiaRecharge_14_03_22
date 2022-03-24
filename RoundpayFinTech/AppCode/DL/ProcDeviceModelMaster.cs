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
    public class ProcDeviceModelMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeviceModelMaster(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MasterDeviceModel)obj;

            var res = new List<MasterDeviceModel>();
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
                        var resItem = new MasterDeviceModel
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_Id"]),
                            VendorId = Convert.ToInt32(dt.Rows[i]["_VendorId"]),
                            VendorName = dt.Rows[i]["_VendorName"].ToString(),
                            ServiceId = dt.Rows[i]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_Type"]),
                            ServiceName = dt.Rows[i]["_SeviceName"] is DBNull ? "" : dt.Rows[i]["_SeviceName"].ToString(),
                            ModelName = dt.Rows[i]["_Name"].ToString(),
                            Remark = dt.Rows[i]["_Remark"] is DBNull ? string.Empty : dt.Rows[i]["_Remark"].ToString(),
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
        public string GetName() => "proc_GetDeviceModelMaster";
    }

    public class ProcDeviceModelMasterCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeviceModelMasterCU(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MasterDeviceModel)obj;

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
                    new SqlParameter("@ServiceId", _req.ServiceId),
                    new SqlParameter("@ModelName", _req.ModelName),
                    new SqlParameter("@Remark", _req.Remark)
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
        public string GetName() => "proc_DeviceModelMasterCU";
    }

    public class ProcDeviceModelMasterToggle : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeviceModelMasterToggle(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MasterDeviceModel)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                string query = "update Master_Device set _IsActive = " + (_req.IsActive == true ? "1" : "0") + " where _id = " + _req.ID.ToString() + "; select 1 as StatusCode, 'Success' as Msg";
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

    public class ProcDeviceModelDdl : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeviceModelDdl(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int vendorId = (int)obj;

            var res = new List<MasterDeviceModel>();
            try
            {
                string query = "select * from Master_Device where _VendorId = " + vendorId.ToString();
                DataTable dt = _dal.Get(query);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var resItem = new MasterDeviceModel
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_Id"]),
                            VendorId = Convert.ToInt32(dt.Rows[i]["_VendorId"]),
                            ServiceId = dt.Rows[i]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_Type"]),
                            ModelName = dt.Rows[i]["_Name"].ToString(),
                            Remark = dt.Rows[i]["_Remark"] is DBNull ? string.Empty : dt.Rows[i]["_Remark"].ToString(),
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
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "";
    }
}
