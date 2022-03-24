using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBanner : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBanner(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            string BannerUrl = (string)obj;
            SqlParameter[] param = {
                new SqlParameter("@BannerUrl", BannerUrl),
            };
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.Get(GetNames(),param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
            }
            return _res;
        }

        public object Call()
        {
            var res = new List<BannerImage>();
            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var bannerImage = new BannerImage
                        {
                            URL = row["_BannerUrl"] is DBNull ? "" : row["_BannerUrl"].ToString(),
                            RefUrl = row["_RefUrl"] is DBNull ? "" : row["_RefUrl"].ToString(),
                        };
                        res.Add(bannerImage);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }


        public string GetName() => "Proc_GetBanner";
        public string GetNames() => "Delete from tbl_Banners where _BannerUrl = @BannerUrl;select 1 StatusCode,'Banner Deleted Successfully' msg";
    }
}
