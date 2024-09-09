using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;

namespace FlAdmin.Common.Extensions;

public static class ModelExtensions
{
    public static AccountModel ToAccountModel(this Account account)
    {
        return new AccountModel()
        {
            Id = account.Id,
            Characters = account.Characters,
            Cash = account.Cash,
            ScheduledUnbanDate = account.ScheduledUnbanDate,
            GameRoles = account.GameRoles,
            WebRoles = account.WebRoles,
            LastOnline = account.LastOnline,
            Extra = account.Extra
        };
    }

    public static Account ToDatabaseAccount(this AccountModel account)
    {
        return new Account()
        {
            Id = account.Id,
            Characters = account.Characters,
            Cash = account.Cash,
            ScheduledUnbanDate = account.ScheduledUnbanDate,
            GameRoles = account.GameRoles,
            WebRoles = account.WebRoles,
            LastOnline = account.LastOnline,
            Extra = account.Extra
        };
    }
    
}