using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{

    public class ProcGetQRGenerationData : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetQRGenerationData(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetQRGenerationData";
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@PageSize", _req.CommonInt),
                new SqlParameter("@PageNum", _req.CommonInt2),
                new SqlParameter("@TransactionID", _req.CommonStr??string.Empty),
                new SqlParameter("@BankRefID", _req.CommonStr2??string.Empty),
                new SqlParameter("@MobileNo", _req.CommonStr3??string.Empty)
            };
            var _qrList = new QRGenerationReq
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var data = new List<QRGenData>();
            try
            {
                DataSet ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        _qrList.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                        _qrList.Msg = dt.Rows[0]["Msg"].ToString();

                        if (_qrList.Statuscode == ErrorCodes.One)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                data.Add(new QRGenData
                                {
                                    RefID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                    TransactionID = row["_TransactionID"] is DBNull ? string.Empty : row["_TransactionID"].ToString(),
                                    BankRefID = row["_BankRefID"] is DBNull ? string.Empty : row["_BankRefID"].ToString(),
                                    EntryDate = row["_EntryDate"] is DBNull ? string.Empty : row["_EntryDate"].ToString(),
                                    AssignedTo = row["_AssignedTo"] is DBNull ? 0 : Convert.ToInt32(row["_AssignedTo"]),
                                    AssignedDate = row["_AssignedDate"] is DBNull ? string.Empty : row["_AssignedDate"].ToString(),
                                    _AssignedTo = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString()
                                });
                                _qrList.QRGenData = data;

                            }
                        }
                    }
                }
                if (ds.Tables.Count > 1)
                {
                    var pageSetting = new PegeSetting
                    {
                        Count = (int?)ds.Tables[1].Rows[0][0],
                        TopRows = _req.CommonInt,
                        PageNumber = _req.CommonInt2
                    };
                    _qrList.PegeSetting = pageSetting;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _qrList;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }

}
