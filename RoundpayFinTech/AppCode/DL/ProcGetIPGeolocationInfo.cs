using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetIPGeolocationInfo : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetIPGeolocationInfo(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (string)obj;
            SqlParameter[] param = {
                new SqlParameter("@ip",req)
            };
            IPGeolocationInfo res = new IPGeolocationInfo();
            try
            {
                var dt = await _dal.GetAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    res = new IPGeolocationInfo
                    {
                        ip = Convert.ToString(dr["_IP"]),
                        type = Convert.ToString(dr["_Type"]),
                        continent_name = Convert.ToString(dr["_continent_name"]),
                        country_name = Convert.ToString(dr["_country_name"]),
                        region_name = Convert.ToString(dr["_region_name"]),
                        city = Convert.ToString(dr["_city"]),
                        zip = Convert.ToString(dr["_zip"]),
                        latitude = dr["_latitude"] is DBNull ? 0 : Convert.ToDouble(dr["_latitude"]),
                        longitude = dr["_longitude"] is DBNull ? 0 : Convert.ToDouble(dr["_longitude"]),
                    };
                }
            }
            catch (Exception ex)
            {

                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return res;
        }
        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "select * from tbl_IPGeoLocation(nolock) where _IP=@ip";
    }
}
