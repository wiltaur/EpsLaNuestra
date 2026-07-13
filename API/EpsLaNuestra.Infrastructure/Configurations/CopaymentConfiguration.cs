using EpsLaNuestra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpsLaNuestra.Infrastructure.Configurations;

public class CopaymentConfiguration : IEntityTypeConfiguration<Copayment>
{
    public void Configure(EntityTypeBuilder<Copayment> entity)
    {
        entity.HasKey(e => e.PatientDocument);
        entity.ToTable("Copago");

        entity.Property(e => e.PatientDocument).HasColumnName("DocumentoPaciente").HasMaxLength(16);
        entity.Property(e => e.CopaymentValue).HasColumnName("ValorCopago");
    }
}