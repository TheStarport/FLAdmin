namespace FlAdmin.Common.Auth;

public interface IKeyProvider
{
    byte[] GetSigningKey();
    byte[] GetEncryptionKey();
}