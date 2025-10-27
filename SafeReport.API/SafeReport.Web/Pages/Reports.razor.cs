using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SafeReport.Web.DTOs;
using SafeReport.Web.Services;

namespace SafeReport.Web.Pages;

public partial class Reports
{
    private List<ReportDTO> pagedReports = new();
    private List<IncidentType> reportTypes = new();
    [Inject]
    Microsoft.JSInterop.IJSRuntime JS { get; set; }
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

        await LoadReportsAsync();
    }

    private async Task LoadReportsAsync()
    {
        var filter = new ReportFilterDto
        {
            IncidentId = filterType,
            CreatedDate = filterDate,
            PageNumber = currentPage,
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
        currentPage = 1;
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
            await JS.InvokeVoidAsync("alert", "Failed to open PDF report.");
        }
    }

    private async Task OnFilterChanged(ChangeEventArgs e)
    {
        currentPage = 1;
        await LoadReportsAsync();
    }


    private async Task DeleteReport(Guid id)
    {
        bool confirm = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete this report?");
        if (!confirm)
            return; 
        bool success = await ReportService.DeleteReportAsync(id);
        if (success)
        {
            await JS.InvokeVoidAsync("alert", "Report deleted successfully ✅");
            await LoadReportsAsync(); 
        }
        else
        {
            await JS.InvokeVoidAsync("alert", "Failed to delete the report ❌");
        }
    }
    private void ShowDetails(Guid reportId)
    {
        NavigationManager.NavigateTo($"/reportdetails/{reportId}");
    }



}
