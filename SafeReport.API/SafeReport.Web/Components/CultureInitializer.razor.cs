using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace SafeReport.Web.Components
{
    public partial class CultureInitializer
    {

        [Inject]
        public NavigationManager _navigationManager { get; set; }
        [Inject]
        public IJSRuntime _jSRuntime { get; set; }

        CultureInfo[] cultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ar-EG")
        };

        public CultureInfo _culture
        {
            get => CultureInfo.CurrentCulture;

            set
            {
                if (CultureInfo.CurrentCulture != value)
                {
                    var js = (IJSInProcessRuntime)_jSRuntime;
                    js.InvokeVoid("blazorCulture.set", value.Name);
                    _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
                }
            }


        }


    }
}
