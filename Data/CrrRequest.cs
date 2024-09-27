namespace CRRService.Data;
public class CrrRequest
{
    public string CIF { get; set; }=string.Empty;
    public string RiskClassification { get; set; }=string.Empty;
    public string PepCategory { get; set; }=string.Empty;
    public string ReasonForClassification { get; set; }=string.Empty;
}