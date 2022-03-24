using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetEmpTargetReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetEmpTargetReport(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@EmpID", _req.CommonInt),
                new SqlParameter("@DT", _req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy"))
            };
            var _alist = new List<EmployeeTargetReport>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow dr in dt.Rows)
                {
                    _alist.Add(new EmployeeTargetReport
                    {
                        OID = Convert.ToInt32(dr["_OID"]),
                        AMRoleID = Convert.ToInt32(dr["_AMRoleID"]), 
                        AM = Convert.ToString(dr["_AM"]),
                        SHDetail = Convert.ToString(dr["_SHDetail"]),
                        CDetail = Convert.ToString(dr["_CDetail"]),
                        ZDetail = Convert.ToString(dr["_ZDetail"]),
                        ADetail = Convert.ToString(dr["_ADetail"]),
                        //TSMID = Convert.ToInt32(dr["_TSMID"]),
                        TSMDetail = Convert.ToString(dr["_TSMDetail"]),
                        //UserDetail = Convert.ToString(dr["_UserDetail"]),
                        //Primary
                        //LMPrimary = Convert.ToString(dr["_LMPrimary"]),
                        //LMTDPrimary = Convert.ToString(dr["_LMTDPrimary"]),
                        //MTDPrimary = Convert.ToString(dr["_MTDPrimary"]),
                        //TargetPrimary = Convert.ToString(dr["_TargetPrimary"]),

                        //Prepaid
                        LMPrepaid = Convert.ToString(dr["_LMPrepaid"]),
                        LMTDPrepaid = Convert.ToString(dr["_LMTDPrepaid"]),
                        MTDPrepaid = Convert.ToString(dr["_MTDPrepaid"]),
                        TOLMPrepaid = Convert.ToString(dr["_TOLMPrepaid"]),
                        TOLMTDPrepaid = Convert.ToString(dr["_TOLMTDPrepaid"]),
                        TOMTDPrepaid = Convert.ToString(dr["_TOMTDPrepaid"]),
                        TargetPrepaid = Convert.ToString(dr["_TargetPrepaid"]),
                        //DMT
                        LMDMT = Convert.ToString(dr["_LMDMT"]),
                        LMTDDMT = Convert.ToString(dr["_LMTDDMT"]),
                        MTDDMT = Convert.ToString(dr["_MTDDMT"]),
                        TOLMDMT = Convert.ToString(dr["_TOLMDMT"]),
                        TOLMTDDMT = Convert.ToString(dr["_TOLMTDDMT"]),
                        TOMTDDMT = Convert.ToString(dr["_TOMTDDMT"]),
                        TargetDMT = Convert.ToString(dr["_TargetDMT"]),
                        //BBPS
                        LMBBPS = Convert.ToString(dr["_LMBBPS"]),
                        LMTDBBPS = Convert.ToString(dr["_LMTDBBPS"]),
                        MTDBBPS = Convert.ToString(dr["_MTDBBPS"]),
                        TOLMBBPS = Convert.ToString(dr["_TOLMBBPS"]),
                        TOLMTDBBPS = Convert.ToString(dr["_TOLMTDBBPS"]),
                        TOMTDBBPS = Convert.ToString(dr["_TOMTDBBPS"]),
                        TargetBBPS = Convert.ToString(dr["_TargetBBPS"]),
                        //AEPS
                        LMAEPS = Convert.ToString(dr["_LMAEPS"]),
                        LMTDAEPS = Convert.ToString(dr["_LMTDAEPS"]),
                        MTDAEPS = Convert.ToString(dr["_MTDAEPS"]),
                        TOLMAEPS = Convert.ToString(dr["_TOLMAEPS"]),
                        TOLMTDAEPS = Convert.ToString(dr["_TOLMTDAEPS"]),
                        TOMTDAEPS = Convert.ToString(dr["_TOMTDAEPS"]),
                        TargetAEPS = Convert.ToString(dr["_TargetAEPS"]),

                        //TransactionDate = Convert.ToString(dr["_TransactionDate"]),
                        //EntryDate = Convert.ToString(dr["_EntryDate"]),
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetEmpTargetReport";
    }
}
