using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Logic.DataAccess;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests;

//IDatabaseAccess databaseAccess, FlAdminConfig config, Logger<AccountDataAccess> logger

public class AccountDataAccessTests : IClassFixture<EphemeralTestDatabase>
{
    private readonly EphemeralTestDatabase _fixture;
    private readonly IAccountDataAccess _accountDataAccess;

    public AccountDataAccessTests(EphemeralTestDatabase fixture)
    {
        _fixture = fixture;
        _accountDataAccess =
            new AccountDataAccess(fixture.DatabaseAccess, _fixture.Config, new NullLogger<AccountDataAccess>());
    }

    [Fact]
    public async Task When_Creating_Account_With_Valid_Id_Should_Be_Created_Successfully()
    {
        var account = new Account()
        {
            Id = "123abc"
        };

        var result = await _accountDataAccess.CreateAccounts(account);

        result.IsNone.Should().BeTrue();
    }
    
    
}