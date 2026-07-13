namespace EpsLaNuestra.Domain.Entities;

public partial class Copayment
{
    public string PatientDocument { get; set; } = string.Empty;
    public int CopaymentValue { get; set; }
}