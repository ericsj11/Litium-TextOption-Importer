using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using ExcelDataReader;
using Litium;
using Litium.Accelerator.Builders;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Foundation.Modules.ExtensionMethods;
using Litium.Media;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;
using Litium.Websites;
using Litium.Accelerator.ViewModels.Product;

namespace Litium.Accelerator.Builders.Product
{
    public class TextOptionImportPageViewModelBuilder : IViewModelBuilder<TextOptionImportPageViewModel>
    {
        private readonly FolderService _folderService;
        private readonly FileService _fileService;
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly FieldDefinitionService _fieldDefinitionService;

        public TextOptionImportPageViewModelBuilder(
            FolderService folderService,
            FileService fileService,
            FieldTemplateService fieldTemplateService,
            FieldDefinitionService fieldDefinitionService)
        {
            _folderService = folderService;
            _fileService = fileService;
            _fieldTemplateService = fieldTemplateService;
            _fieldDefinitionService = fieldDefinitionService;
        }

        public TextOptionImportPageViewModel Build(PageModel currentPageModel)
        {
            var pageModel = currentPageModel.MapTo<TextOptionImportPageViewModel>();

            pageModel.NumberOfFiles = GetNumberOfFiles();

            var areas = GetAreas();

            foreach (var area in areas)
            {
                pageModel.Areas.Add(new SelectListItem
                {
                    Text = area,
                    Value = area
                });
            }

            return pageModel;
        }

        public IEnumerable<string> GetAreas()
        {
            return new List<string>
            {
                nameof(ProductArea),
                nameof(CustomerArea),
                nameof(WebsiteArea),
                nameof(MediaArea)
            };
        }

        private int GetNumberOfFiles()
        {
            var folder = _folderService.Get("TextOptionImport");

            if (folder == null)
            {
                var folderFieldTemplate = _fieldTemplateService.GetAll().FirstOrDefault(x => x.Id == MediaNameConstants.DefaultFolderTemplate);
                if (folderFieldTemplate != null)
                {
                    _folderService.Create(
                        new Folder(folderFieldTemplate.SystemId, "TextOptionImport")
                        { Id = "TextOptionImport" });
                }

                return 0;
            }

            var files = _fileService.GetByFolder(folder.SystemId);

            return files.Count();
        }

        public void Import(string textOptionName, bool isMultiCulture, string area)
        {
            if (string.IsNullOrWhiteSpace(textOptionName))
            {
                throw new Exception("TextOptionName is empty!");
            }

            var textOptionImportFolder = _folderService.Get("TextOptionImport");

            if (textOptionImportFolder == null)
            {
                throw new Exception("TextOptionImport folder doesn't exist.");
            }

            var excelFiles = _fileService.GetByFolder(textOptionImportFolder.SystemId).ToList();

            if (excelFiles == null)
            {
                throw new Exception("Can't find any excel file in the folder TextOptionImport.");
            }

            if (excelFiles.Count > 1)
            {
                throw new Exception("There are to many files in the folder TextOptionImport. Make sure it's only 1.");
            }

            var excelFile = excelFiles.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(excelFile?.Name))
            {
                throw new Exception("Excel file name is empty.");
            }

            var filePath = ConfigurationManager.AppSettings["TextOptionImportPath"];

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("TextOptionImportPath is empty. Set it in web.config.");
            }

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (System.IO.File.Exists(filePath + excelFile.Name))
            {
                System.IO.File.Delete(filePath + excelFile.Name);
            }

            System.IO.File.WriteAllBytes(filePath + excelFile.Name, excelFile.GetFileContent());

