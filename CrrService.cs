using System.Text.Json;
using CRRService.Data;
using CRRService.Services;

namespace CRRService;

public class CrrService : BackgroundService
{
    private readonly ILogger<CrrService> _logger;
    private readonly IConfiguration _configuration;

    public CrrService(ILogger<CrrService> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }
    private static string _token = string.Empty;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ApiService apiService = new ApiService(_configuration);
        _token = await apiService.GetTokenAsync();
        List<string> SelectedCustomerIds = new();
        string selectedIds = string.Empty;
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DbService dbService = new(_configuration);
                CrrRequest apiRequest = new CrrRequest();
                try
                {
                    selectedIds = SelectedCustomerIds.Count > 0 ? string.Join(",", SelectedCustomerIds) : string.Empty;

                    var newCrrData = await dbService.GetAllNewCustomerRiskRatingsAsync(selectedIds);
                    Console.WriteLine($"New CRR counts: {newCrrData.Count()}");
                    foreach (var crr in newCrrData)
                    {
                         //await dbService.UpdateCustomerRiskRatingsStatusAsync(crr);
                        if (!string.IsNullOrEmpty(_token))
                        {
                            Console.WriteLine($"Init Api for CRR : {crr.CIF}");
                            var response = await apiService.SendPostRequestAsync(_token, apiRequest);
                            Console.WriteLine($"Api response for CRR : {crr.CIF} status is : {response.Status}");
                            if (response != null && response.Status == true)
                            {
                                Console.WriteLine($"Api executed successfully for CRR : {crr.CIF}");
                                crr.IsSent = true;
                                crr.DateSent = DateTimeOffset.Now;
                                var result = await dbService.UpdateCustomerRiskRatingsStatusAsync(crr);
                                if (result > 0)
                                {
                                    Console.WriteLine($"CRR : {crr.CIF} data save successfully");
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("401"))
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    Console.WriteLine("Getting a new token");
                    // Token is invalid, get a new one
                    _token = await apiService.GetTokenAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    break;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(5000);
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("An error occurred starting the application: " + ex.Message);
        }
    }
}
