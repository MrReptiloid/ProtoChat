using System.Security.Cryptography;
using Akka.Actor;

namespace ProtoChat.Actors;

public class AesGcmActor : ReceiveActor
{
    public AesGcmActor()
    {
        Receive<EncryptRequest>(msg => HandleEncrypt(msg));
        Receive<DecryptRequest>(msg => HandleDecrypt(msg));
    }
    
    private void HandleEncrypt(EncryptRequest msg)
    {
        byte[] key = RandomNumberGenerator.GetBytes(32);
        using AesGcm aesGcm = new(key);
        
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] cipherText = new byte[msg.Message.Length];
        byte[] tag = new byte[16];
        
        aesGcm.Encrypt(nonce, System.Text.Encoding.UTF8.GetBytes(msg.Message), cipherText, tag);
        
        Sender.Tell(new EncryptResponse(cipherText, nonce, tag, key));
    }
    
    private void HandleDecrypt(DecryptRequest msg)
    {
        using AesGcm aesGcm = new(msg.Key);
        
        byte[] plainText = new byte[msg.CipherText.Length];
        aesGcm.Decrypt(msg.Nonce, msg.CipherText, msg.Tag, plainText);
        
        string message = System.Text.Encoding.UTF8.GetString(plainText);
        Sender.Tell(new DecryptResponse(message));
    }

    public static Props Props() =>
        Akka.Actor.Props.Create(() => new AesGcmActor());
        
    public record EncryptRequest(string Message);
    public record EncryptResponse(byte[] CipherText, byte[] Nonce, byte[] Tag, byte[] Key);
    public record DecryptRequest(byte[] CipherText, byte[] Nonce, byte[] Tag, byte[] Key);
    public record DecryptResponse(string Message);
}