            var thread1 = new Thread(x => ImportTextOptions(filePath + excelFile.Name, textOptionName, isMultiCulture, area));
            thread1.Start();
        }

        private void ImportTextOptions(string filePath, string textOptionName, bool isMultiCulture, string area)
        {
            var textOptions = new Dictionary<string, string>();

            IExcelDataReader reader = null;

            //Load file into a stream
            var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);

            //Must check file extension to adjust the reader to the excel file type
            if (Path.GetExtension(filePath).Equals(".xls"))
            {
                reader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else if (Path.GetExtension(filePath).Equals(".xlsx"))
            {
                reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }

            if (reader != null)
            {
                //Fill DataSet
                var rowCount = 0;
                var content = reader.AsDataSet();

                if (content?.Tables == null || content.Tables.Count < 1 || content.Tables[0].Rows.Count < 1)
                {
                    throw new Exception($"Can't find any lines in the Excel in path {filePath}");
                }

                var columnValueOfFirst = 0;
                var columnValueOfSecond = 0;

                foreach (DataRow row in content.Tables[0].Rows)
                {
                    rowCount++;

                    var key = row.ItemArray[columnValueOfFirst].ToString();
                    var value = row.ItemArray[columnValueOfSecond].ToString();

                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                    {
                        throw new Exception($"One of the columns on line {rowCount} is empty. Fix that or remove the row.");
                    }

                    if (rowCount == 1)
                    {
                        columnValueOfFirst = row.ItemArray.Where(x => x != null && x != DBNull.Value).Cast<string>().ToList().IndexOf("Key");
                        columnValueOfSecond = row.ItemArray.Where(x => x != null && x != DBNull.Value).Cast<string>().ToList().IndexOf("Value");

                        key = row.ItemArray[columnValueOfFirst].ToString();
                        value = row.ItemArray[columnValueOfSecond].ToString();

                        if (key != "Key")
                        {
                            throw new Exception("There must exist a column on the first row named: Key.");
                        }
                        if (value != "Value")
                        {
                            throw new Exception("There must exist a column on the first row named: Value.");
                        }
                    }
                    else
                    {
                        if (!textOptions.ContainsKey(key))
                        {
                            textOptions.Add(key, value);
                        }
                    }
                }
            }

            var timer = new Stopwatch();
            timer.Start();

            FieldDefinition textOptionField;
            switch (area)
            {
                default:
                    textOptionField = GetProductAreaFieldDefinition(textOptionName, isMultiCulture);
                    break;
                case nameof(CustomerArea):
                    textOptionField = GetCustomerAreaFieldDefinition(textOptionName, isMultiCulture);
                    break;
                case nameof(WebsiteArea):
                    textOptionField = GetWebsiteAreaFieldDefinition(textOptionName, isMultiCulture);
                    break;
                case nameof(MediaArea):
                    textOptionField = GetMediaAreaFieldDefinition(textOptionName, isMultiCulture);
                    break;
            }


            if (!(textOptionField.Option is TextOption option))
            {
                textOptionField.Option = new TextOption
                {
                    MultiSelect = false
                };

                option = textOptionField.Option as TextOption;
            }

            if (option != null && option.Items == null)
            {
                option.Items = new List<TextOption.Item>();
            }

            foreach (var textOption in textOptions)
            {
                if (option != null && option.Items.FirstOrDefault(x => x.Value == textOption.Key) == null)
                {
                    this.Log().Info($"Adding Text Option: {textOption.Key}/{textOption.Value} to Field: {textOptionField.Id} in Area: {area}");

                    option.Items.Add(new TextOption.Item
                    {
                        Value = textOption.Key,
                        Name = new Dictionary<string, string> { { "en-US", textOption.Value }, { "sv-SE", textOption.Value } }
                    });
                }
            }

            _fieldDefinitionService.Update(textOptionField);

            stream.Close();
            reader?.Close();
            timer.Stop();

            this.Log().Info($"Time taken in seconds: {timer.Elapsed.TotalSeconds}");
        }

        private FieldDefinition GetProductAreaFieldDefinition(string textOptionName, bool isMultiCulture)
        {
            var textOptionField = _fieldDefinitionService.Get<ProductArea>(textOptionName)?.MakeWritableClone();

            if (textOptionField != null)
            {
                return textOptionField;
            }

            _fieldDefinitionService.Create(new FieldDefinition<ProductArea>(textOptionName, SystemFieldTypeConstants.TextOption)
            {
                Localizations =
                {
                    ["sv-SE"] = {Name = textOptionName},
                    ["en-US"] = {Name = textOptionName}
                },
                CanBeGridColumn = true,
                CanBeGridFilter = true,
                MultiCulture = isMultiCulture,
                Option = new TextOption
                {
                    MultiSelect = false
                }
            });

            textOptionField = _fieldDefinitionService.Get<ProductArea>(textOptionName).MakeWritableClone();

            return textOptionField;
        }

        private FieldDefinition GetCustomerAreaFieldDefinition(string textOptionName, bool isMultiCulture)
        {
            var textOptionField = _fieldDefinitionService.Get<CustomerArea>(textOptionName)?.MakeWritableClone();

            if (textOptionField != null)
            {
                return textOptionField;
            }

            _fieldDefinitionService.Create(new FieldDefinition<CustomerArea>(textOptionName, SystemFieldTypeConstants.TextOption)
            {
                Localizations =
                {
                    ["sv-SE"] = {Name = textOptionName},
                    ["en-US"] = {Name = textOptionName}
                },
                CanBeGridColumn = true,
                CanBeGridFilter = true,
                MultiCulture = isMultiCulture,
                Option = new TextOption
                {
                    MultiSelect = false
                }
            });

            textOptionField = _fieldDefinitionService.Get<CustomerArea>(textOptionName).MakeWritableClone();

            return textOptionField;
        }

        private FieldDefinition GetWebsiteAreaFieldDefinition(string textOptionName, bool isMultiCulture)
        {
            var textOptionField = _fieldDefinitionService.Get<WebsiteArea>(textOptionName)?.MakeWritableClone();

            if (textOptionField != null)
            {
                return textOptionField;
            }

            _fieldDefinitionService.Create(new FieldDefinition<WebsiteArea>(textOptionName, SystemFieldTypeConstants.TextOption)
            {
                Localizations =
                {
                    ["sv-SE"] = {Name = textOptionName},
                    ["en-US"] = {Name = textOptionName}
                },
                CanBeGridColumn = true,
                CanBeGridFilter = true,
                MultiCulture = isMultiCulture,
                Option = new TextOption
                {
                    MultiSelect = false
                }
            });

            textOptionField = _fieldDefinitionService.Get<WebsiteArea>(textOptionName).MakeWritableClone();

            return textOptionField;
        }

        private FieldDefinition GetMediaAreaFieldDefinition(string textOptionName, bool isMultiCulture)
        {
            var textOptionField = _fieldDefinitionService.Get<MediaArea>(textOptionName)?.MakeWritableClone();

            if (textOptionField != null)
            {
                return textOptionField;
            }

            _fieldDefinitionService.Create(new FieldDefinition<MediaArea>(textOptionName, SystemFieldTypeConstants.TextOption)
            {
                Localizations =
                {
                    ["sv-SE"] = {Name = textOptionName},
                    ["en-US"] = {Name = textOptionName}
                },
                CanBeGridColumn = true,
                CanBeGridFilter = true,
                MultiCulture = isMultiCulture,
                Option = new TextOption
                {
                    MultiSelect = false
                }
            });

            textOptionField = _fieldDefinitionService.Get<MediaArea>(textOptionName).MakeWritableClone();

            return textOptionField;
        }
    }
}
