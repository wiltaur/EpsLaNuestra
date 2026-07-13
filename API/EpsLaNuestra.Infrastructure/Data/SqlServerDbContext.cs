using EpsLaNuestra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EpsLaNuestra.Infrastructure.Data;

public partial class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : DbContext(options)
{
    public virtual DbSet<Copayment> Copayments { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}