using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetServiceField: IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetServiceField(IDAL dal)
        {
            _dal = dal;
        }

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var resp = new List<FieldMasterModel>();
            SqlParameter[] param = {
                new SqlParameter("@ID",req.CommonInt),
                new SqlParameter("@OID",req.CommonInt2)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count>0)
                {
                    if (req.CommonInt.Equals(0)&&req.CommonInt2>0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            resp.Add(new FieldMasterModel
                            {
                                _ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                _Name = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
                                _FieldType = (PESFieldType)(dr["_FieldType"] is DBNull ? 0 : Convert.ToInt32(dr["_FieldType"])),
                                _InputType = dr["_InputType"] is DBNull ? "" : dr["_InputType"].ToString(),
                                _ServiceName = dr["_ServiceName"] is DBNull ? "" : dr["_ServiceName"].ToString(),
                                _EntryBy = dr["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dr["_EntryBy"]),
                                _EntryDate = dr["_EntryDate"] is DBNull ? "" : dr["_EntryDate"].ToString(),
                                _ModifyBy = dr["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dr["_ModifyBy"]),
                                _ModifyDate = dr["_ModifyDate"] is DBNull ? "" : dr["_ModifyDate"].ToString(),
                                _IND = dr["_IND"] is DBNull ? 0 : Convert.ToInt32(dr["_IND"]),
                                _OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                                _VocabID = dr["_VocabID"] is DBNull ? 0 : Convert.ToInt32(dr["_VocabID"]),
                                _Placeholder = dr["_Placeholder"] is DBNull ? "" : dr["_Placeholder"].ToString(),
                                _Label = dr["_Label"] is DBNull ? "" :dr["_Label"].ToString(),
                                _IsRequired = dr["_IsRequired"] is DBNull ? false : Convert.ToBoolean(dr["_IsRequired"]),
                                _MaxLength = dr["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MaxLength"]),
                                _MinLength = dr["_MinLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MinLength"]),
                                _AutoComplete = dr["_AutoComplete"] is DBNull ? false : Convert.ToBoolean(dr["_AutoComplete"]),
                                _IsDisabled = dr["_IsDisabled"] is DBNull ? false : Convert.ToBoolean(dr["_IsDisabled"]),
                                _IsReadOnly = dr["_IsReadOnly"] is DBNull ? false : Convert.ToBoolean(dr["_IsReadOnly"])
                            });
                        }
                    }
                    else
                    {
                        return new FieldMasterModel
                        {
                            _ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            _Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString(),
                            _FieldType = (PESFieldType)(dt.Rows[0]["_FieldType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_FieldType"])),
                            _InputType = dt.Rows[0]["_InputType"] is DBNull ? "" : dt.Rows[0]["_InputType"].ToString(),
                            _ServiceName = dt.Rows[0]["_ServiceName"] is DBNull ? "" : dt.Rows[0]["_ServiceName"].ToString(),
                            _EntryBy = dt.Rows[0]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EntryBy"]),
                            _EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString(),
                            _ModifyBy = dt.Rows[0]["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ModifyBy"]),
                            _ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                            _IND = dt.Rows[0]["_IND"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_IND"]),
                            _OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]),
                            _VocabID = dt.Rows[0]["_VocabID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_VocabID"]),
                            _Placeholder = dt.Rows[0]["_Placeholder"] is DBNull ? "" : dt.Rows[0]["_Placeholder"].ToString(),
                            _Label = dt.Rows[0]["_Label"] is DBNull ? "" : dt.Rows[0]["_Label"].ToString(),
                            _IsRequired = dt.Rows[0]["_IsRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRequired"]),
                            _MaxLength = dt.Rows[0]["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_MaxLength"]),
                            _MinLength = dt.Rows[0]["_MinLength"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_MinLength"]),
                            _AutoComplete = dt.Rows[0]["_AutoComplete"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_AutoComplete"]),
                            _IsDisabled = dt.Rows[0]["_IsDisabled"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDisabled"]),
                            _IsReadOnly = dt.Rows[0]["_IsReadOnly"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsReadOnly"])
                        };
                    }
                }
            }
            catch (Exception er)
            {
            }
            if (req.CommonInt.Equals(0) && req.CommonInt2 > 0)
                return resp;
            else
                return new FieldMasterModel();
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "select tfm.*,optr._ID _OID,optr._Name _ServiceName from tbl_FieldMaster tfm inner join tbl_Operator optr on tfm._OID=optr._ID where tfm._ID=iif(@ID=0,tfm._ID,@ID) and tfm._OID=iif(@OID=0,tfm._OID,@OID)";
        }
    }
}
