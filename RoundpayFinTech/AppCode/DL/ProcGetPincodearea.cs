using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPincodearea : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPincodearea(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@PinCode",req.CommonInt2)
            };
            var res = new List<PincodeDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    
                        foreach (DataRow row in dt.Rows)
                        {
                            var operatorDetail = new PincodeDetail
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                Pincode = row["_Pincode"] is DBNull ? "" : row["_Pincode"].ToString(),
                                Area = row["Area"] is DBNull ? "" : row["Area"].ToString(),
                                ReachInHour = row["_ReachInHour"] is DBNull ? 0 : Convert.ToInt32(row["_ReachInHour"]),
                                ExpectedDeliverInDays = row["_ExpectedDeliverInDays"] is DBNull ? 0 : Convert.ToInt32(row["_ExpectedDeliverInDays"]),
                                IsDeliveryOff = row["_IsDeliveryOff"] is DBNull ? false : Convert.ToBoolean(row["_IsDeliveryOff"]),
                                City = row["city"] is DBNull ? "" : Convert.ToString(row["city"]),
                                Districtname = row["DistrictName"] is DBNull ? "" : Convert.ToString(row["DistrictName"]),
                                Statename = row["Statename"] is DBNull ? string.Empty : Convert.ToString(row["Statename"]),
                                Lat = row["_lat"] is DBNull ? string.Empty : Convert.ToString(row["_lat"]),
                                Long = row["_long"] is DBNull ? string.Empty : Convert.ToString(row["_long"]),
                            };                           
                            res.Add(operatorDetail);
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
