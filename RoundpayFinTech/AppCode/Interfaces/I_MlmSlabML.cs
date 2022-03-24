using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interface
{
    public interface I_MlmSlabML
    {
        MLM_SlabDetailModel MLM_GetSlabDetail(int SlabID, int OpTypeID);
        IResponseStatus MLM_UpdateSlabDetail(MLM_SlabCommission mlmslabcommission);
    }
}
