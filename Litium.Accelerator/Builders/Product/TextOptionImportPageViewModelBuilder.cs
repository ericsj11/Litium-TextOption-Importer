using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Litium;
using Litium.Accelerator.Builders;
using Litium.Blocks;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Globalization;
using Litium.Media;
using Litium.Products;
using Litium.Runtime;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Web.Models.Websites;
using Litium.Websites;
using Litium.Accelerator.ViewModels.Product;

namespace Litium.Accelerator.Builders.Product
{
    public class TextOptionImportPageViewModelBuilder : IViewModelBuilder<TextOptionImportPageViewModel>
    {
        private const string Key = "Key";
        private const string Value = "Value";
        private readonly FieldDefinitionService _fieldDefinitionService;

        public TextOptionImportPageViewModelBuilder(FieldDefinitionService fieldDefinitionService)
        {
            _fieldDefinitionService = fieldDefinitionService;
        }

        public TextOptionImportPageViewModel Build(PageModel currentPageModel)
        {
            var pageModel = currentPageModel.MapTo<TextOptionImportPageViewModel>();

            pageModel.Areas = CreateSelectListItemList(GetAreas());

            return pageModel;
        }

        private static List<SelectListItem> CreateSelectListItemList(IEnumerable<string> areas)
        {
            return areas.Select(area => new SelectListItem { Text = area, Value = area }).ToList();
        }

        public static IEnumerable<string> GetAreas()
        {
            return new List<string>
            {
                nameof(ProductArea),
                nameof(CustomerArea),
                nameof(SalesArea),
                nameof(WebsiteArea),
                nameof(GlobalizationArea),
                nameof(MediaArea),
                nameof(BlockArea),
            };
        }

        public void ImportTextOptions(TextOptionImportPageViewModel textOptionImportPageViewModel, DataSet content)
        {
            if (string.IsNullOrWhiteSpace(textOptionImportPageViewModel.TextOptionName))
            {
                throw new Exception("TextOptionName is empty!");
            }

            if (string.IsNullOrWhiteSpace(textOptionImportPageViewModel.Area))
            {
                throw new Exception("No Area is selected!");
            }

            if (content == null)
            {
                throw new Exception("Can't find any content in the Excel file!");
            }

            if (content.Tables.Count < 1 || content.Tables[0].Rows.Count < 1)
            {
                throw new Exception("Can't find any lines in the uploaded Excel!");
            }

            Import(textOptionImportPageViewModel, content);
        }

        private void Import(TextOptionImportPageViewModel textOptionImportPageViewModel, DataSet content)
        {
            var textOptions = GetTextOptionsFromExcelContent(content);

            var timer = new Stopwatch();
            timer.Start();

            var textOptionField = GetOrCreateTextOptionField(textOptionImportPageViewModel);

            var option = CreateOptionOnNewTextOption(textOptionField);

            foreach (var textOption in textOptions)
            {
                AddOptionsToTextOptionField(textOptionImportPageViewModel, textOptionField, option, textOption);
            }

            _fieldDefinitionService.Update(textOptionField);

            timer.Stop();

            this.Log().Info($"Time taken in seconds: {timer.Elapsed.TotalSeconds}");
        }

        private void AddOptionsToTextOptionField(TextOptionImportPageViewModel textOptionImportPageViewModel, FieldDefinition textOptionField, TextOption option, KeyValuePair<string, string> textOption)
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

        private static TextOption CreateOptionOnNewTextOption(FieldDefinition textOptionField)
        {
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

            return option;
        }

        private FieldDefinition GetOrCreateTextOptionField(TextOptionImportPageViewModel textOptionImportPageViewModel)
        {
            switch (textOptionImportPageViewModel.Area)
            {
                default:
                    return GetAreaFieldDefinition<ProductArea>(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                case nameof(CustomerArea):
                    return GetAreaFieldDefinition<CustomerArea>(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                case nameof(SalesArea):
                    return GetAreaFieldDefinition<SalesArea>(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                case nameof(WebsiteArea):
                    return GetAreaFieldDefinition<WebsiteArea>(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                case nameof(GlobalizationArea):
                    return GetAreaFieldDefinition<GlobalizationArea>(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                case nameof(MediaArea):
                    return GetAreaFieldDefinition<MediaArea>(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
                case nameof(BlockArea):
                    return GetAreaFieldDefinition<BlockArea>(textOptionImportPageViewModel.TextOptionName, textOptionImportPageViewModel.IsMultiCulture);
            }
        }

        private static Dictionary<string, string> GetTextOptionsFromExcelContent(DataSet content)
        {
            var textOptions = new Dictionary<string, string>();

            var rowCount = 0;
            var columnValueOfFirst = 0;
            var columnValueOfSecond = 0;

            foreach (DataRow row in content.Tables[0].Rows)
            {
                rowCount++;

                // This allow Key/Value to be placed in any column of the Excel.
                var key = row.ItemArray[columnValueOfFirst].ToString();
                var value = row.ItemArray[columnValueOfSecond].ToString();

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception($"One of the columns on line {rowCount} is empty. Fix that or remove the row!");
                }

                if (rowCount == 1)
                {
                    ValidateFirstRowOfExcel(out columnValueOfFirst, out columnValueOfSecond, row);
                }
                else
                {
                    if (!textOptions.ContainsKey(key))
                    {
                        textOptions.Add(key, value);
                    }
                }
            }

            return textOptions;
        }

        private static void ValidateFirstRowOfExcel(out int columnValueOfFirst, out int columnValueOfSecond, DataRow row)
        {
            columnValueOfFirst = row.ItemArray.Where(x => x != null && x != DBNull.Value).Cast<string>().ToList().IndexOf(Key);
            columnValueOfSecond = row.ItemArray.Where(x => x != null && x != DBNull.Value).Cast<string>().ToList().IndexOf(Value);

            if (row.ItemArray[columnValueOfFirst].ToString() != Key)
            {
                throw new Exception("There must exist a column on the first row named: Key.");
            }
            if (row.ItemArray[columnValueOfSecond].ToString() != Value)
            {
                throw new Exception("There must exist a column on the first row named: Value.");
            }
        }

        private FieldDefinition GetAreaFieldDefinition<T>(string textOptionName, bool isMultiCulture) where T : IArea
        {
            var textOptionField = _fieldDefinitionService.Get<T>(textOptionName)?.MakeWritableClone();

            if (textOptionField != null)
            {
                if (textOptionField.MultiCulture != isMultiCulture)
                {
                    throw new Exception($"A Text Option with ID: {textOptionField.Id} exists. It's not possible to change To or From Multi Culture.");
                }

                return textOptionField;
            }

            _fieldDefinitionService.Create(new FieldDefinition<T>(textOptionName, SystemFieldTypeConstants.TextOption)
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

            textOptionField = _fieldDefinitionService.Get<T>(textOptionName).MakeWritableClone();

            return textOptionField;
        }
    }
}
