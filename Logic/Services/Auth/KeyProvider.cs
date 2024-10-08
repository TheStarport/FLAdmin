﻿using System.Security.Cryptography;
using FlAdmin.Common.Auth;

namespace FlAdmin.Logic.Services.Auth;

public class KeyProvider : IKeyProvider
{
    private readonly string _encryptionPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FLAdmin",
            "encryption.key");

    private readonly string _signingPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FLAdmin",
            "signing.key");

    private byte[] _encryptionKey = Array.Empty<byte>();


    private byte[] _signingKey = Array.Empty<byte>();

    public byte[] GetSigningKey()
    {
        if (_signingKey.Length == 0)
        {
            ReadKey(KeyType.SigningKey);
            return _signingKey;
        }

        return _signingKey;
    }

    public byte[] GetEncryptionKey()
    {
        if (!_encryptionKey.Any())
        {
            ReadKey(KeyType.EncryptionKey);
            return _encryptionKey;
        }

        return _encryptionKey;
    }

    private static byte[] GenerateKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        return aes.Key;
    }

    private void WriteKey(string path, KeyType type)
    {
        try
        {
            FileInfo info = new(path);
            if (!info.Directory!.Exists) info.Directory.Create();

            using var file = File.Open(path, FileMode.OpenOrCreate);
            if (file is null) throw new IOException("Unable to load or write security.key");

            var key = GenerateKey();
            foreach (var b in key) file.WriteByte(b);

            switch (type)
            {
                case KeyType.EncryptionKey:
                    _encryptionKey = key;
                    break;
                case KeyType.SigningKey:
                    _signingKey = key;
                    break;
            }

            file.Close();
        }
        catch (Exception ex)
        {
            //	_logger.LogError(ex, "Unable to read security.key");
        }
    }

    private void ReadKey(KeyType type)
    {
        var path = type switch
        {
            KeyType.EncryptionKey => _encryptionPath,
            KeyType.SigningKey => _signingPath,
            _ => throw new NotImplementedException()
        };

        try
        {
            FileInfo info = new(path);
            if (!info.Directory!.Exists)
            {
                info.Directory.Create();
                WriteKey(path, type);
                return;
            }

            if (!info.Exists)
            {
                WriteKey(path, type);
                return;
            }

            switch (type)
            {
                case KeyType.EncryptionKey:
                    _encryptionKey = File.ReadAllBytes(path);
                    break;
                case KeyType.SigningKey:
                    _signingKey = File.ReadAllBytes(path);
                    break;
            }
        }
        catch (Exception ex)
        {
            //	_logger.LogError(ex, "Unable to read security.key");
        }
    }

    private enum KeyType
    {
        EncryptionKey,
        SigningKey
    }
}