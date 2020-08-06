using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Litium;
using Litium.Accelerator.Builders;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
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
        private readonly FieldDefinitionService _fieldDefinitionService;

        public TextOptionImportPageViewModelBuilder(FieldDefinitionService fieldDefinitionService)
        {
            _fieldDefinitionService = fieldDefinitionService;
        }

        public TextOptionImportPageViewModel Build(PageModel currentPageModel)
        {
            var pageModel = currentPageModel.MapTo<TextOptionImportPageViewModel>();

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

        public void Import(TextOptionImportPageViewModel textOptionImportPageViewModel, DataSet content)
        {
            if (string.IsNullOrWhiteSpace(textOptionImportPageViewModel.TextOptionName))
            {
                throw new Exception("TextOptionName is empty!");
            }

            if (string.IsNullOrWhiteSpace(textOptionImportPageViewModel.Area))
            {
                throw new Exception("No Area is selected!");
            }

            ImportTextOptions(content, textOptionImportPageViewModel);
        }

        private void ImportTextOptions(DataSet content, TextOptionImportPageViewModel textOptionImportPageViewModel)
        {
            var textOptions = new Dictionary<string, string>();

            if (content != null)
            {
                var rowCount = 0;

                if (content.Tables.Count < 1 || content.Tables[0].Rows.Count < 1)
                {
                    throw new Exception("Can't find any lines in the uploaded Excel");
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
            switch (textOptionImportPageViewModel.Area)
            {
                default:
                    textOptionField = GetProductAreaFieldDefinition(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                    break;
                case nameof(CustomerArea):
                    textOptionField = GetCustomerAreaFieldDefinition(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                    break;
                case nameof(WebsiteArea):
                    textOptionField = GetWebsiteAreaFieldDefinition(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                    break;
                case nameof(MediaArea):
                    textOptionField = GetMediaAreaFieldDefinition(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
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
                    this.Log().Info($"Adding Text Option: {textOption.Key}/{textOption.Value} to Field: {textOptionField.Id} in Area: {textOptionImportPageViewModel.Area}");

                    option.Items.Add(new TextOption.Item
                    {
                        Value = textOption.Key,
                        Name = new Dictionary<string, string> { { "en-US", textOption.Value }, { "sv-SE", textOption.Value } }
                    });
                }
            }

            _fieldDefinitionService.Update(textOptionField);

            timer.Stop();

            this.Log().Info($"Time taken in seconds: {timer.Elapsed.TotalSeconds}");
        }

        private FieldDefinition GetProductAreaFieldDefinition(string textOptionName, bool isMultiCulture)
        {
            var textOptionField = _fieldDefinitionService.Get<ProductArea>(textOptionName)?.MakeWritableClone();

            if (textOptionField != null)
            {
                if (textOptionField.MultiCulture != isMultiCulture)
                {
                    throw new Exception($"A Text Option with ID: {textOptionField.Id} exists. It's not possible to change To or From Multi Culture.");
                }

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
                if (textOptionField.MultiCulture != isMultiCulture)
                {
                    throw new Exception($"A Text Option with ID: {textOptionField.Id} exists. It's not possible to change To or From Multi Culture.");
                }

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
                if (textOptionField.MultiCulture != isMultiCulture)
                {
                    throw new Exception($"A Text Option with ID: {textOptionField.Id} exists. It's not possible to change To or From Multi Culture.");
                }

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
                if (textOptionField.MultiCulture != isMultiCulture)
                {
                    throw new Exception($"A Text Option with ID: {textOptionField.Id} exists. It's not possible to change To or From Multi Culture.");
                }

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
