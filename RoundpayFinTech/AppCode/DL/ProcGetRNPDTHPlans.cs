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
    public class ProcGetRNPDTHPlans : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRNPDTHPlans(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.UserID),
                new SqlParameter("@OID",req.CommonInt)
            };
            var res = new RNPDTHPlans();
            var responses = new List<RNPDTHPlansResponse>();
            var package = new List<RNPDTHPlansPackage>();
            var language = new List<RNPDTHPlansLanguages>();

            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    var dtResp = ds.Tables[0];
                    if (dtResp.Rows.Count > 0)
                    {
                        foreach (DataRow item in dtResp.Rows)
                        {
                            responses.Add(new RNPDTHPlansResponse {
                                RechargeValidity = item["_RechargeValidity"] is DBNull ? string.Empty : item["_RechargeValidity"].ToString(),
                                RechargeType = item["_RechargeType"] is DBNull ? string.Empty : item["_RechargeType"].ToString(),
                                PackageId = item["_PackageId"] is DBNull ? 0 : Convert.ToInt32(item["_PackageId"]),
                                details = item["_PackageDescription"] is DBNull ? string.Empty : item["_PackageDescription"].ToString(),
                                rechargeAmount = item["_PackagePrice"] is DBNull ? string.Empty : item["_PackagePrice"].ToString(),
                                opName = item["_OpName"] is DBNull ? string.Empty : item["_OpName"].ToString(),
                                Channelcount = item["_ChannelCount"] is DBNull ? 0 : Convert.ToInt32(item["_ChannelCount"]),
                                EntryDate = Convert.ToDateTime( item["_EntryDate"])
                            });
                        }
                        res.Response = responses;
                    }
                    if (ds.Tables.Count > 1)
                    {
                        DataTable dtPack = ds.Tables[1];
                        foreach (DataRow item in dtPack.Rows)
                        {
                            package.Add(new RNPDTHPlansPackage
                            {
                                PackageId = item["_PackageId"] is DBNull ? 0 : Convert.ToInt32(item["_PackageId"]),
                                packagelanguage = item["_PackageLanguage"] is DBNull ? string.Empty : item["_PackageLanguage"].ToString(),
                                Channelcount = item["_ChannelCount"] is DBNull ? 0 : Convert.ToInt32(item["_ChannelCount"]),
                                RechargeValidity = item["_RechargeValidity"] is DBNull ? string.Empty : item["_RechargeValidity"].ToString(),
                                RechargeType = item["_RechargeType"] is DBNull ? string.Empty : item["_RechargeType"].ToString(),
                                details = item["_PackageDescription"] is DBNull ? string.Empty : item["_PackageDescription"].ToString(),
                                rechargeAmount = item["_PackagePrice"] is DBNull ? string.Empty : item["_PackagePrice"].ToString(),
                                opName = item["_OpName"] is DBNull ? string.Empty : item["_OpName"].ToString(),
                            });
                        }
                        res.Package = package;
                    }
                    if (ds.Tables.Count > 2)
                    {
                        DataTable dtLang = ds.Tables[2];
                        foreach (DataRow item in dtLang.Rows)
                        {
                            language.Add(new RNPDTHPlansLanguages
                            {
                                Opname = item["_OpName"] is DBNull ? string.Empty : item["_OpName"].ToString(),
                                Language = item["_PackageLanguage"] is DBNull ? string.Empty : item["_PackageLanguage"].ToString(),
                                PackageCount = item["_PackageCount"] is DBNull ? 0 : Convert.ToInt32(item["_PackageCount"])
                            });
                        }
                        res.Language = language;
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetRNPDTHPlans";
    }
}
