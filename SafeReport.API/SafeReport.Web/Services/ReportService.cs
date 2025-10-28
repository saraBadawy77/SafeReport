using Microsoft.JSInterop;
using SafeReport.Web.DTOs;
using SafeReport.Web.Interfaces;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;


namespace SafeReport.Web.Services;

public class ReportService : IReportService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    public ReportService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }
    private void AddCultureHeader()
    {
        var currentCulture = System.Globalization.CultureInfo.CurrentCulture.Name; // ex: "ar-EG" or "en-US"

        if (_httpClient.DefaultRequestHeaders.Contains("Accept-Language"))
            _httpClient.DefaultRequestHeaders.Remove("Accept-Language");

        _httpClient.DefaultRequestHeaders.Add("Accept-Language", currentCulture);
    }
    public async Task<Response<PagedResultDto>> GetAllReportsAsync(ReportFilterDto filter)
    {
        AddCultureHeader();
        try
        {
            var httpResponse = await _httpClient.PostAsJsonAsync("api/Report/GetAll", filter);

            if (httpResponse.IsSuccessStatusCode)
            {
                var result = await httpResponse.Content.ReadFromJsonAsync<Response<PagedResultDto>>();
                if (result != null && result.Data != null)
                    return Response<PagedResultDto>.SuccessResponse(result.Data, "Request succeeded");

                return Response<PagedResultDto>.SuccessResponse(null,"Request succeeded but no data was returned.");
            }

            return Response<PagedResultDto>.FailResponse("Failed to fetch reports or no data found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching reports: {ex.Message}");
            return Response<PagedResultDto>.FailResponse("An unexpected error occurred while fetching reports.");
        }

    }
    public async Task<List<Response<Incident>>> GetAllIncidentsAsync()
    {
        AddCultureHeader();
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Response<IEnumerable<IncidentDto>>>("api/Incident");

            if (response != null && response.Data != null)
            {
                var incidentTypes = response.Data.Select(d => new Incident
                {
                    Id = d.Id,
                    Name = d.Name
                });
                var responseList = incidentTypes
                    .Select(it => Response<Incident>.SuccessResponse(it))
                    .ToList();

                return responseList;
            }

            return new List<Response<Incident>>
            {
               Response<Incident>.SuccessResponse(null, "No incidents found")
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching incidents: {ex.Message}");
            return new List<Response<Incident>>
                {
                    Response<Incident>.FailResponse("Failed to fetch incidents.")
                };
        }
    }
    public async Task<bool> DeleteReportAsync(Guid id)
    {
        AddCultureHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Report/SoftDelete/{id}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<string>>();
                return result != null && result.Success;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception deleting report: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> PrintReportAsync(Guid id)
    {
        AddCultureHeader();
        try
        {
            var response = await _httpClient.GetAsync($"api/Report/PrintReport/{id}");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                if (pdfBytes.Length == 0)
                {
                    Console.WriteLine("Empty PDF received.");
                    return false;
                }
                var base64 = Convert.ToBase64String(pdfBytes);
                await _jsRuntime.InvokeVoidAsync("pdfHelper.openPdf", base64, $"Report_{id}.pdf");
                return true;
            };
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Exception while printing report: {ex.Message}");
            return false;
        }
    }
    public async Task<Response<ReportDTO>> ShowReportDetails(Guid reportId)
    {
        AddCultureHeader();
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Response<ReportDTO>>($"api/Report/GetById/{reportId}");
            if (response != null && response.Success)
            {
                return response;
            }
            else
            {
                return Response<ReportDTO>.FailResponse("Failed to load report details.");
            }
        }
        catch (Exception ex)
        {
            return Response<ReportDTO>.FailResponse($"Error fetching report details: {ex.Message}");
        }
    }
    public async Task<Response<List<ReportDTO>>> GetNewReportsAsync(DateTime lastVisitUtc)
    {
        AddCultureHeader();
        string url = $"api/Report/GetNewReports?lastVisitUtc={Uri.EscapeDataString(lastVisitUtc.ToString("o"))}";

        try
        {
            var httpResponse = await _httpClient.GetAsync(url);

            if (!httpResponse.IsSuccessStatusCode)
                return Response<List<ReportDTO>>.FailResponse($"Failed to fetch new reports. Status code: {httpResponse.StatusCode}");

            var response = await httpResponse.Content.ReadFromJsonAsync<Response<List<ReportDTO>>>();
            return response ?? Response<List<ReportDTO>>.SuccessResponse(new List<ReportDTO>());
        }
        catch (Exception ex)
        {
            return Response<List<ReportDTO>>.FailResponse($"Error fetching new reports: {ex.Message}");
        }
    }
    public async Task<Response<List<IncidentTypeDto>>> GetAllIncidentTypeAsync()
    {
        AddCultureHeader();
        try
        {
            var httpResponse = await _httpClient.GetAsync("api/IncidentType/GetAllTypes");

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Response<List<IncidentTypeDto>>.FailResponse(
                    $"Request failed: {httpResponse.StatusCode}"
                );
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<Response<IEnumerable<IncidentTypeDto>>>();

            if (response != null && response.Data != null)
            {
                return Response<List<IncidentTypeDto>>.SuccessResponse(response.Data.ToList());
            }

            return Response<List<IncidentTypeDto>>.SuccessResponse(null,"No incident types found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching incidents: {ex.Message}");
            return Response<List<IncidentTypeDto>>.FailResponse("Failed to fetch incident types");
        }
    }
    public async Task<Response<List<IncidentTypeDto>>> GetIncidentTypesByIncidentIdAsync(int incidentId)
    {
        AddCultureHeader();
        try
        {
            var httpResponse = await _httpClient.GetAsync($"api/IncidentType/GetByIncidentId{incidentId}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Response<List<IncidentTypeDto>>.FailResponse(
                    $"Request failed: {httpResponse.StatusCode}"
                );
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<Response<IEnumerable<IncidentTypeDto>>>();

            if (response != null && response.Data != null)
            {
                return Response<List<IncidentTypeDto>>.SuccessResponse(
                    response.Data.ToList(),
                    response.Message
                );
            }

            return Response<List<IncidentTypeDto>>.SuccessResponse(null,"No incident types found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching incident types by incidentId: {ex.Message}");
            return Response<List<IncidentTypeDto>>.FailResponse("Failed to fetch data");
        }
    }







}
