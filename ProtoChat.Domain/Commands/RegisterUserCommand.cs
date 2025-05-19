using ProtoChat.DataAccess;
using ProtoChat.Domain.Abstractions;

namespace ProtoChat.Domain.Commands;

public class RegisterUserCommand : IAppCommand
{
    public string UserName { get; init; }
    public string Password { get; init; }
    
    public RegisterUserCommand(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
    
    public async Task<CommandResult> ExecuteAsync(CommandContext context)
    {
        if (string.IsNullOrWhiteSpace(UserName))
            return CommandResult.Fail("User name cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(Password))
            return CommandResult.Fail("Password cannot be empty.");
        
        var result = await context.UserStore.RegisterUserAsync(UserName, Password);
        return result ? CommandResult.Ok() : CommandResult.Fail("User registration failed.");
    }
}