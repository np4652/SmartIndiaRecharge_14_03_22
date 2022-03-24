using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCircle : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetCircle(IDAL dal) => _dal = dal;

        public Task<object> Call(object obj) => throw new NotImplementedException();

        public async Task<object> Call()
        {
            var res = new List<CirlceMaster>();
            try
            {
                DataTable dt = await _dal.GetAsync(GetName());
                foreach (DataRow row in dt.Rows)
                {
                    CirlceMaster cirlceMaster = new CirlceMaster
                    {
                        ID = row["_ID"] is DBNull ? (short)0 : Convert.ToInt16(row["_ID"]),
                        Circle = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                        Code = Convert.ToString(row["_Code"])
                    };
                    res.Add(cirlceMaster);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return res;
        }

        public string GetName() => "select _ID,_Name,_Code from tbl_Circle order by _Name";
    }

    public class ProcGetCircleWithAll : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetCircleWithAll(IDAL dal) => _dal = dal;

        public Task<object> Call(object obj) => throw new NotImplementedException();

        public async Task<object> Call()
        {
            var res = new List<CirlceMaster>();
            try
            {
                DataTable dt = await _dal.GetAsync(GetName());
                foreach (DataRow row in dt.Rows)
                {
                    CirlceMaster cirlceMaster = new CirlceMaster
                    {
                        ID = row["_ID"] is DBNull ? (short)0 : Convert.ToInt16(row["_ID"]),
                        Circle = row["_Name"] is DBNull ? "" : row["_Name"].ToString()
                    };
                    res.Add(cirlceMaster);
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public string GetName() => "select _ID,_Name from tbl_Circle UNION ALL (SELECT -1 _ID, 'ALL' _NAME) order by _Name";
    }

    public class ProcGetCircleWithDomination : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetCircleWithDomination(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            List<CircleWithDomination> res = new List<CircleWithDomination>();

            var req = (CircleWithDomination)obj;
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LT",req.LT),
                    new SqlParameter("@LoginID",req.LoginID),
                    new SqlParameter("@CircleID",req.CircleID),
                    //new SqlParameter("@DenomID",req.DenomID),
                    new SqlParameter("@SlabID",req.SlabID),
                    new SqlParameter("@OID",req.OID),
                    //new SqlParameter("@DenomRangeID",req.DenomRangeID),
                    //new SqlParameter("@IsDenom",req.IsDenom),
                };
                var dt = await _dal.GetByProcedureAdapterAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new CircleWithDomination();
                        item.ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"].ToString());
                        item.CircleID = dt.Rows[i]["_CircleID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_CircleID"].ToString());
                        item.CircleName = dt.Rows[i]["_CircleName"] is DBNull ? "" : dt.Rows[i]["_CircleName"].ToString();
                        item.DenomID = dt.Columns.Contains("_DnomID") ? (dt.Rows[i]["_DnomID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DnomID"].ToString())) : 0;
                        item.DenomRangeID = dt.Columns.Contains("_DenomRangeID") ? (dt.Rows[i]["_DenomRangeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DenomRangeID"].ToString())) : 0;
                        item.Amount = dt.Rows[i]["_Amount"] is DBNull ? "" : dt.Rows[i]["_Amount"].ToString();
                        item.OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"].ToString());
                        item.Comm = dt.Rows[i]["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Comm"].ToString());
                        item.AmtType = dt.Rows[i]["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_AmtType"]);
                        item.CommType = dt.Rows[i]["_CommType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_CommType"]);
                        item.IsActive = dt.Rows[i]["_IsActive"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_IsActive"]);
                        item.DominationType = dt.Rows[i]["_DenominationType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DenominationType"].ToString());
                        item.ModificationDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "" : dt.Rows[i]["_ModifyDate"].ToString();
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetCircleWithDominationSpecial";
    }

    public class ProcGetRemainDominationSpecial : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetRemainDominationSpecial(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            List<CircleWithDomination> res = new List<CircleWithDomination>();

            var req = (CircleWithDomination)obj;
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LT",req.LT),
                    new SqlParameter("@LoginID",req.LoginID),
                    new SqlParameter("@CircleID",req.CircleID),
                    new SqlParameter("@SlabID",req.SlabID),
                    new SqlParameter("@OID",req.OID),
                };
                var dt = await _dal.GetByProcedureAdapterAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new CircleWithDomination();
                        item.ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"].ToString());
                        item.Amount = dt.Rows[i]["_Amount"] is DBNull ? "" : dt.Rows[i]["_Amount"].ToString();
                        item.DominationType = dt.Rows[i]["_DenominationType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DenominationType"].ToString());
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetRemainDominationSpecialSlab";
    }

    #region API-REGION
    public class ProcGetCircleWithDominationAPI : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetCircleWithDominationAPI(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            List<CircleWithDomination> res = new List<CircleWithDomination>();

            var req = (CircleWithDomination)obj;
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LT",req.LT),
                    new SqlParameter("@LoginID",req.LoginID),
                    new SqlParameter("@CircleID",req.CircleID),
                    //new SqlParameter("@DenomID",req.DenomID),
                    new SqlParameter("@APIID",req.APIID),
                    new SqlParameter("@OID",req.OID),
                    //new SqlParameter("@DenomRangeID",req.DenomRangeID),
                    //new SqlParameter("@IsDenom",req.IsDenom),
                };
                var dt = await _dal.GetByProcedureAdapterAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new CircleWithDomination();
                        item.ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"].ToString());
                        item.CircleID = dt.Rows[i]["_CircleID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_CircleID"].ToString());
                        item.CircleName = dt.Rows[i]["_CircleName"] is DBNull ? "" : dt.Rows[i]["_CircleName"].ToString();
                        item.DenomID = dt.Columns.Contains("_DnomID") ? (dt.Rows[i]["_DnomID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DnomID"].ToString())) : 0;
                        item.DenomRangeID = dt.Columns.Contains("_DenomRangeID") ? (dt.Rows[i]["_DenomRangeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DenomRangeID"].ToString())) : 0;
                        item.Amount = dt.Rows[i]["_Amount"] is DBNull ? "" : dt.Rows[i]["_Amount"].ToString();
                        item.OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"].ToString());
                        item.Comm = dt.Rows[i]["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Comm"].ToString());
                        item.AmtType = dt.Rows[i]["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_AmtType"]);
                        item.CommType = dt.Rows[i]["_CommType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_CommType"]);
                        item.IsActive = dt.Rows[i]["_IsActive"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_IsActive"]);
                        item.DominationType = dt.Rows[i]["_DenominationType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DenominationType"].ToString());
                        item.ModificationDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "" : dt.Rows[i]["_ModifyDate"].ToString();
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetCircleWithDominationSpecialAPI";
    }

    public class ProcGetRemainDominationSpecialAPI : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetRemainDominationSpecialAPI(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            List<CircleWithDomination> res = new List<CircleWithDomination>();

            var req = (CircleWithDomination)obj;
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LT",req.LT),
                    new SqlParameter("@LoginID",req.LoginID),
                    new SqlParameter("@CircleID",req.CircleID),
                    new SqlParameter("@APIID",req.APIID),
                    new SqlParameter("@OID",req.OID),
                };
                var dt = await _dal.GetByProcedureAdapterAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new CircleWithDomination();
                        item.ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"].ToString());
                        item.Amount = dt.Rows[i]["_Amount"] is DBNull ? "" : dt.Rows[i]["_Amount"].ToString();
                        item.DominationType = dt.Rows[i]["_DenominationType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_DenominationType"].ToString());
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetRemainDominationSpecialAPI";
    }
    #endregion
}
