using System.Net.Http.Headers;
using System.Text.Json;
using CRRService.Data;

namespace CRRService.Services;
public class ApiService(IConfiguration configuration)
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _baseUrl = configuration.GetValue<string>("Settings:BaseUrl")?.ToString() ?? string.Empty;
    readonly string grantType = configuration.GetValue<string>("Settings:GrantType")?.ToString() ?? string.Empty;
    readonly string consumerKey = configuration.GetValue<string>("Settings:ConsumerKey")?.ToString() ?? string.Empty;
    readonly string consumerSecret = configuration.GetValue<string>("Settings:ConsumerSecret")?.ToString() ?? string.Empty;

    public async Task<string> GetTokenAsync()
    {
        try
        {
            Console.WriteLine("init trying to get token");
            string url = $"{_baseUrl}/services/oauth2/token?grant_type={grantType}&client_id={consumerKey}&client_secret={consumerSecret}";

            // Implement your token retrieval logic here
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                // response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
                return tokenResponse?.AccessToken!;
            }
            return string.Empty;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("An error occurred try to get token: " + ex.Message);
            return string.Empty;
        }
    }

    public async Task<CrrResponse> SendPostRequestAsync(string token, CrrRequest crrRequest)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(crrRequest), System.Text.Encoding.UTF8, "application/json");

            string url = $"{_baseUrl}/services/apexrest/ssUpdatePepRating";
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ApiStatus>(responseBody);
                //   response.EnsureSuccessStatusCode();
                if (data?.ErrorCode == "200")
                {
                    return new CrrResponse { CIF = crrRequest.CIF, Status = true };
                }
                return new CrrResponse { CIF = crrRequest.CIF, Status = false };
            }
            return new CrrResponse { CIF = crrRequest.CIF, Status = false };
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"An error occurred try to execute the post endpoint: {ex.Message} - InnerException: {ex?.InnerException}" );
            return new CrrResponse { CIF = string.Empty, Status = false };
        }
    }
}