using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcProceedToPay : IProcedure
    {
        private readonly IDAL _dal;
        public ProcProceedToPay(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            var res = new ProceedToPay
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID)
            };
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        res.Quantity = dt.Rows[0]["TotalQuantity"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TotalQuantity"]);
                        res.TotalCost = dt.Rows[0]["TotalCost"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["TotalCost"]);
                        res.TotalMRP = dt.Rows[0]["TotalMRP"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["TotalMRP"]);
                        res.TotalDiscount = dt.Rows[0]["TotalDiscount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["TotalDiscount"]);
                        res.TotalSellingPrice = dt.Rows[0]["TotalSellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["TotalSellingPrice"]);
                        res.PDeduction = dt.Rows[0]["PDeduction"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["PDeduction"]);
                        res.SDeduction = dt.Rows[0]["SDeduction"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["SDeduction"]);
                        res.ShippingCharge = dt.Rows[0]["ShippingCharge"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ShippingCharge"]);
                        res.PWallet =  Convert.ToString(dt.Rows[0]["PWallet"]);
                        res.SWallet = Convert.ToString(dt.Rows[0]["SWallet"]);
                    }
                    dt = ds.Tables[1];
                    if (dt.Rows.Count > 0)
                    {
                        var AdddressList = new List<ShippingAddress>();
                        foreach(DataRow dr in dt.Rows)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("{Address} ,City : {City},PIN : {PIN},Mobile : {Mobile},<br/>LandMark : {LandMark}");
                            sb.Replace("{Address}", Convert.ToString(dr["_Address"]));
                            sb.Replace("{City}", Convert.ToString(dr["_City"]));
                            sb.Replace("{PIN}", Convert.ToString(dr["_PIN"]));
                            sb.Replace("{Mobile}", Convert.ToString(dr["_MobileNo"]));
                            sb.Replace("{LandMark}", Convert.ToString(dr["_LandMark"]));
                            AdddressList.Add(new ShippingAddress
                            {
                                CustomerName=Convert.ToString(dr["_CustomerName"]),
                                Title=Convert.ToString(dr["_Title"]),
                                City= Convert.ToString(dr["_City"]),
                                AddressOnly = Convert.ToString(dr["_Address"]),
                                PinCode=Convert.ToString(dr["_PIN"]),
                                Landmark = Convert.ToString(dr["_Landmark"]),
                                MobileNo = Convert.ToString(dr["_MobileNo"]),
                                ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                Address = sb.ToString(),
                                IsDefault = dr["_IsDefault"] is DBNull ? false : Convert.ToBoolean(dr["_IsDefault"]),
                                
                            });
                        }
                        res.Addresses = AdddressList;
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
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_ProceedToPay";
    }
}