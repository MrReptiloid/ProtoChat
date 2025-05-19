namespace ProtoChat.Domain.Commands;

public class CommandResult
{
    public bool Success { get; init; }
    public string Message { get; init; }
    
    public static CommandResult Ok(string? message = null) => new() { Success = true, Message = message ?? "" };
    public static CommandResult Fail(string message) => new() { Success = false, Message = message };
}