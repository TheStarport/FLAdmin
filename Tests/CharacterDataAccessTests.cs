using FlAdmin.Common.DataAccess;
using FlAdmin.Logic.DataAccess;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlAdmin.Tests;


[Collection("DatabaseTests")]
public class CharacterDataAccessTests : IDisposable
{
    
    private readonly EphemeralTestDatabase _fixture;
    private readonly ICharacterDataAccess _characterDataAccess;

    public CharacterDataAccessTests()
    {
        _fixture = new EphemeralTestDatabase();
        _characterDataAccess = new CharacterDataAccess(_fixture.DatabaseAccess, _fixture.Config, new NullLogger<CharacterDataAccess>());
    }
    
    
    
    public void Dispose()
    {
        _fixture.Dispose();
    }
}