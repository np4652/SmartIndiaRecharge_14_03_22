using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.AppCode.HelperClass
{
    public static class ReportHelper
    {
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        public static FileStreamResult HtmlToPdfConverterold(string html, string filename, int Orientation = 0)
        {

            HtmlToPdfConverter converter = new HtmlToPdfConverter();
            WebKitConverterSettings settings = new WebKitConverterSettings();
            settings.WebKitPath = Fintech.AppCode.StaticModel.DOCType.WebKitPath;

            converter.ConverterSettings = settings;
            settings.Orientation = (PdfPageOrientation)Orientation;
            PdfDocument document = converter.Convert(html, string.Empty);
            MemoryStream ms = new MemoryStream();
            document.Save(ms);
            document.Close(true);
            ms.Position = 0;
            FileStreamResult fsr = new FileStreamResult(ms, "application/pdf");
            fsr.FileDownloadName = filename;
            return fsr;


        }

        public static ResponseStatus HtmlToPdfConverter(string html, string filename, int Orientation = 0)
        {
            var response = new ResponseStatus
            {
                Statuscode = -1,
                Msg = "Temp Error"
            };
            HtmlToPdfConverter converter = new HtmlToPdfConverter();
            WebKitConverterSettings settings = new WebKitConverterSettings();
            settings.WebKitPath = Fintech.AppCode.StaticModel.DOCType.WebKitPath;

            converter.ConverterSettings = settings;
            settings.Orientation = (PdfPageOrientation)Orientation;
            PdfDocument document = converter.Convert(html, string.Empty);
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                document.Close(true);
                ms.Position = 0;
                response = new ResponseStatus
                {
                    Statuscode = 1,
                    ByteArray = ms.ToArray(),
                    Msg = filename,
                    CommonStr = "application/pdf"
                };
            }
            return response;
        }
    }
}
