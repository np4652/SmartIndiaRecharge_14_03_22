using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDeviceMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDeviceMaster(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            
            var deviceMasters = new List<DeviceMaster>();
            var deviceMaster = new DeviceMaster();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    if (_req.CommonInt > 0)
                    {
                        deviceMaster = new DeviceMaster
                        {
                            ID = Convert.ToInt32(dt.Rows[0]["_ID"]),
                            DeviceName = dt.Rows[0]["_Name"].ToString(),
                            Remark = dt.Rows[0]["_Remark"] is DBNull ? string.Empty : dt.Rows[0]["_Remark"].ToString(),
                            Img = dt.Rows[0]["_ID"].ToString() + ".jpg"
                        };
                    }
                    else
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var deviceM = new DeviceMaster
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                
                                DeviceName = dt.Rows[i]["_Name"].ToString(),
                                Remark = dt.Rows[i]["_Remark"] is DBNull ? string.Empty : dt.Rows[i]["_Remark"].ToString(),
                                Img = dt.Rows[i]["_ID"].ToString() + ".jpg"
                            };
                            deviceMasters.Add(deviceM);
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            if (_req.CommonInt > 0)
                return deviceMaster;
            return deviceMasters;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetFingerPrintDevice";
    }

    public class ProcUpdateUserDeviceID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateUserDeviceID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID", req.LoginID),
                new SqlParameter("@FPDeviceTypeID",req.CommonInt2),
                new SqlParameter("@FPDeviceID",req.CommonStr),
            };
            try
            {
                _dal.Execute(GetName(), param);
                return true;
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

            return false;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "update tbl_Users set _FPDeviceID=@FPDeviceID, _FPDeviceTypeID=@FPDeviceTypeID where _ID=@ID";
    }
}
