using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRecentDaysTransaction : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRecentDaysTransaction(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@OpTypeID",req.CommonInt),
                new SqlParameter("@ServiceID",req.CommonInt2),
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@OutletID",req.CommonInt3)
            };
            var res = new List<ProcRecentTransactionCounts>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRecentTransactionCounts
                    {
                        EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                        ValueFloat = dt.Rows[i]["Value"] is DBNull ? 0 : float.Parse(dt.Rows[i]["Value"].ToString()),
                        _AmountLMTD = dt.Rows[i]["_AmountLMTD"] is DBNull ? 0 : float.Parse(dt.Rows[i]["_AmountLMTD"].ToString()),
                        _AmountTillDate = dt.Rows[i]["_AmountTillDate"] is DBNull ? 0 : float.Parse(dt.Rows[i]["_AmountTillDate"].ToString()),
                        _AmountLastDay = dt.Rows[i]["_AmountLastDay"] is DBNull ? 0 : float.Parse(dt.Rows[i]["_AmountLastDay"].ToString()),
                        _AmountCurrentDay = dt.Rows[i]["_AmountCurrentDay"] is DBNull ? 0 : float.Parse(dt.Rows[i]["_AmountCurrentDay"].ToString()),
                        LMTDCount = dt.Rows[i]["LMTDCount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["LMTDCount"].ToString()),
                        TillDateCount = dt.Rows[i]["TillDateCount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["TillDateCount"].ToString()),
                        _LastDayCount = dt.Rows[i]["_LastDayCount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_LastDayCount"].ToString()),
                        _CurrentDayCount = dt.Rows[i]["_CurrentDayCount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_CurrentDayCount"].ToString()),
                        LastDay_Current_Diff = dt.Rows[i]["LastDay_Current_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["LastDay_Current_Diff"].ToString()),
                        LMTD_MTD_Diff = dt.Rows[i]["LMTD_MTD_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["LMTD_MTD_Diff"].ToString()),
                        //ServiceRenderIndex = dt.Rows[i]["ServiceRenderIndex"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["ServiceRenderIndex"].ToString())
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_RecentDaysTransaction";
    }

    public class ProcMonthWeekDaysTransaction : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcMonthWeekDaysTransaction(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@ActivityType",req.CommonStr),
                new SqlParameter("@RequestedDataType",req.CommonInt),
                new SqlParameter("@ServiceTypeID",req.CommonInt2)
            };
            var res = new List<ProcRecentTransactionCounts>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRecentTransactionCounts
                    {
                        EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                        ValueFloat = dt.Rows[i]["Value"] is DBNull ? 0 : float.Parse(dt.Rows[i]["Value"].ToString())
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetMontlyWeeklyDailyTransaction";
    }

    public class ProcRecentDaysPriSecTer : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRecentDaysPriSecTer(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@PriSecTerType",req.CommonInt),
                new SqlParameter("@UserID",req.LoginID)
            };
            var res = new List<ProcRecentTransactionCounts>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRecentTransactionCounts
                    {
                        EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                        ValueFloat = dt.Rows[i]["Value"] is DBNull ? 0 : float.Parse(dt.Rows[i]["Value"].ToString())
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_RecentDaysPriSecTer";
    }

    public class ProcRecentDaysTransactionRecharge : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRecentDaysTransactionRecharge(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@OutletID",req.CommonInt3)
            };
            var res = new List<ProcRecentTransactionCounts>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRecentTransactionCounts
                    {
                        EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                        Value = dt.Rows[i]["Value"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["Value"].ToString())
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_RecentDaysRechargeTransaction";
    }

    public class ProcMostUsedServices : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcMostUsedServices(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID)
            };
            var res = new List<MostUsedServices>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new MostUsedServices
                    {
                        UserID = dt.Rows[i]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_UserID"].ToString()),
                        ServiceID = dt.Rows[i]["_ServiceID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ServiceID"].ToString()),
                        ServiceRenderIndex = dt.Rows[i]["ServiceRenderIndex"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["ServiceRenderIndex"].ToString())
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_MostUsedServices";
    }

    public class ProcRecentTransactionActivity : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRecentTransactionActivity(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID)
            };
            var res = new List<ProcRecentTransactionCounts>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRecentTransactionCounts
                    {
                        EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                        ValueFloat = dt.Rows[i]["Value"] is DBNull ? 0 : float.Parse(dt.Rows[i]["Value"].ToString()),
                        Operator = dt.Rows[i]["_Operator"] is DBNull ? "-" : dt.Rows[i]["_Operator"].ToString(),
                        Status = dt.Rows[i]["_Type"] is DBNull ? "-" : dt.Rows[i]["_Type"].ToString(),
                        OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"].ToString()),
                        Account = dt.Rows[i]["_Account"] is DBNull ? "" : dt.Rows[i]["_Account"].ToString(),
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetRecentTertiaryTransaction";
    }

    public class ProcTodayTransactionStatus : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcTodayTransactionStatus(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@RequestedDataType",req.CommonInt),
            };
            var res = new List<ProcRecentTransactionCounts>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRecentTransactionCounts
                    {
                        ValueFloat = dt.Rows[i]["Value"] is DBNull ? 0 : float.Parse(dt.Rows[i]["Value"].ToString()),
                        Status = dt.Rows[i]["_Type"] is DBNull ? "-" : dt.Rows[i]["_Type"].ToString(),
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetTodayTransactionStatus";
    }

    public class ProcOpTypeWiseTransactionStatus : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcOpTypeWiseTransactionStatus(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@RequestedDataType",req.CommonInt),
                new SqlParameter("@RequestedDate",req.CommonStr),
                new SqlParameter("@OpTypeID",req.CommonInt2),
            };
            var res = new List<ProcRecentTransactionCounts>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRecentTransactionCounts
                    {
                        ValueFloat = dt.Rows[i]["Value"] is DBNull ? 0 : float.Parse(dt.Rows[i]["Value"].ToString()),
                        Status = dt.Rows[i]["_Type"] is DBNull ? "-" : dt.Rows[i]["_Type"].ToString(),
                    };
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetDateOptypeWiseTransactionStatus";
    }
}