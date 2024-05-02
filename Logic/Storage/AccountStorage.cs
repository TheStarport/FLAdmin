namespace Logic.Storage;

using System.Linq.Expressions;
using Auth;
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

	public async Task SetAccountToken(Account account, string? token) =>
		await AccountsCollection.FindOneAndUpdateAsync(x => x.Id == account.Id,
			Builders<Account>.Update.Set(x => x.HashedToken, token));

	public async Task SetAccountRoles(Account account, IEnumerable<Role> webRoles, List<string> gameRoles) =>
		await AccountsCollection.FindOneAndUpdateAsync(x => x.Id == account.Id,
			Builders<Account>.Update
				.Set(x => x.WebRoles, webRoles.Select(x => x.ToString()))
				.Set(x => x.GameRoles, gameRoles));


	public async Task<bool> InstanceAdminExists()
	{
		await EnsureCollection();

		return await AccountsCollection!.CountDocumentsAsync(Builders<Account>.Filter.AnyStringIn(x => x.WebRoles, Role.InstanceAdmin.ToString())) != 0;
	}

	public async Task<string?> CreateInstanceAdmin(SignUp signUp)
	{
		await EnsureCollection();

		if (await InstanceAdminExists())
		{
			return "Instance admin already exists.";
		}

		var name = signUp.Name.Trim();

		// Ensure that if the roles are messed up duplicate accounts cannot be created
		if (await AccountsCollection!.CountDocumentsAsync(Builders<Account>.Filter.Where(x => x.Username == name)) is
			not 0)
		{
			return "An account with that name already exists.";
		}

		var password = signUp.Password.Trim();
		byte[]? salt = null;
		var account = new Account()
		{
			Id = Guid.NewGuid().ToString(),
			PasswordHash = PasswordHasher.GenerateSaltedHash(password, ref salt),
			Salt = salt,
			Username = name,
			WebRoles = new List<string> { Role.Web.ToString(), Role.InstanceAdmin.ToString() },
		};

		await AccountsCollection.InsertOneAsync(account);

		return null;
	}

	public IQueryable<Account> GetAdmins() =>
		AccountsCollection.AsQueryable()
			.Where(account => account.GameRoles.Count != 0 || account.WebRoles.Count != 0);

	private async Task EnsureCollection() => AccountsCollection ??= await _mongo.GetCollectionAsync<Account>(_configuration.Mongo.AccountCollectionName);
}
