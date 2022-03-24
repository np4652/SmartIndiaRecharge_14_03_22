using System;
using System.Data;
using System.Data.SqlClient;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcEmpCreate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEmpCreate(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            EmpDetailRequest _req = (EmpDetailRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@EmpID", _req.empInfo.EmpID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LTID", _req.LTID),
                new SqlParameter("@EmpRoleID", _req.empInfo.RoleID),
                new SqlParameter("@Name", _req.empInfo.Name),
                new SqlParameter("@MobileNo", _req.empInfo.MobileNo),
                new SqlParameter("@EmailID", _req.empInfo.EmailID??""),
                new SqlParameter("@EmpCode", _req.empInfo.EmpCode),
                new SqlParameter("@Pincode", _req.empInfo.Pincode),
                new SqlParameter("@ReferalID", _req.ReferalID),
                new SqlParameter("@Aadhar", _req.empInfo.Aadhar??""),
                new SqlParameter("@PAN", _req.empInfo.PAN??""),
                new SqlParameter("@Address", _req.empInfo.Address??""),                
                new SqlParameter("@Password", HashEncryption.O.Encrypt(_req.Password)),
                new SqlParameter("@Browser", _req.empInfo.Pincode),
                new SqlParameter("IP", _req.IP),
                new SqlParameter("@RequestModeID", _req.empInfo.RequestModeID)
            };
            ResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {                   
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if(_resp.Statuscode!= ErrorCodes.Minus1)
                        _resp.CommonStr = dt.Rows[0]["NewEmpID"].ToString();
                   
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error=ex.Message,
                    LoginTypeID=_req.LTID,
                    UserId=_req.LoginID
                };
               var _= new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_CreateEmp";
    }
}
