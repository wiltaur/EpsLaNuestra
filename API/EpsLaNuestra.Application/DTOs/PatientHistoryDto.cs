namespace EpsLaNuestra.Application.DTOs;

public partial class PatientHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string ResourceType {  get; set; } = string.Empty;
    public int Copayment {  get; set; }
}