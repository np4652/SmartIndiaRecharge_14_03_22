using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveDenomDetailByRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveDenomDetailByRole(IDAL dal) => _dal = dal;
        public string GetName() => "proc_SaveDenomDetailByRole";
        public object Call(object obj)
        {
            var req = (DenomDetailReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.Detail.ID),
                new SqlParameter("@DenomID",req.Detail.DenomID),
                new SqlParameter("@SlabID",req.Detail.SlabID),
                new SqlParameter("@Comm",req.Detail.Comm),
                new SqlParameter("@AmtType",req.Detail.AmtType),
                new SqlParameter("@OID",req.Detail.OID),
                new SqlParameter("@RoleID",req.Detail.RoleID),
                new SqlParameter("@DenomRangeID",req.Detail.DenomRangeID),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }

    public class ProcSaveSpecialSlbalDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveSpecialSlbalDetail(IDAL dal) => _dal = dal;
        public string GetName() => "proc_SaveSpecialSlabDetail";
        public object Call(object obj)
        {
            var req = (CircleWithDomination)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@DenomIDs",req.DenomIDs),
                new SqlParameter("@SlabID",req.SlabID),
                new SqlParameter("@Comm",req.Comm),
                new SqlParameter("@AmtType",req.AmtType),
                new SqlParameter("@CommType",req.CommType),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@DenomRangeIDs",req.DenomRangeIDs),
                new SqlParameter("@CircleID",req.CircleID),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@IsDenom",req.IsDenom),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }

    public class ProcSaveSpecialAPIIDlDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveSpecialAPIIDlDetail(IDAL dal) => _dal = dal;
        public string GetName() => "proc_SaveSpecialAPIIDDetail";
        public object Call(object obj)
        {
            var req = (CircleWithDomination)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@DenomIDs",req.DenomIDs),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@Comm",req.Comm),
                new SqlParameter("@AmtType",req.AmtType),
                new SqlParameter("@CommType",req.CommType),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@DenomRangeIDs",req.DenomRangeIDs),
                new SqlParameter("@CircleID",req.CircleID),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@IsDenom",req.IsDenom),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }

    public class ProcUpdateSpecialSlbalDominaton : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSpecialSlbalDominaton(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateSpecialSlbalDominaton";
        public object Call(object obj)
        {
            var req = (CircleWithDomination)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@SlabID",req.SlabID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@CircleID",req.CircleID),
                new SqlParameter("@IsDenom",req.IsDenom),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }

    public class ProcUpdateSpecialSlbalGroupDominaton : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSpecialSlbalGroupDominaton(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateSpecialSlabGroupDomination";
        public object Call(object obj)
        {
            var req = (CircleWithDomination)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@DenomIDs",req.DenomIDs),
                new SqlParameter("@SlabID",req.SlabID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@DenomRangeIDs",req.DenomRangeIDs),
                new SqlParameter("@CircleID",req.CircleID),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@IsDenom",req.IsDenom),
                new SqlParameter("@AmtType",req.AmtType),
                new SqlParameter("@CommType",req.CommType),
                new SqlParameter("@Comm",req.Comm),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }

    public class ProcUpdateSpecialAPIIDDominaton : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSpecialAPIIDDominaton(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateSpecialAPIIDDominaton";
        public object Call(object obj)
        {
            var req = (CircleWithDomination)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@CircleID",req.CircleID),
                new SqlParameter("@IsDenom",req.IsDenom),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }

    public class ProcUpdateSpecialAPIIDGroupDominaton : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSpecialAPIIDGroupDominaton(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateSpecialAPIIDGroupDomination";
        public object Call(object obj)
        {
            var req = (CircleWithDomination)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@DenomIDs",req.DenomIDs),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@DenomRangeIDs",req.DenomRangeIDs),
                new SqlParameter("@CircleID",req.CircleID),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@IsDenom",req.IsDenom),
                new SqlParameter("@AmtType",req.AmtType),
                new SqlParameter("@CommType",req.CommType),
                new SqlParameter("@Comm",req.Comm),
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }

}


