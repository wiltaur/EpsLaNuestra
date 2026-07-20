namespace EpsLaNuestra.BlazorApp.Models;

public class AdmissionModel
{
    public string NumberId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Copayment { get; set; }
    public DateTime RegisterDate { get; set; }
    public bool IsUpdate { get; set; }
    public bool JustUpdated { get; set; }
}