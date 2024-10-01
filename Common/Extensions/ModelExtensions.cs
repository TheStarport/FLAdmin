using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;

namespace FlAdmin.Common.Extensions;

public static class ModelExtensions
{
    public static AccountModel ToModel(this Account account)
    {
        return new AccountModel
        {
            Id = account.Id,
            Characters = account.Characters,
            Cash = account.Cash,
            ScheduledUnbanDate = account.ScheduledUnbanDate,
            GameRoles = account.GameRoles.ToList(),
            WebRoles = account.WebRoles.ToList(),
            LastOnline = account.LastOnline,
            Extra = account.Extra
        };
    }

    public static Account ToDatabaseAccount(this AccountModel account)
    {
        return new Account
        {
            Id = account.Id,
            Characters = account.Characters,
            Cash = account.Cash,
            ScheduledUnbanDate = account.ScheduledUnbanDate,
            GameRoles = account.GameRoles.ToHashSet(),
            WebRoles = account.WebRoles.ToHashSet(),
            LastOnline = account.LastOnline,
            Extra = account.Extra
        };
    }

    public static List<AccountModel> ToAccountModelList(this List<Account> accounts)
    {
        var list = new List<AccountModel>();
        accounts.ForEach(acc => list.Add(acc.ToModel()));
        return list;
    }
}