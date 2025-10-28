using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SafeReport.Web.DTOs;
using SafeReport.Web.ResourcesFiles;
using SafeReport.Web.Services;

namespace SafeReport.Web.Pages;

public partial class Reports
{
    private List<ReportDTO> pagedReports = new();
    private List<Incident> reportTypes = new();
    private List<IncidentTypeDto> incidentTypes=new();
    public int? incidentTypeId;
    [Inject]
    Microsoft.JSInterop.IJSRuntime JS { get; set; }
    [Inject]
    IStringLocalizer<Resource> _Localizer { get; set; }
    private int? filterType;
    private DateTime? filterDate;

    private int currentPage = 1;
    private int pageSize = 10;
    private int totalPages ;

    [Inject]
    private ReportService ReportService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        var typesData = await ReportService.GetAllIncidentsAsync();
        reportTypes = typesData.Select(r => r.Data!).ToList();

        var incidentTypeData = await ReportService.GetAllIncidentTypeAsync();
        incidentTypes = incidentTypeData.Data;

        await LoadReportsAsync();
    }

    private async Task LoadReportsAsync()
    {
        var filter = new ReportFilterDto
        {
            IncidentId = filterType,
            CreatedDate = filterDate,
            PageNumber = currentPage,
            IncidentTypeId = incidentTypeId,
            PageSize = pageSize
        };

        var response = await ReportService.GetAllReportsAsync(filter);
        if (response.Success && response.Data != null)
        {
            pagedReports = response.Data.Reports.ToList();
            totalPages = (int)Math.Ceiling((double)response.Data.TotalCount / pageSize);
        }
        else
        {
            pagedReports.Clear();
            totalPages = 1;
        }
    }
    private async Task ResetFilters()
    {
        filterType = null;
        filterDate = null;
        incidentTypeId = null;
        currentPage = 1;

        var allTypes = await ReportService.GetAllIncidentTypeAsync();
        incidentTypes = allTypes.Data ?? new List<IncidentTypeDto>();

        await LoadReportsAsync();
    }

    private async Task PrevPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            await LoadReportsAsync();
        }
    }

    private async Task NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            await LoadReportsAsync();
        }
    }

    private async Task GoToPage(int page)
    {
        if (page >= 1 && page <= totalPages)
        {
            currentPage = page;
            await LoadReportsAsync();
        }
    }
    private async Task PrintReport(Guid id)
    {
        var success = await ReportService.PrintReportAsync(id);
        if (!success)
        {
            await JS.InvokeVoidAsync("alert", _Localizer["PrintFailedMessage"].Value);
        }
    }

    private async Task OnIncidentTypeChanged(ChangeEventArgs e)
    {
        incidentTypeId = int.TryParse(e.Value?.ToString(), out var val) ? val : null;

        filterDate = null;
        currentPage = 1;

        await LoadReportsAsync();
    }
    private async Task OnIncidentChanged(ChangeEventArgs e)
    {
        filterType = int.TryParse(e.Value?.ToString(), out var val) ? val : null;

        incidentTypeId = null;
        filterDate = null;
        currentPage = 1;

        if (filterType.HasValue)
        {
            var typesRes = await ReportService.GetIncidentTypesByIncidentIdAsync(filterType.Value);

            if (typesRes.Success && typesRes.Data != null)
                incidentTypes = typesRes.Data;
            else
                incidentTypes = new List<IncidentTypeDto>();
        }
        else
        {
            var allTypes = await ReportService.GetAllIncidentTypeAsync();
            incidentTypes = allTypes.Data ?? new List<IncidentTypeDto>();
        }

        await LoadReportsAsync();
    }

    private async Task OnDateChanged(ChangeEventArgs e)
    {
        filterDate = DateTime.TryParse(e.Value?.ToString(), out var val) ? val : null;

        incidentTypeId = null;
        filterType = null;
        currentPage = 1;

        await LoadReportsAsync();
    }
    private async Task DeleteReport(Guid id)
    {
        bool confirm = await JS.InvokeAsync<bool>("confirm",
            _Localizer["ConfirmDeleteMessage"].Value);

        if (!confirm)
            return;

        bool success = await ReportService.DeleteReportAsync(id);

        if (success)
        {
            await JS.InvokeVoidAsync("alert", _Localizer["DeleteSuccessMessage"].Value);
            await LoadReportsAsync();
        }
        else
        {
            await JS.InvokeVoidAsync("alert", _Localizer["DeleteFailedMessage"].Value);
        }
    }

    private void ShowDetails(Guid reportId)
    {
        NavigationManager.NavigateTo($"/reportdetails/{reportId}");
    }



}
