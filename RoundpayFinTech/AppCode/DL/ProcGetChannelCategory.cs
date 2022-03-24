using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetChannelCategory : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetChannelCategory(IDAL dal) => _dal = dal;
        public object Call(object obj)=> throw new NotImplementedException();

        public object Call()
        {
            List<ChannelCategory> categories = new List<ChannelCategory>();
            DataTable dt = _dal.Get(GetName());
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ChannelCategory category = new ChannelCategory
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            CategoryName=Convert.ToString(dt.Rows[i]["_CategoryName"])
                        };
                        categories.Add(category);
                    }
                }
                catch (Exception)
                {
                }
            }
            return categories;
        }

        public string GetName() => "select * from Master_ChannelCategory";

    }
}
