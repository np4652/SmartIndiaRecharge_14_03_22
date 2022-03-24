using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetVocabOptions : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVocabOptions(IDAL dal)
        {
            _dal = dal;
        }

        public object Call(object obj)
        {
            var resp = new List<VocabList>();
            SqlParameter[] param = {
                new SqlParameter("@VMID",(int)obj)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        resp.Add(new VocabList
                        {
                            _ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            _Name = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
                            _EntryBy = dr["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dr["_EntryBy"]),
                            _EntryDate = dr["_EntryDate"] is DBNull ? "" : dr["_EntryDate"].ToString(),
                            _ModifyBy = dr["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dr["_ModifyBy"]),
                            _ModifyDate = dr["_ModifyDate"] is DBNull ? "" : dr["_ModifyDate"].ToString(),
                            _IND = dr["_IND"] is DBNull ? 0 : Convert.ToInt32(dr["_IND"]),
                            _VMID= dr["_VMID"] is DBNull ? 0 : Convert.ToInt32(dr["_VMID"])
                        });
                    }
                }
                else
                {
                    resp.Add(new VocabList
                    {
                        _VMID = (int)obj
                    });
                }
            }
            catch (Exception er)
            {
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "select * from tbl_VocabList where _VMID=@VMID";
        }
    }
}
