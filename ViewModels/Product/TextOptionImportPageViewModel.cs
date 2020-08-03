using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.ViewModels;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.ViewModels.Product
{
    public class TextOptionImportPageViewModel : PageViewModel, IAutoMapperConfiguration
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public int NumberOfFiles { get; set; }
        public string TextOptionName { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public List<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
        public string Area { get; set; }
        public bool IsMultiCulture { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, TextOptionImportPageViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField("Title"))
               .ForMember(x => x.Text, m => m.MapFrom(orderPage => orderPage.GetValue<string>("Text")));
        }
    }
}
