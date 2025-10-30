using System.Linq.Expressions;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface IAccountDataAccess
{
    /// <summary>
    /// Creates accounts on the database
    /// </summary>
    /// <param name="token"></param>
    /// <param name="accounts">list of accounts to be added.</param>
    /// <returns>Error object if failed.</returns>
    Task<Option<ErrorResult>> CreateAccounts(CancellationToken token, params Account[] accounts);
    
    /// <summary>
    /// Update an account by replacing one with the specified accountID with the provided document.
    /// </summary>
    /// <param name="account">new account doc that will replace the old one.</param>
    /// <param name="token"></param>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> UpdateAccount(BsonDocument account, CancellationToken token);
    /// <summary>
    /// Deletes accounts of the specified ids.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="ids">Ids to delete.</param>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> DeleteAccounts(CancellationToken token, params string[] ids);
    /// <summary>
    /// Gets account object of specified id on the database.
    /// </summary>
    /// <param name="accountId">id of account to fetch.</param>
    /// <param name="token"></param>
    /// <returns>Account if successful, Error result if failed.</returns>
    Task<Either<ErrorResult, Account>> GetAccount(string accountId, CancellationToken token);
    
    /// <summary>
    /// Updates a field on a specified account.
    /// </summary>
    /// <param name="accountId">Id of account to edit.</param>
    /// <param name="fieldName">Name of field being edited.</param>
    /// <param name="value">Value the field will be updated to.</param>
    /// <param name="token"></param>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token);
    /// <summary>
    /// Creates a new field on an account.
    /// </summary>
    /// <param name="accountId">Id of account to edit.</param>
    /// <param name="fieldName">Name of field to be added.</param>
    /// <param name="value">Value the field will be set to. </param>
    /// <param name="token"></param>
    /// <typeparam name="T">Type of the value/field.</typeparam>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token);
    
    /// <summary>
    /// Removes a field of a provided account.
    /// </summary>
    /// <param name="accountId">id of account that will have the field deleted.</param>
    /// <param name="fieldName">name of field to be removed.</param>
    /// <param name="token"></param>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> RemoveFieldOnAccount(string accountId, string fieldName, CancellationToken token);

    
    /// <summary>
    /// Gets a list of accounts that match a filter provided by a BSON document.
    /// </summary>
    /// <param name="filter">Filter document.</param>
    /// <param name="token"></param>
    /// <param name="page">page number to be grabbed.</param>
    /// <param name="pageSize"></param>
    /// <returns>List of accounts, empty list if it fails.</returns>
    Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, CancellationToken token,
        int page = 1, int pageSize = 100);
    Task<Option<ErrorResult>> ReplaceAccount(Account account, CancellationToken token);
}