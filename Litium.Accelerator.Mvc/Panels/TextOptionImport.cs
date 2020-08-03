using System.Threading.Tasks;
using Litium.Accelerator.Constants;
using Litium.Products;
using Litium.Web.Administration.Panels;

namespace Litium.Accelerator.Mvc.Panels
{
    public class TextOptionImport : PanelDefinitionBase<ProductArea, TextOptionImport.SettingsModel>
    {
        public override string ComponentName => null;

        public override string Url => "Panels/TextOptionImport";

        public override bool PermissionCheck()
        {
            return true;
        }

        public override async Task<SettingsModel> GetSettingsAsync()
        {
            return new SettingsModel();
        }

        public class SettingsModel : IPanelSettings
        {

        }
    }
}