namespace FrameUp.ProcessService.Api.Configuration;

public record PostgreSQLSettings
{
    public required string ConnectionString { get; set; }
}