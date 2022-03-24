using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveDTHPackage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveDTHPackage(IDAL dal) => _dal = dal;
        public string GetName() => "proc_SaveDTHPackage";
        public object Call(object obj)
        {
            var req = (DTHPackageReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@PackageName",req.PackageName!=null?req.PackageName:""),
                new SqlParameter("@PackageMRP",req.PackageMRP),
                new SqlParameter("@BookingAmount",req.BookingAmount),
                new SqlParameter("@Validity",req.Validity),
                new SqlParameter("@Description",req.Description!=null?req.Description:""),
                new SqlParameter("@Remark",req.Remark!=null?req.Remark:""),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@FRC",req.FRC),
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
    }
}