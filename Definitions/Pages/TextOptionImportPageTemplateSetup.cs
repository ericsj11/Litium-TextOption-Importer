using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Definitions;
using Litium.FieldFramework;
using Litium.Websites;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class TextOptionImportPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate("TextOptionImport")
                {
                    TemplatePath = "",
                    Localizations =
                    {
                        ["sv-SE"] = { Name = "Text option import" },
                        ["en-US"] = { Name = "Text option Import" }
                    },
                    FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Collapsed = false,
                            Fields =
                            {
                                "_name",
                                "_url"
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "Contents",
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Innehåll" },
                                ["en-US"] = { Name = "Content" }
                            },
                            Collapsed = false,
                            Fields =
                            {
                               "Title",
                               "Text"
                            }
                        }
                    }
                }
            };

            return templates;
        }
    }
}
