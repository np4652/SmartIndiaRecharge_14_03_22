using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class procGetMessage : IProcedure
    {
        private readonly IDAL _dal;
        public procGetMessage(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public object Call()
        {
            List<MasterMessage> res = new List<MasterMessage>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                foreach (DataRow row in dt.Rows)
                {
                    var resp = new MasterMessage
                    {
                        ID = Convert.ToInt32(row["_ID"]),
                        FormatType = row["_FormatType"].ToString(),
                        Remark= row["_Remark"].ToString()
                    };
                    res.Add(resp);
                }
            }
            catch (Exception ex)
            {
               
            }
            return res;
        }
        public string GetName()
        {
            return "select * from MASTER_MESSAGE_FORMAT where _IsActive=1";
        }

    }
    public class procGetMessageAll : IProcedure
    {
        private readonly IDAL _dal;
        public procGetMessageAll(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public object Call()
        {
            List<MasterMessage> res = new List<MasterMessage>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                foreach (DataRow row in dt.Rows)
                {
                    var resp = new MasterMessage
                    {
                        ID = Convert.ToInt32(row["_ID"]),
                        FormatType = row["_FormatType"].ToString(),
                        Remark = row["_Remark"].ToString()
                    };
                    res.Add(resp);
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }
        public string GetName()
        {
            return "select * from MASTER_MESSAGE_FORMAT";
        }

    }
}
