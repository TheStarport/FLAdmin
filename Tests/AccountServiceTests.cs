using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Logic.Services.Database;
using FlAdmin.Tests.DataAccessMocks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlAdmin.Tests;

public class AccountServiceTests : IDisposable
{
    AccountDataAccessMock _accountDataAccess;
    private AccountService _service;
    private FlAdminConfig config;

    
    public AccountServiceTests()
    {
        _accountDataAccess = new AccountDataAccessMock();
        config = new FlAdminConfig();
        _service = new AccountService(_accountDataAccess, config, new NullLogger<AccountService>());
    }

    [Fact]
    public async Task When_Adding_Valid_Id_Account_Should_Succeed()
    {
        var account = new Account()
        {
            Id = "123abc"
        };

        var result = await _service.CreateAccounts(account);

        result.IsNone.Should().BeTrue();
    }
    
    
    
    
    public void Dispose()
    {
        _accountDataAccess.Dispose();
    }
}