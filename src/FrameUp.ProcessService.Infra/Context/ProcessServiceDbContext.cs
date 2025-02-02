using Microsoft.EntityFrameworkCore;

namespace FrameUp.ProcessService.Infra.Context;

public sealed class ProcessServiceDbContext : DbContext
{
    public ProcessServiceDbContext(DbContextOptions<ProcessServiceDbContext> dbContextOptions)
        : base(dbContextOptions)
    {
        Database.EnsureCreated();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProcessServiceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}