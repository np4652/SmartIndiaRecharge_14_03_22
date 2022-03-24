using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region Public-E-Services

        [HttpGet]
        [Route("Admin/PES")]
        [Route("PublicEServices")]
        public IActionResult PublicEServices()
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return View(operatorML.GetOperators(OPTypes.PublicEServices));
        }

        [HttpPost]
        [Route("Admin/LoadFields")]
        [Route("LoadFields")]
        public IActionResult LoadFields(int OID)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return PartialView("PES/_FieldMaster", pes.GetFieldList(OID));
        }

        [HttpPost]
        [Route("Admin/AddFields")]
        [Route("AddFields")]
        public IActionResult AddFields(int id)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return PartialView("PES/_AddFields", pes.GetFieldListByID(id));
        }

        [HttpPost]
        [Route("Admin/SaveField")]
        [Route("SaveField")]
        public IActionResult SaveField([FromBody] FieldMasterModel model)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return Json((ResponseStatus)pes.SaveField(model));
        }

        [HttpPost]
        [Route("Admin/AddVocabModal")]
        [Route("AddVocabModal")]
        public IActionResult AddVocabModal(int id, string name, int ind)
        {
            var model = new VocabMaster
            {
                _ID = id,
                _Name = name,
                _IND = ind
            };
            return PartialView("PES/_AddVocab", model);
        }

        [HttpPost]
        [Route("Admin/SaveVocab")]
        [Route("SaveVocab")]
        public IActionResult SaveVocab([FromBody] VocabMaster model)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return Json((ResponseStatus)pes.SaveVocab(model));
        }

        [HttpPost]
        [Route("Admin/LoadVocabs")]
        [Route("LoadVocabs")]
        public IActionResult LoadVocabs()
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            var lst = pes.GetVocabMaster();
            return PartialView("PES/_VocabMaster", lst);
        }

        [HttpPost]
        [Route("Admin/OpenOptionModal")]
        [Route("OpenOptionModal")]
        public IActionResult OpenOptionModal()
        {
            return PartialView("PES/_OptionModal");
        }

        [HttpPost]
        [Route("Admin/OpenOptionList")]/*open option list below option insert form*/
        [Route("OpenOptionList")]
        public IActionResult OpenOptionList(int id)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return PartialView("PES/_OptionList", pes.GetVocabOption(id));
        }

        [HttpPost]
        [Route("Admin/SaveVocabOption")]
        [Route("SaveVocabOption")]
        public IActionResult SaveVocabOption([FromBody] VocabList model)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return Json((ResponseStatus)pes.SaveVocabOption(model));
        }

        [HttpPost]
        [Route("Admin/GetAllVocab")]
        [Route("GetAllVocab")]
        public IActionResult GetAllVocab()
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            var lst = pes.GetVocabMaster();
            return Json(lst);
        }

        [HttpPost]
        [Route("Admin/ShowForm")]
        [Route("ShowForm")]
        public IActionResult ShowForm(int OID)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return PartialView("PES/_ShowForm", pes.ShowForm(OID));
        }
        #endregion
    }
}
