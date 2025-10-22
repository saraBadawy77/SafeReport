using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace SafeReport.Web.Components
{
    public partial class CultureInitializer
    {
        [Inject]
        public IJSRuntime _JSRuntime { get; set; }
        [Inject]
        public NavigationManager _NavigationManager { get; set; }

        CultureInfo[] SupportedCultures =
        {
        new("ar-EG"),
        new("en-US")
        };

        public CultureInfo _culture {
            get => CultureInfo.CurrentCulture;
        
            set
            {
                if (CultureInfo.CurrentCulture!=value)
                {
                    var js = (IJSInProcessRuntime)_JSRuntime;
                    js.InvokeVoid("blazorCulture.set", value.Name);
                    _NavigationManager.NavigateTo(_NavigationManager.Uri, forceLoad: true);

                }


            }
        
        }


    }
}
