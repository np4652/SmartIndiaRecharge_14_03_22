using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using System;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUniqueIDIProcedure:IProcedure
    {
        
        private readonly IDAL _dal;
        public ProcGetUniqueIDIProcedure(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public object Call()
        {
            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        public string GetName()
        {
            return "proc_GetUniqueID";
        }
    }
}
