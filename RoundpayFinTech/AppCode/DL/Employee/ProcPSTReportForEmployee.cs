using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcPSTReportForEmployee : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPSTReportForEmployee(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Date", _req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@RoleID", _req.RoleID)
            };
            var _alist = new List<PSTReportEmp>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    var _PGrowth = 0.0m;
                    var _SGrowth = 0.0m;
                    var _TGrowth = 0.0m;
                    var _OGrowth = 0.0m;
                    var _PackageGrowth = 0.0m;
                    _PGrowth = Convert.ToDecimal(dr["_Pri"]) > 0 ? ((Convert.ToDecimal(dr["_Pri"]) - Convert.ToDecimal(dr["_PriLMTD"])) / Convert.ToDecimal(dr["_Pri"])) * 100 : ((Convert.ToDecimal(dr["_Pri"]) - Convert.ToDecimal(dr["_PriLMTD"])) / 1) * 100;

                    _SGrowth = Convert.ToDecimal(dr["_Sec"]) > 0 ? ((Convert.ToDecimal(dr["_Sec"]) - Convert.ToDecimal(dr["_SecLMTD"])) / Convert.ToDecimal(dr["_Sec"])) * 100 : ((Convert.ToDecimal(dr["_Sec"]) - Convert.ToDecimal(dr["_SecLMTD"])) / 1) * 100;

                    _TGrowth = Convert.ToDecimal(dr["_Ter"]) > 0 ? ((Convert.ToDecimal(dr["_Ter"]) - Convert.ToDecimal(dr["_TerLMTD"])) / Convert.ToDecimal(dr["_Ter"])) * 100 : ((Convert.ToDecimal(dr["_Ter"]) - Convert.ToDecimal(dr["_TerLMTD"])) / 1) * 100;

                    _OGrowth = Convert.ToDecimal(dr["_TOutlet"]) > 0 ? ((Convert.ToDecimal(dr["_TOutlet"]) - Convert.ToDecimal(dr["_TOutletLMTD"])) / Convert.ToDecimal(dr["_TOutlet"])) * 100 : ((Convert.ToDecimal(dr["_TOutlet"]) - Convert.ToDecimal(dr["_TOutletLMTD"])) / 1) * 100;

                    _PackageGrowth = Convert.ToDecimal(dr["_PackageSell"]) > 0 ? ((Convert.ToDecimal(dr["_PackageSell"]) - Convert.ToDecimal(dr["_PackageSellLMTD"])) / Convert.ToDecimal(dr["_PackageSell"])) * 100 : ((Convert.ToDecimal(dr["_PackageSell"]) - Convert.ToDecimal(dr["_PackageSellLMTD"])) / 1) * 100;


                    _alist.Add(new PSTReportEmp
                    {
                        Statuscode = ErrorCodes.One,
                        Msg = "Success",
                        UserID = Convert.ToInt32(dr["_UserID"]),
                        User = Convert.ToString(dr["_User"]),
                        UserMobile = Convert.ToString(dr["_UserMobile"]),
                        URoleID = Convert.ToInt32(dr["_URoleID"]),
                        PriLM = Convert.ToDecimal(dr["_PriLM"]),
                        PriLMTD = Convert.ToDecimal(dr["_PriLMTD"]),
                        Pri = Convert.ToDecimal(dr["_Pri"]),
                        PGrowth = _PGrowth,
                        Sec = Convert.ToDecimal(dr["_Sec"]),
                        SecLM = Convert.ToDecimal(dr["_SecLM"]),
                        SecLMTD = Convert.ToDecimal(dr["_SecLMTD"]),
                        SGrowth = _SGrowth,
                        Ter = Convert.ToDecimal(dr["_Ter"]),
                        TerLM = Convert.ToDecimal(dr["_TerLM"]),
                        TerLMTD = Convert.ToDecimal(dr["_TerLMTD"]),
                        TGrowth = _TGrowth,
                        TOutlet = Convert.ToInt32(dr["_TOutlet"]),
                        TOutletLM = Convert.ToInt32(dr["_TOutletLM"]),
                        TOutletLMTD = Convert.ToInt32(dr["_TOutletLMTD"]),
                        TNewOutlet = Convert.ToInt32(dr["_TNewOutlet"]),
                        TNewOutletLM = Convert.ToInt32(dr["_TNewOutletLM"]),
                        TNewOutletLMTD = Convert.ToInt32(dr["_TNewOutletLMTD"]),
                        OGrowth = _OGrowth,
                        PackageSell = Convert.ToDecimal(dr["_PackageSell"]),
                        PackageSellLM = Convert.ToDecimal(dr["_PackageSellLM"]),
                        PackageSellLMTD = Convert.ToDecimal(dr["_PackageSellLMTD"]),
                        PackageGrowth = _PackageGrowth,
                        AMID = Convert.ToInt32(dr["_AMID"]),
                        AM = Convert.ToString(dr["_AM"]),
                        AMRoleID = Convert.ToInt32(dr["_AMRoleID"]),
                        SHID = Convert.ToInt32(dr["_SHID"]),
                        SHDetail = Convert.ToString(dr["_SHDetail"]),
                        CHID = Convert.ToInt32(dr["_CHID"]),
                        CDetail = Convert.ToString(dr["_CDetail"]),
                        ZID = Convert.ToInt32(dr["_ZID"]),
                        ZDetail = Convert.ToString(dr["_ZDetail"]),
                        AID = Convert.ToInt32(dr["_AID"]),
                        ADetail = Convert.ToString(dr["_ADetail"]),
                        TSMID = Convert.ToInt32(dr["_TSMID"]),
                        TSMDetail = Convert.ToString(dr["_TSMDetail"]),
                        EntryDate = Convert.ToString(dr["_EntryDate"]),
                        AMMobileNo = Convert.ToString(dr["_AMMobileNo"])
                    });
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_PSTReportForEmployee";
    }
}
