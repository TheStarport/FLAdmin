namespace Logic.Storage;

using System.Linq.Expressions;
using Common.Auth;
using Common.Configuration;
using Common.Models;
using Common.Models.Database;
using Common.Models.Forms;
using Common.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class AccountStorage : IAccountStorage
{
	private readonly ILogger<AccountStorage> _logger;
	private readonly FLAdminConfiguration _configuration;
	private readonly IMongoManager _mongo;

	private IMongoCollection<Account>? AccountsCollection { get; set; }

	public AccountStorage(ILogger<AccountStorage> logger, FLAdminConfiguration configuration, IMongoManager mongo)
	{
		_logger = logger;
		_configuration = configuration;
		_mongo = mongo;
	}

	public async Task<long> GetAccountCountAsync()
	{
		await EnsureCollection();

		var count = await AccountsCollection.Aggregate()
			.Count()
			.FirstOrDefaultAsync();

		return count.Count;
	}

	public async Task<int> GetCharacterCountAsync()
	{
		await EnsureCollection();

		return await AccountsCollection.Aggregate()
			.Project(Builders<Account>.Projection.Expression(x => x.Characters.Count))
			.FirstOrDefaultAsync();
	}

	public async Task<Pagination<Account>?> GetAccountsAsync(int page, int amountPerPage = 20, Expression<Func<Account, bool>>? filter = null)
	{
		await EnsureCollection();

		_logger.LogDebug("Fetching accounts by page. Page: {Page}, AmountPerPage: {Amount}", page, amountPerPage);

		var countFacet = AggregateFacet.Create("count",
			PipelineDefinition<Account, AggregateCountResult>.Create(new[]
			{
				PipelineStageDefinitionBuilder.Count<Account>()
			}));

		var dataFacet = AggregateFacet.Create("data",
		PipelineDefinition<Account, Account>.Create(new[]
		{
				PipelineStageDefinitionBuilder.Skip<Account>((page - 1) * amountPerPage),
				PipelineStageDefinitionBuilder.Limit<Account>(amountPerPage),
		}));

		var filterDefinition = filter is null ? Builders<Account>.Filter.Empty : Builders<Account>.Filter.Where(filter);

		var aggregation = await AccountsCollection.Aggregate()
			.Match(filterDefinition)
			.Facet(countFacet, dataFacet)
			.ToListAsync();

		var count = aggregation?.ElementAtOrDefault(0)
			?.Facets.First(x => x.Name == "count")
			.Output<AggregateCountResult>()
			?.Count ?? 0;

		var totalPages = count / amountPerPage;

		var data = aggregation?.ElementAtOrDefault(0)
			?.Facets.First(x => x.Name == "data")
			.Output<Account>();

		return data is null ? null : new Pagination<Account>()
		{
			CurrentPage = page,
			PageCount = totalPages,
			Data = data
		};
	}

	public async Task<Account?> GetAccountByIdAsync(string id)
	{
		await EnsureCollection();

		return await AccountsCollection
			.Find(Builders<Account>.Filter.Eq(account => account.Id, id))
			.FirstOrDefaultAsync();
	}

	public async Task<Account?> CreateNewCharacterAsync(string accountId, Character character)
	{
		await EnsureCollection();

		var filter = Builders<Account>.Filter.Eq(account => account.Id, accountId);
		var update = Builders<Account>.Update.Push(account => account.Characters, character);

		return await AccountsCollection.FindOneAndUpdateAsync(filter, update);
	}

	public async Task<Character?> GetCharacterByNameAsync(string name)
	{
		await EnsureCollection();

		return await AccountsCollection.Aggregate()
			.Match(account => account.Characters.Any(x => x.CharacterName == name))
			.Project(Builders<Account>.Projection.Expression(x => x.Characters.Where(y => y.CharacterName == name)))
			.ReplaceRoot(q => q.ElementAt(0))
			.FirstOrDefaultAsync();
	}

	public Task<Pagination<Account>?> SearchForCharacter(string characterName, int amountPerPage = 20) => throw new NotImplementedException();
	public Task CreateSuperAdmin(SignUp signUp)
	{
		return Task.CompletedTask;
	}

	private async Task EnsureCollection() => AccountsCollection ??= await _mongo.GetCollectionAsync<Account>(_configuration.Mongo.AccountCollectionName);
}
