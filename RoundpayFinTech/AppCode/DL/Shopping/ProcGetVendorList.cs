using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetVendorList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVendorList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@lt", req.LoginTypeID)
            };
            var res = new List<VendorMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new VendorMaster
                        {
                            Id = Convert.ToInt32(dr["_Id"]),
                            VendorName = Convert.ToString(dr["_VendorName"]),
                            VendorUserID = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
                            IsB2BAllowed = dr["_IsB2BAllowed"] is DBNull ? false : Convert.ToBoolean(dr["_IsB2BAllowed"]),
                            IsOnlyB2B = dr["_IsOnlyB2B"] is DBNull ? false : Convert.ToBoolean(dr["_IsOnlyB2B"]),
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

        public string GetName() => "Proc_GetVendorList";
    }

    public class ProcChangeEComVendorStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcChangeEComVendorStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@lt", req.LoginTypeID),
                new SqlParameter("@type", req.CommonInt),
                new SqlParameter("@id", req.CommonInt2),
                new SqlParameter("@val", req.CommonBool)
            };
            var res = new ResponseStatus();
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

        public string GetName() => "Proc_ChangeEComVendorStatus";
    }

    public class ProcGetEComVendorLocation : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEComVendorLocation(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@lt", req.LoginTypeID),
                new SqlParameter("@ID", req.CommonInt)
            };
            var res = new ResponseStatus();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Status = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0][1]);
                    if(res.Status == ErrorCodes.One)
                    {
                        res.CommonStr = Convert.ToString(dt.Rows[0]["Latitude"]);
                        res.CommonStr2 = Convert.ToString(dt.Rows[0]["Longitude"]);
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

        public string GetName() => "Proc_GetECommVendorLocation";
    }

    public class ProcUpdateEComVendorLocation : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateEComVendorLocation(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@lt", req.LoginTypeID),
                new SqlParameter("@ID", req.CommonInt),
                new SqlParameter("@lat", req.CommonStr),
                new SqlParameter("@long", req.CommonStr2)
            };
            var res = new ResponseStatus();
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

        public string GetName() => "Proc_UpdateECommVendorLocation";
    }
}