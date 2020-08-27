using System;
using System.Data;
using System.IO;
using System.Web.Mvc;
using ExcelDataReader;
using ExcelLibrary;
using Litium.Web.Models.Websites;
using Litium.Accelerator.ViewModels.Product;
using Litium.Accelerator.Builders.Product;

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
        public ActionResult Index(TextOptionImportPageViewModel textOptionImportPageViewModel)
        {
            try
            {
                _textOptionImportPageViewModelBuilder.ImportTextOptions(textOptionImportPageViewModel, GetFileContent());

                return RedirectToAction(nameof(Index), new { Message = $"The import of Text Option: \"{textOptionImportPageViewModel.TextOptionName}\" was successful in Area: \"{textOptionImportPageViewModel.Area}\"!", Success = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { ex.Message, Success = false });
            }
        }

        [HttpPost]
        public ActionResult DownloadExcel(TextOptionImportPageViewModel textOptionImportPageViewModel)
        {
            try
            {
                var dataSet = _textOptionImportPageViewModelBuilder.CreateTextOptions(textOptionImportPageViewModel);

                return CreateWorkBook(dataSet, textOptionImportPageViewModel.TextOption);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { ex.Message, Success = false });
            }
        }

        public ActionResult CreateWorkBook(DataSet dataSet, string textOption)
        {
            DataSetHelper.CreateWorkbook("Workbook.xls", dataSet);

            var fileBytes = System.IO.File.ReadAllBytes("Workbook.xls");
                
            var splitTextOption = textOption.Split(';');
            var area = splitTextOption[0];
            var textOptionName = splitTextOption[1];

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet,
                $"{area}_{textOptionName}.xls");
        }

        public DataSet GetFileContent()
        {
            if (Request.Files.Count <= 0)
            {
                throw new Exception("Could not find any uploaded file!");
            }

            var file = Request.Files[0];

            if (string.IsNullOrWhiteSpace(file?.FileName) || file.ContentLength <= 0)
            {
                throw new Exception("Something is wrong with the uploaded file!");
            }

            IExcelDataReader reader;

            //Must check file extension to adjust the reader to the excel file type
            if (Path.GetExtension(file.FileName).Equals(".xls"))
            {
                reader = ExcelReaderFactory.CreateBinaryReader(file.InputStream);
            }
            else if (Path.GetExtension(file.FileName).Equals(".xlsx"))
            {
                reader = ExcelReaderFactory.CreateOpenXmlReader(file.InputStream);
            }
            else
            {
                throw new Exception("Wrong file ending. Use .xls or .xlsx!");
            }

            if (reader == null)
            {
                throw new Exception("File Reader is null. Something is wrong with the uploaded file!");
            }

            var content = reader.AsDataSet();

            reader.Close();

            return content;
        }
    }
}
