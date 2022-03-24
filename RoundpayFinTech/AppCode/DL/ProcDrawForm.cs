using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model;
using System.Data;
using RoundpayFinTech.AppCode.StaticModel;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDrawForm : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDrawForm(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@OID",(int)obj)
            };
            var resp = new List<FieldMasterModel>();
            var op = new List<VocabList>();
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            op.Add(new VocabList
                            {
                                _ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                _VMID = dr["_VMID"] is DBNull ? 0 : Convert.ToInt32(dr["_VMID"]),
                                _Name = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
                                _IND = dr["_IND"] is DBNull ? 0 : Convert.ToInt32(dr["_IND"]),
                                _EntryDate = dr["_EntryDate"] is DBNull ? "" : dr["_EntryDate"].ToString(),
                                _EntryBy = dr["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dr["_EntryBy"]),
                                _ModifyDate = dr["_ModifyDate"] is DBNull ? "" : dr["_ModifyDate"].ToString(),
                                _ModifyBy = dr["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dr["_ModifyBy"])
                            });
                        }
                    }
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            var fieldMasterModel = new FieldMasterModel
                            {
                                _ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                _Name = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
                                _FieldType = (PESFieldType)(dr["_FieldType"] is DBNull ? 0 : Convert.ToInt32(dr["_FieldType"])),
                                _InputType = dr["_InputType"] is DBNull ? "" : dr["_InputType"].ToString(),
                                _EntryBy = dr["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dr["_EntryBy"]),
                                _EntryDate = dr["_EntryDate"] is DBNull ? "" : dr["_EntryDate"].ToString(),
                                _ModifyBy = dr["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dr["_ModifyBy"]),
                                _ModifyDate = dr["_ModifyDate"] is DBNull ? "" : dr["_ModifyDate"].ToString(),
                                _IND = dr["_IND"] is DBNull ? 0 : Convert.ToInt32(dr["_IND"]),
                                _OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                                _VocabID = dr["_VocabID"] is DBNull ? 0 : Convert.ToInt32(dr["_VocabID"]),
                                _Placeholder = dr["_Placeholder"] is DBNull ? "" : dr["_Placeholder"].ToString(),
                                _Label = dr["_Label"] is DBNull ? "" : dr["_Label"].ToString(),
                                _IsRequired = dr["_IsRequired"] is DBNull ? false : Convert.ToBoolean(dr["_IsRequired"]),
                                _MaxLength = dr["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MaxLength"]),
                                _MinLength = dr["_MinLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MinLength"]),
                                _AutoComplete = dr["_AutoComplete"] is DBNull ? false : Convert.ToBoolean(dr["_AutoComplete"]),
                                _IsDisabled = dr["_IsDisabled"] is DBNull ? false : Convert.ToBoolean(dr["_IsDisabled"]),
                                _IsReadOnly = dr["_IsReadOnly"] is DBNull ? false : Convert.ToBoolean(dr["_IsReadOnly"])
                                //    _VocabOptions=new List<VocabList>()
                            };
                            if (fieldMasterModel._VocabID>0)
                            {
                                fieldMasterModel._VocabOptions = op.Where(x => x._VMID == fieldMasterModel._VocabID).OrderBy(p => p._IND).ToList();
                            }
                            resp.Add(fieldMasterModel);
                        }
                    }
                }
            }
            catch (Exception er)
            {
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_DrawForm";
    }
}
