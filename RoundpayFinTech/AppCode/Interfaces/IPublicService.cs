using Fintech.AppCode.Interfaces;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IPublicService
    {
        List<FieldMasterModel> GetFieldList(int id);
        FieldMasterModel GetFieldListByID(int id);
        IResponseStatus SaveField(FieldMasterModel model);
        FieldMasterModel GetField(int id);
        IResponseStatus SaveVocab(VocabMaster model);
        List<VocabMaster> GetVocabMaster();
        IResponseStatus SaveVocabOption(VocabList model);
        List<VocabList> GetVocabOption(int vmid);
        List<FieldMasterModel> ShowForm(int oid);
        IResponseStatus SavePESFormML(IFormCollection formdata);     

    }
}
