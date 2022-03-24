using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateMenuOperations : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateMenuOperations(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            MenuOperation _req = (MenuOperation)obj;
            SqlParameter[] param = {
                new SqlParameter("@DevKey", HashEncryption.O.DevEncrypt(_req.DevKey??"")),
                new SqlParameter("@MenuID", _req.MenuID),
                new SqlParameter("@OperationID", _req.OperationID),
                new SqlParameter("@IsActive", _req.IsActive)
            };
            ResponseStatus _res = new ResponseStatus
            {
               Statuscode=ErrorCodes.Minus1,
               Msg=ErrorCodes.TempError
           };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception)
            { }
            return _res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UpdateMenuOperations";
        }
    }
}