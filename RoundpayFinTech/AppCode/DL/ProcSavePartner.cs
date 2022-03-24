using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSavePartner : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSavePartner(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (PartnerCreate)obj;
            SqlParameter[] param = {
                new SqlParameter("@Name", _req.Name??string.Empty),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@FatherName", _req.FatherName??string.Empty),
                new SqlParameter("@DOB", _req.DOB),
                new SqlParameter("@OutletName", _req.OutletName??string.Empty),
                new SqlParameter("@MobileNo", _req.MobileNo??string.Empty),
                new SqlParameter("@EmailID", _req.EmailID??string.Empty),
                new SqlParameter("@PAN", _req.PAN??string.Empty),
                new SqlParameter("@AADHAR", _req.AADHAR??string.Empty),
                new SqlParameter("@CompanyPAN", _req.CompanyPAN??string.Empty),
                new SqlParameter("@GSTIN", _req.GSTIN??string.Empty),
                new SqlParameter("@AuthPersonName", _req.AuthPersonName??string.Empty),
                new SqlParameter("@AuthPersonAADHAR", _req.AuthPersonAADHAR??string.Empty),
                new SqlParameter("@CurrentAccountNo", _req.CurrentAccountNo??string.Empty),
                new SqlParameter("@Address", _req.Address??string.Empty),
                new SqlParameter("@Block", _req.Block??string.Empty),
                new SqlParameter("@District", _req.District??string.Empty),
                new SqlParameter("@Pincode", _req.Pincode??string.Empty),
                new SqlParameter("@Banner", _req.Banner??string.Empty)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_resp.Statuscode == ErrorCodes.One)
                    {
                        _resp.CommonInt = Convert.ToInt32(dt.Rows[0]["PartnerID"] is DBNull ? 0 : dt.Rows[0]["PartnerID"]);
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
                    LoginTypeID = 0,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_SavePartner";
    }
}
