using System;
using System.Web.Mvc;
using Litium.Web.Models.Websites;
using Litium.Accelerator.Abstractions.ViewModels.Product;
using Litium.Accelerator.Extension.Builders.Product;

namespace Litium.Accelerator.Mvc.Controllers.TextOptionImport
{
    public class TextOptionImportController : ControllerBase
    {
        private readonly TextOptionImportPageViewModelBuilder _textOptionImportPageViewModelBuilder;

        public TextOptionImportController(TextOptionImportPageViewModelBuilder textOptionImportPageViewModelBuilder)
        {
            _textOptionImportPageViewModelBuilder = textOptionImportPageViewModelBuilder;
        }

        public ActionResult Index(PageModel currentPageModel, string message = "", bool success = false)
        {
            var pageModel = _textOptionImportPageViewModelBuilder.Build(currentPageModel);

            pageModel.Message = message;
            pageModel.Success = success;

            return View(pageModel);
        }

        [HttpPost]
        public ActionResult Index(TextOptionImportPageViewModel categoryTreeImportPageViewModel)
        {
            try
            {
                _textOptionImportPageViewModelBuilder.Import(categoryTreeImportPageViewModel.TextOptionName, categoryTreeImportPageViewModel.IsMultiCulture, categoryTreeImportPageViewModel.Area);

                return RedirectToAction(nameof(Index), new { Message = "The import has started successfully. This may take some time. Don't start a new Import.", Success = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { ex.Message, Success = false });
            }
        }
    }
}
