using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Logic.DataAccess;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;

namespace FlAdmin.Tests;

[Collection("DatabaseTests")]
public class AccountDataAccessTests : IDisposable
{
    private readonly EphemeralTestDatabase _fixture;
    private readonly IAccountDataAccess _accountDataAccess;
    
    public AccountDataAccessTests()
    {
        _fixture = new EphemeralTestDatabase();
        _accountDataAccess =
            new AccountDataAccess(_fixture.DatabaseAccess, _fixture.Config, new NullLogger<AccountDataAccess>());
    }
    
    [Fact]
    public async Task When_Grabbing_All_Accounts_Should_Count_Of_150()
    {
        var result = await _accountDataAccess.GetAccountsByFilter(a => true, 1, 999);

        result.Count.Should().Be(150);
    }

    [Fact]
    public async Task When_Grabbing_Page_Of_Accounts_Should_Count_Of_50()
    {
        var result = await _accountDataAccess.GetAccountsByFilter(a => true, 2, 50);

        result.Count.Should().Be(50);
    }

    [Fact]
    public async Task When_Getting_Valid_Account_Should_Successfully_Return_Correct_Account()
    {
        var fixedTestAccount = new Account
        {
            Id = "123abc456",
            LastOnline = DateTime.Now - TimeSpan.FromDays(30),
            Cash = 12345678
        };

        var result = await _accountDataAccess.GetAccount("123abc456");

        result.Match(
            Left: err => false,
            Right: acc => acc.Id == fixedTestAccount.Id).Should().BeTrue();
    }

    [Fact]
    public async Task When_Getting_Nonexistent_Account_Should_Return_Account_NotFound()
    {
        var result = await _accountDataAccess.GetAccount("123");
        
        result.IsLeft.Should().BeTrue();
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

    [Fact]
    public async Task When_Adding_Duplicate_Account_Should_Return_AccountId_Already_Exists()
    {
        var account = new Account()
        {
            Id = "123abc456"
        };

        var result = await _accountDataAccess.CreateAccounts(account);

        result.Match(err => err == AccountError.AccountIdAlreadyExists, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Valid_Account_Should_Update_Successfully()
    {
        var account = new Account()
        {
            Id = "123abc456",
            Cash = 1235
        };

        var result = await _accountDataAccess.UpdateAccount(account.ToBsonDocument());

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Non_Existing_Account_Should_Return_Account_Not_Found()
    {
        var account = new Account()
        {
            Id = "123",
            Cash = 1235
        };

        var result = await _accountDataAccess.UpdateAccount(account.ToBsonDocument());

        result.Match(err => err == AccountError.AccountNotFound, false).Should().BeTrue();
    }
    
    [Fact]
    public async Task When_Deleting_Existing_Account_Should_Delete_Successfully()
    {
        var result = await _accountDataAccess.DeleteAccounts("123abc456");

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Non_Existing_Account_Should_Return_Account_Does_Not_Exist()
    {
        var result = await _accountDataAccess.DeleteAccounts("123");

        result.Match(err => err == AccountError.AccountNotFound, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Delete_SuperAdmin_Should_Return_Account_Is_Protected()
    {
        var result = await _accountDataAccess.DeleteAccounts("SuperAdmin");

        result.Match(err => err == AccountError.AccountIsProtected, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Editing_Field_On_Account_With_Correct_Type_Should_Update_Successfully()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123abc456", "cash", 12345);
        
        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Editing_Field_On_Account_With_Incorrect_Type_Should_Return_Type_Element_Mismatch()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123abc456", "cash", "bob");
        
        result.Match(err => err == AccountError.ElementTypeMismatch, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Edit_Field_On_Non_Existing_Account_Should_Return_Account_Not_Found()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123", "cash", 123);
        
        result.Match(err => err == AccountError.AccountNotFound, false).Should().BeTrue();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}