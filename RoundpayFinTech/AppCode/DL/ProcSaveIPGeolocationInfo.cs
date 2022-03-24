using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveIPGeolocationInfo : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcSaveIPGeolocationInfo(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (IPGeolocationInfo)obj;
            SqlParameter[] param = {
                new SqlParameter("@ip",req.ip),
                new SqlParameter("@type",req.type??string.Empty),
                new SqlParameter("@continentname",req.continent_name??string.Empty),
                new SqlParameter("@countryname",req.country_name??string.Empty),
                new SqlParameter("@regionname",req.region_name??string.Empty),
                new SqlParameter("@city",req.city??string.Empty),
                new SqlParameter("@zip",req.zip??string.Empty),
                new SqlParameter("@latitude",req.latitude),
                new SqlParameter("@longitude",req.longitude),
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {

                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call" + _dal.GetDBNameFromIntialCatelog(),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return res;
        }
        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_SaveIPGeolocationInfo";
    }
}
