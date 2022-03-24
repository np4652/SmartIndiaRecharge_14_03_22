using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetTertiaryReportForEmp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetTertiaryReportForEmp(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@EmpID", _req.CommonInt),
                new SqlParameter("@DT", _req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy"))
            };
            var _alist = new List<TertiaryReport>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow dr in dt.Rows)
                {
                    _alist.Add(new TertiaryReport
                    {
                        Statuscode = ErrorCodes.One,
                        Msg = "Success",
                        //ServiceID = Convert.ToInt32(dr["_ServiceID"]),
                        //OPTypeID = Convert.ToInt32(dr["_OPTypeID"]),
                        UserID = Convert.ToInt32(dr["_UserID"]),
                        User = Convert.ToString(dr["_User"]),
                        UserMobile = Convert.ToString(dr["_UserMobile"]),
                        URoleID = Convert.ToInt32(dr["_URoleID"]),
                        URole = Convert.ToString(dr["_URole"]),
                        AM = Convert.ToString(dr["_AM"]),
                        AMRoleID = Convert.ToInt32(dr["_AMRoleID"]),
                        AMRole = Convert.ToString(dr["_AMRole"]),
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
                        //Prepaid
                        LMPrepaid = Convert.ToString(dr["_LMPrepaid"]),
                        LMTDPrepaid = Convert.ToString(dr["_LMTDPrepaid"]),
                        MTDPrepaid = Convert.ToString(dr["_MTDPrepaid"]),
                        TOLMPrepaid = Convert.ToString(dr["_TOLMPrepaid"]),
                        TOLMTDPrepaid = Convert.ToString(dr["_TOLMTDPrepaid"]),
                        TOMTDPrepaid = Convert.ToString(dr["_TOMTDPrepaid"]),
                        //DMT
                        LMDMT = Convert.ToString(dr["_LMDMT"]),
                        LMTDDMT = Convert.ToString(dr["_LMTDDMT"]),
                        MTDDMT = Convert.ToString(dr["_MTDDMT"]),
                        TOLMDMT = Convert.ToString(dr["_TOLMDMT"]),
                        TOLMTDDMT = Convert.ToString(dr["_TOLMTDDMT"]),
                        TOMTDDMT = Convert.ToString(dr["_TOMTDDMT"]),
                        //BBPS
                        LMBBPS = Convert.ToString(dr["_LMBBPS"]),
                        LMTDBBPS= Convert.ToString(dr["_LMTDBBPS"]),
                        MTDBBPS = Convert.ToString(dr["_MTDBBPS"]),
                        TOLMBBPS = Convert.ToString(dr["_TOLMBBPS"]),
                        TOLMTDBBPS = Convert.ToString(dr["_TOLMTDBBPS"]),
                        TOMTDBBPS = Convert.ToString(dr["_TOMTDBBPS"]),
                        //AEPS
                        LMAEPS = Convert.ToString(dr["_LMAEPS"]),
                        LMTDAEPS = Convert.ToString(dr["_LMTDAEPS"]),
                        MTDAEPS = Convert.ToString(dr["_MTDAEPS"]),
                        TOLMAEPS = Convert.ToString(dr["_TOLMAEPS"]),
                        TOLMTDAEPS = Convert.ToString(dr["_TOLMTDAEPS"]),
                        TOMTDAEPS = Convert.ToString(dr["_TOMTDAEPS"]),
                        EntryDate = Convert.ToString(dr["_EntryDate"]),
                        TransactionDate = Convert.ToString(dr["_TransactionDate"]),
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

        public string GetName() => "proc_GetTertiaryReportForEmp";
    }
}
