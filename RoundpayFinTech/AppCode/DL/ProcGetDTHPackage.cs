using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDTHPackage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDTHPackage(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@ID", _req.CommonInt),
                new SqlParameter("@OID", _req.CommonInt2)
            };
            var _List = new List<DTHPackage>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var _Package = new DTHPackage
                    {
                        ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                        OID = Convert.ToInt32(dt.Rows[i]["_OID"]),
                        OPTypeID = Convert.ToInt32(dt.Rows[i]["_OPTypeID"]),
                        Operator = Convert.ToString(dt.Rows[i]["_Operator"]),
                        OpType = Convert.ToString(dt.Rows[i]["_OpType"]),
                        PackageMRP = Convert.ToInt32(dt.Rows[i]["_PackageMRP"]),
                        BookingAmount = dt.Rows[i]["_BookingAmount"] is DBNull?0:Convert.ToInt32(dt.Rows[i]["_BookingAmount"]),
                        FRC = dt.Rows[i]["_FRC"] is DBNull?0:Convert.ToInt32(dt.Rows[i]["_FRC"]),
                        ChannelCount = dt.Rows[i]["_ChannelCount"] is DBNull?0:Convert.ToInt32(dt.Rows[i]["_ChannelCount"]),
                        Validity = Convert.ToInt32(dt.Rows[i]["_Validity"]),
                        PackageName = Convert.ToString(dt.Rows[i]["_PackageName"]),
                        Description = Convert.ToString(dt.Rows[i]["_Description"]),
                        Remark = Convert.ToString(dt.Rows[i]["_Remark"]),
                        IsActive = dt.Rows[i]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsActive"]),
                        Comm = dt.Rows[i]["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Comm"])
                    };
                    _List.Add(_Package);
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
            return _List;
        }

        public object Call()=> throw new NotImplementedException();

        public string GetName() => "proc_GetDTHPackage";
    }
}
