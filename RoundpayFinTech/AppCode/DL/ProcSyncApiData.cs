using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSyncApiData : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSyncApiData(IDAL dal)
        {
            _dal = dal;
        }

        public object Call(object obj)
        {
            DestinationSearchStaticDataResult req = (DestinationSearchStaticDataResult)obj;
            var response = new List<TekTvlDestinations>();

            DataTable TP_HotelDestination = ConverterHelper.O.ToDataTable(req.Destinations);
            SqlParameter[] param = {
                new SqlParameter("@ID",0),
                new SqlParameter("@TP_HotelDestination",TP_HotelDestination)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        response.Add(new TekTvlDestinations
                        {
                            DestinationId = dr["_DestinationID"] is DBNull ? 0 : Convert.ToInt32(dr["_DestinationID"]),
                            CityName = dr["_CityName"] is DBNull ? string.Empty : Convert.ToString(dr["_CityName"]),
                            CountryCode = dr["_CountryCode"] is DBNull ? string.Empty : Convert.ToString(dr["_CountryCode"]),
                            CountryName = dr["_CountryName"] is DBNull ? string.Empty : Convert.ToString(dr["_CountryName"]),
                        });
                    }
                }
            }
            catch (Exception er)
            {

            }
            return response;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_SyncApiData";
        }
    }

    public class ProcSyncApiHotelData : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSyncApiHotelData(IDAL dal)
        {
            _dal = dal;
        }

        public object Call(object obj)
        {
            var req = (TvkHotelSyncProcRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@xmlTBOBasicPropertyInfo",req.xmlTBOBasicPropertyInfo),
                new SqlParameter("@xmlTBOAttribute",req.xmlTBOAttribute),
                new SqlParameter("@cityId",req.cityId)
            };
            var resp = new TekTvlErrorModel();
            resp.ErrorCode = ErrorCodes.Minus1.ToString();
            resp.ErrorMessage = ErrorCodes.AnError;
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.ErrorCode = Convert.ToString(dt.Rows[0][0]);
                    resp.ErrorMessage = dt.Rows[0]["Msg"].ToString();
                    if (Convert.ToInt32(resp.ErrorCode) == ErrorCodes.One)
                    {
                        //resp.CommonStr = dt.Rows[0]["Tid"].ToString();
                    }
                }
            }
            catch (Exception er)
            {
                resp.ErrorCode = ErrorCodes.Minus1.ToString();
                resp.ErrorMessage = ErrorCodes.AnError;
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_SyncApiHotelData";
        }
    }


    public class ProcSyncApiHotelDataByHotelCode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSyncApiHotelDataByHotelCode(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (TvkHotelSyncProcRequestByHotelCode)obj;
            SqlParameter[] param = {
                 new SqlParameter("@xmlTBOImages",req.xmlTBOImage),
                 new SqlParameter("@xmlTBOFacility",req.xmlTBOFacility),
                new SqlParameter("@cityId",req.cityId),
                new SqlParameter("@HotelID",req.HotelID)
            };
            var resp = new TekTvlErrorModel();
            resp.ErrorCode = ErrorCodes.Minus1.ToString();
            resp.ErrorMessage = ErrorCodes.AnError;
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.ErrorCode = Convert.ToString(dt.Rows[0][0]);
                    resp.ErrorMessage = dt.Rows[0]["Msg"].ToString();
                    if (Convert.ToInt32(resp.ErrorCode) == ErrorCodes.One)
                    {
                        //resp.CommonStr = dt.Rows[0]["Tid"].ToString();
                    }
                }
            }
            catch (Exception er)
            {
                resp.ErrorCode = ErrorCodes.Minus1.ToString();
                resp.ErrorMessage = ErrorCodes.AnError;
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_SyncApiHotelData_HotelWise";
        }
    }



    public class ProcGetHotelCode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetHotelCode(IDAL dal)
        {
            _dal = dal;
        }
        public object Call()
        {
            var response = new List<HotelImages>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        response.Add(new HotelImages
                        {
                            CityId = dr["_CityId"] is DBNull ? "" :dr["_CityId"].ToString(),
                            TBOHotelCode = dr["_TBOHotelCode"] is DBNull ? "" : dr["_TBOHotelCode"].ToString()
                        });
                    }
                }
            }
            catch (Exception er)
            {

            }
            return response;
        }

        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "select distinct  _TBOHotelCode,_CityId from  tbl_TBOBasicPropertyInfo";
        }
    }
}
