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
        byte[] cipherPayload = new byte[msg.Payload.Length];
        byte[] tag = new byte[16];
        
        aesGcm.Encrypt(nonce, msg.Payload, cipherPayload, tag);
        
        Sender.Tell(new EncryptResponse(cipherPayload, nonce, tag, key));
    }
    
    private void HandleDecrypt(DecryptRequest msg)
    {
        using AesGcm aesGcm = new(msg.Key);
        
        byte[] payload = new byte[msg.CipherPayload.Length];
        aesGcm.Decrypt(msg.Nonce, msg.CipherPayload, msg.Tag, payload);
        
        Sender.Tell(new DecryptResponse(payload));
    }

    public static Props Props() =>
        Akka.Actor.Props.Create(() => new AesGcmActor());
        
    public record EncryptRequest(byte[] Payload);
    public record EncryptResponse(byte[] CipherPayload, byte[] Nonce, byte[] Tag, byte[] Key);
    public record DecryptRequest(byte[] CipherPayload, byte[] Nonce, byte[] Tag, byte[] Key);
    public record DecryptResponse(byte[] Payload);
}