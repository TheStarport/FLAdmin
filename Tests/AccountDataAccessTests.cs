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
    private readonly IAccountDataAccess _accountDataAccess;
    private readonly EphemeralTestDatabase _fixture;

    private CancellationToken _token = CancellationToken.None;
    
    public AccountDataAccessTests()
    {
        _fixture = new EphemeralTestDatabase();
        _accountDataAccess =
            new AccountDataAccess(_fixture.DatabaseAccess, _fixture.Config, new NullLogger<AccountDataAccess>());
    }


    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task When_Grabbing_All_Accounts_Should_Count_Of_150()
    {
        var result = await _accountDataAccess.GetAccountsByFilter(a => true, _token, 1, 999);

        result.Count.Should().Be(150);
    }

    [Fact]
    public async Task When_Grabbing_Page_Of_Accounts_Should_Count_Of_50()
    {
        var result = await _accountDataAccess.GetAccountsByFilter(a => true, _token, 2, 50);

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

        var result = await _accountDataAccess.GetAccount("123abc456", _token);

        result.Match(
            Left: _ => false,
            Right: acc => acc.Id == fixedTestAccount.Id).Should().BeTrue();
    }

    [Fact]
    public async Task When_Getting_Nonexistent_Account_Should_Return_Account_NotFound()
    {
        var result = await _accountDataAccess.GetAccount("123", _token);

        result.IsLeft.Should().BeTrue();
    }

    [Fact]
    public async Task When_Creating_Account_With_Valid_Id_Should_Be_Created_Successfully()
    {
        var account = new Account
        {
            Id = "123abc"
        };

        var result = await _accountDataAccess.CreateAccounts(_token, account);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Adding_Duplicate_Account_Should_Return_AccountId_Already_Exists()
    {
        var account = new Account
        {
            Id = "123abc456"
        };

        var result = await _accountDataAccess.CreateAccounts(_token, account);

        result.Match(err => err == FLAdminErrorCode.AccountIdAlreadyExists, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Valid_Account_Should_Update_Successfully()
    {
        var account = new Account
        {
            Id = "123abc456",
            Cash = 1235
        };

        var result = await _accountDataAccess.UpdateAccount(account.ToBsonDocument(), _token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Non_Existing_Account_Should_Return_Account_Not_Found()
    {
        var account = new Account
        {
            Id = "123",
            Cash = 1235
        };

        var result = await _accountDataAccess.UpdateAccount(account.ToBsonDocument(), _token);

        result.Match(err => err == FLAdminErrorCode.AccountNotFound, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Existing_Account_Should_Delete_Successfully()
    {
        var result = await _accountDataAccess.DeleteAccounts(_token, "123abc456");

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Non_Existing_Account_Should_Return_Account_Does_Not_Exist()
    {
        var result = await _accountDataAccess.DeleteAccounts(_token, "123");

        result.Match(err => err == FLAdminErrorCode.AccountNotFound, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Editing_Field_On_Account_With_Correct_Type_Should_Update_Successfully()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123abc456", "cash", 12345, _token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Editing_Field_On_Account_With_Incorrect_Type_Should_Return_Type_Element_Mismatch()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123abc456", "cash", "bob", _token);

        result.Match(err => err == FLAdminErrorCode.AccountElementTypeMismatch, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Edit_Field_On_Non_Existing_Account_Should_Return_Account_Not_Found()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123", "cash", 123, _token);

        result.Match(err => err == FLAdminErrorCode.AccountNotFound, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Edit_Field_On_Non_Existing_Field_Should_Return_Field_Does_Not_Exist()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123abc456", "gabsdf", "bob", _token);

        result.Match(err => err == FLAdminErrorCode.AccountFieldDoesNotExist, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Delete_Field_Should_Delete_Successfully()
    {
        var result = await _accountDataAccess.RemoveFieldOnAccount("123abc456", "cash", _token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Delete_Field_From_Non_Existing_Account_Should_Return_Account_Not_Found()
    {
        var result = await _accountDataAccess.RemoveFieldOnAccount("123", "cash", _token);

        result.Match(err => err == FLAdminErrorCode.AccountNotFound, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Delete_Non_Existing_Field_Should_Return_Field_Does_Not_Exist()
    {
        var result = await _accountDataAccess.RemoveFieldOnAccount("123abc456", "gagds", _token);

        result.Match(err => err == FLAdminErrorCode.AccountFieldDoesNotExist, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Create_Valid_Field_On_Valid_Account_Should_Be_Successful()
    {
        var result = await _accountDataAccess.CreateNewFieldOnAccount("123abc456", "someNewField", 456, _token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Create_Field_On_Non_Existing_Account_Should_Account_Not_Found()
    {
        var result = await _accountDataAccess.CreateNewFieldOnAccount("123", "someNewField", 456, _token);

        result.Match(err => err == FLAdminErrorCode.AccountNotFound, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Create_Field_That_Already_Exists_Should_Return_Field_Already_Exists()
    {
        var result = await _accountDataAccess.CreateNewFieldOnAccount("123abc456", "cash", 456, _token);

        result.Match(err => err == FLAdminErrorCode.AccountFieldAlreadyExists, false).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Update_List_Field_On_Account_Should_Succeed()
    {
        var result = await _accountDataAccess.UpdateFieldOnAccount("123abc456", "characters", new List<ObjectId>(), _token);

        result.IsNone.Should().BeTrue();
    }


    //TODO: This function currently doesn't do this so when applying it finish this test case.
    [Fact]
    public void When_Attempting_To_Update_List_With_List_Of_Wrong_Type_Should_Fail()
    {
        /*var result =
            await _accountDataAccess.UpdateFieldOnAccount("123abc456", "characters", new List<int> {123, 456});

      //  result.Match(err => err == FLAdminError.AccountElementTypeMismatch, false).Should().BeTrue();*/
    }
}