using ProtoChat.DataAccess;
using ProtoChat.Domain.Commands;

namespace ProtoChat.Domain.Abstractions;

public interface IAppCommand
{
    Task<CommandResult> ExecuteAsync(CommandContext context);
}