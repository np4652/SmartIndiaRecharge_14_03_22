using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fintech.AppCode.StaticModel;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetAreaByPincode : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetAreaByPincode(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@PinCode",req.CommonInt)
            };
            var res = new List<ShoppingPincodeDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var item = new ShoppingPincodeDetail
                        {
                            Statuscode = ErrorCodes.One,
                            Msg = ErrorCodes.SUCCESS,
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Pincode = row["_Pincode"] is DBNull ? "" : row["_Pincode"].ToString(),
                            Area = row["_Area"] is DBNull ? "" : row["_Area"].ToString(),
                            ReachInHour = row["_ReachInHour"] is DBNull ? 0 : Convert.ToInt32(row["_ReachInHour"]),
                            ExpectedDeliverInDays = row["_ExpectedDeliverInDays"] is DBNull ? 0 : Convert.ToInt32(row["_ExpectedDeliverInDays"]),
                            IsDeliveryOff = row["_IsDeliveryOff"] is DBNull ? false : Convert.ToBoolean(row["_IsDeliveryOff"]),
                            City = row["city"] is DBNull ? "" : Convert.ToString(row["city"]),
                            Districtname = row["DistrictName"] is DBNull ? "" : Convert.ToString(row["DistrictName"]),
                            Statename = row["Statename"] is DBNull ? "" : Convert.ToString(row["Statename"]),
                            StateId = row["StateId"] is DBNull ? 0 : Convert.ToInt32(row["StateId"]),
                            CityId = row["CityId"] is DBNull ? 0 : Convert.ToInt32(row["CityId"])
                        };
                        res.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetPincodeArea";
    }
}
