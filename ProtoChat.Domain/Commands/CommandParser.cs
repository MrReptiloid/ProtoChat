using ProtoChat.Domain.Abstractions;
using ProtoChat.Proto;

namespace ProtoChat.Domain.Commands;

public class CommandParser
{
    public static IAppCommand? Parse(byte[] commandBytes)
    {
        Command? envelope = Command.Parser.ParseFrom(commandBytes);
        
        return envelope.PayloadCase switch
        {
            Command.PayloadOneofCase.RegisterUser => 
                new RegisterUserCommand(envelope.RegisterUser.Username, envelope.RegisterUser.Password),
            Command.PayloadOneofCase.LoginUser => 
                new LoginUserCommand(envelope.LoginUser.Username, envelope.LoginUser.Password),
            
            Command.PayloadOneofCase.None => null
        };
    }
}