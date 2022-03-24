using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetHotelDestination : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetHotelDestination(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {

                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TopRows",req.CommonInt),
                new SqlParameter("@KeyWords",req.CommonStr)
            };
            var res = new List<HotelDestination>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var HotelDestinationDetail = new HotelDestination
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            CityName = row["_CityName"] is DBNull ? "" : Convert.ToString(row["_CityName"]),
                            CountryName = row["_CountryName"] is DBNull ? "" : Convert.ToString(row["_CountryName"]),
                            CountryCode = row["_CountryCode"] is DBNull ? "" : Convert.ToString(row["_CountryCode"]),
                            DestinationID = row["_DestinationID"] is DBNull ? 0 : Convert.ToInt32(row["_DestinationID"]),
                            DestType = row["_DestType"] is DBNull ? short.Parse("0") : short.Parse(row["_DestType"].ToString()),
                            IsTop = row["_IsTop"] is DBNull ? false : Convert.ToBoolean(row["_IsTop"])


                        };
                        res.Add(HotelDestinationDetail);
                    }

                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetHotelDestination";
    }

    public class ProcGetHotelInfo : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetHotelInfo(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (TvkHotelInfoRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@CityId",req.CityId),
                new SqlParameter("@TBOHotelCodes",string.Join(',',req.TBOHotelCodes))
            };
            var res = new List<TBOBasicPropertyInfo>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables[0];
                var dtAttr = ds.Tables.Count > 1 ? ds.Tables[1] : new DataTable();

                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var ob = new TBOBasicPropertyInfo
                        {
                            HotelName = row["_HotelName"] is DBNull ? "" : row["_HotelName"].ToString(),
                            ImageUrl = row["_ImageUrl"] is DBNull ? "" : row["_ImageUrl"].ToString(),
                            Facility = row["_Facility"] is DBNull ? "" : row["_Facility"].ToString(),
                            TBOHotelCode = row["_TBOHotelCode"] is DBNull ? "" : row["_TBOHotelCode"].ToString(),
                            HotelCategoryName = row["_HotelCategoryName"] is DBNull ? "" : row["_HotelCategoryName"].ToString(),
                            Position = new TBOPosition
                            {
                                Latitude = row["_Latitude"] is DBNull ? "" : row["_Latitude"].ToString(),
                                Longitude = row["_Longitude"] is DBNull ? "" : row["_Longitude"].ToString(),
                            },
                            Award = new TBOAward
                            {
                                Rating = row["_Rating"] is DBNull ? "" : row["_Rating"].ToString(),
                                ReviewURL = row["_ReviewURL"] is DBNull ? "" : row["_ReviewURL"].ToString()
                            },
                            Policy = new TBOPolicy
                            {
                                CheckInTime = row["_CheckInTime"] is DBNull ? "" : row["_CheckInTime"].ToString(),
                                CheckOutTime = row["_CheckOutTime"] is DBNull ? "" : row["_CheckOutTime"].ToString()
                            },
                            VendorMessages = new TBOVendorMessages
                            {
                                VendorMessage = new TBOVendorMessage
                                {
                                    SubSection = new TBOSubSection
                                    {
                                        Paragraph = new TBOParagraph
                                        {
                                            Text = new TBOText
                                            {
                                                Text = row["_HotelText"] is DBNull ? "" : row["_HotelText"].ToString()
                                            }
                                        }
                                    }
                                }
                            },
                            Address = new TBOAddress
                            {
                                AddressLine = new List<string>
                                {
                                    row["_AddressLine1"] is DBNull ? "" : row["_AddressLine1"].ToString(),
                                    row["_AddressLine2"] is DBNull ? "" : row["_AddressLine2"].ToString()
                                },
                                CityName = row["_CityName"] is DBNull ? "" : row["_CityName"].ToString(),
                                PostalCode = row["_PostalCode"] is DBNull ? "" : row["_PostalCode"].ToString()
                            }
                        };

                        ob.Attributes = new TBOAttributes
                        {
                            Attribute = new List<TBOAttribute>()
                        };

                        if (dtAttr.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtAttr.Rows)
                            {
                                if ((r["_TBOHotelCode"] is DBNull ? "" : r["_TBOHotelCode"].ToString()) == ob.TBOHotelCode)
                                {
                                    var att = new TBOAttribute
                                    {
                                        AttributeName = r["_AttributeName"] is DBNull ? "" : r["_AttributeName"].ToString(),
                                        AttributeType = r["_AttributeType"] is DBNull ? "" : r["_AttributeType"].ToString(),
                                        TBOHotelCode = r["_TBOHotelCode"] is DBNull ? "" : r["_TBOHotelCode"].ToString()
                                    };

                                    ob.Attributes.Attribute.Add(att);
                                }                                
                            }
                        }

                        res.Add(ob);
                    }

                }
            }
            catch (Exception ex)
            {
            }
            return res;

        }

        public async Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetHotelInfo";
    }
    public class ProcGetHotelFacility : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetHotelFacility(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (string)obj;
            var res = new List<string>();
            SqlParameter[] param = {
                new SqlParameter("@HotelCode",req)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string _text = row["_text"] is DBNull ? "" : Convert.ToString(row["_text"]);
                        res.Add(_text);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_selectFacility";
    }
}
