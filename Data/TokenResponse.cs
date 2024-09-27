using System.Text.Json.Serialization;

namespace CRRService.Data;
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }=null!;
    [JsonPropertyName("signature")]
    public string Signature { get; set; }=null!;
    [JsonPropertyName("scope")]
    public string Scope { get; set; }=null!;
    [JsonPropertyName("instance_url")]
    public string InstanceUrl { get; set; }=null!;
    [JsonPropertyName("id")]
    public string Id { get; set; }=null!;
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }=null!;
    [JsonPropertyName("issued_at")]
    public string IssuedAt { get; set; }=null!;
}