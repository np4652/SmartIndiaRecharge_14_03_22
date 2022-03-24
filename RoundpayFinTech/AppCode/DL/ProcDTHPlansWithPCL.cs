using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDTHPlansWithPCL : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDTHPlansWithPCL(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.CommonInt)
            };

            var _res = new List<DTHPlanRespDB>();
            
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        _res.Add(new DTHPlanRespDB
                        {
                            _PackageName = dr["_PackageName"] is DBNull ? string.Empty : dr["_PackageName"].ToString(),
                            _PackagePrice = dr["_PackagePrice"] is DBNull ? string.Empty : dr["_PackagePrice"].ToString(),
                            _PackagePrice_3 = dr["_PackagePrice_3"] is DBNull ? string.Empty : dr["_PackagePrice_3"].ToString(),
                            _PackagePrice_6 = dr["_PackagePrice_6"] is DBNull ? string.Empty : dr["_PackagePrice_6"].ToString(),
                            _PackagePrice_12 = dr["_PackagePrice_12"] is DBNull ? string.Empty : dr["_PackagePrice_12"].ToString(),
                            _PackageDescription = dr["_PackageDescription"] is DBNull ? string.Empty : dr["_PackageDescription"].ToString(),
                            _PackageType = dr["_PackageType"] is DBNull ? string.Empty : dr["_PackageType"].ToString(),
                            _PackageLanguage = dr["_PackageLanguage"] is DBNull ? string.Empty : dr["_PackageLanguage"].ToString(),
                            _PackageId = dr["_PackageId"] is DBNull ? string.Empty : dr["_PackageId"].ToString(),
                            _pChannelCount = dr["_pChannelCount"] is DBNull ? string.Empty : dr["_pChannelCount"].ToString(),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_DTHPlansWithPCL";
    }


    public class ProcDTHChannelByPID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDTHChannelByPID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@PID", _req.CommonInt)
            };
            var _res = new List<DTHChnlRespDB>();
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        _res.Add(new DTHChnlRespDB
                        {
                            name = dr["_ChName"] is DBNull ? string.Empty : dr["_ChName"].ToString(),
                            genre = dr["_Genre"] is DBNull ? string.Empty : dr["_Genre"].ToString(),
                            logo = dr["_LogoURL"] is DBNull ? string.Empty : dr["_LogoURL"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "select _ChName,_Genre,_LogoURL from tbl_DTHChannelList where _PackageID=@PID";
    }
}
