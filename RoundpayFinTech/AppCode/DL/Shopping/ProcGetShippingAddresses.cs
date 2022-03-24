using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetShippingAddresses : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetShippingAddresses(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            var AdddressList = new List<ShippingAddress>();
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                  
                    foreach (DataRow dr in dt.Rows)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{Address} ,City : {City},PIN : {PIN},Mobile : {Mobile},<br/>LandMark : {LandMark}");
                        sb.Replace("{Address}", Convert.ToString(dr["_Address"]));
                        sb.Replace("{City}", Convert.ToString(dr["City"]));
                        sb.Replace("{PIN}", Convert.ToString(dr["_PIN"]));
                        sb.Replace("{Mobile}", Convert.ToString(dr["_MobileNo"]));
                        sb.Replace("{LandMark}", Convert.ToString(dr["_LandMark"]));
                        AdddressList.Add(new ShippingAddress
                        {
                            ID = dr["_id"] is DBNull ? 0 : Convert.ToInt32(dr["_id"]),
                            UserId = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            AddressOnly = Convert.ToString(dr["_Address"]),
                            PinCode = Convert.ToString(dr["_PIN"]),
                            Landmark = Convert.ToString(dr["_Landmark"]),
                            MobileNo = Convert.ToString(dr["_MobileNo"]),
                            CityId = dr["_CityID"] is DBNull ? 0 : Convert.ToInt32(dr["_CityID"]),
                            CustomerName = Convert.ToString(dr["_CustomerName"]),
                            Title = Convert.ToString(dr["_Title"]),
                            StateId = dr["_StateID"] is DBNull ? 0 : Convert.ToInt32(dr["_StateID"]),
                            IsDefault = dr["_IsDefault"] is DBNull ? false : Convert.ToBoolean(dr["_IsDefault"]),
                            Area = dr["_Area"] is DBNull ? "" : Convert.ToString(dr["_Area"]),
                            City = Convert.ToString(dr["City"]),
                            State = Convert.ToString(dr["Statename"]),
                            Address = sb.ToString(),
                            Latitude = dr["_Lat"] is DBNull ? "": Convert.ToString(dr["_Lat"]),
                            Longitude = dr["_Long"] is DBNull ? "" : Convert.ToString(dr["_Long"])
                        });
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
                    LoginTypeID = 1,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return AdddressList;
        }

        public object Call() => throw new NotImplementedException();

        //public string GetName() => @"select a.*, p.City, p.Statename  from tbl_ShippingAddress a left outer join tbl_Pincode p on p.Pincode = a._PIN 
        //                                where isnull(a._IsDeleted,0) = 0 and a._UserID= @LoginID";
        public string GetName() => @"select a.*, MC._City City, MS.Statename  from tbl_ShippingAddress a 
										inner join  Master_State MS on  MS._ID=a._StateID
										inner join Master_City MC on MC._ID=a._CityID 
                                        where isnull(a._IsDeleted,0) = 0 and a._UserID= @LoginID";
    }
}