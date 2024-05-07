namespace Logic.Storage;

using System.Linq.Expressions;
using Auth;
using Common.Auth;
using Common.Configuration;
using Common.Models;
using Common.Models.Database;
using Common.Models.Forms;
using Common.Storage;
using Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

public class AccountStorage : IAccountStorage
{
	private readonly ILogger<AccountStorage> _logger;
	private IMongoCollection<BsonDocument> AccountsCollection { get; }

	public AccountStorage(ILogger<AccountStorage> logger, FLAdminConfiguration configuration, IMongoManager mongo)
	{
		_logger = logger;
		AccountsCollection = mongo.GetCollection<BsonDocument>(configuration.Mongo.AccountCollectionName);
	}

	public async Task<long> GetAccountCountAsync()
	{
		var count = await AccountsCollection.Aggregate()
			.Match(FilterCollectionType(true))
			.Count()
			.FirstOrDefaultAsync();

		return count.Count;
	}

	public async Task<Account?> GetAccountAsync(Expression<Func<Account, bool>> filter)
	{
		var doc = await (await AccountsCollection.FindAsync(FilterCollectionType(true,
			Builders<Account>.Filter.Where(filter).RenderToBsonDocument()))).FirstOrDefaultAsync();
		return doc is null ? null : BsonSerializer.Deserialize<Account>(doc);
	}

	public async Task<Pagination<Account>?> GetAccountsAsync(int page, int amountPerPage = 20,
		Expression<Func<Account, bool>>? filter = null)
	{
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
			.Match(FilterCollectionType(true, filterDefinition.RenderToBsonDocument()))
			.As<Account>()
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

		return data is null
			? null
			: new Pagination<Account>() { CurrentPage = page, PageCount = totalPages, Data = data };
	}

	public async Task<Account?> GetAccountByIdAsync(string id) =>
		await AccountsCollection
			.Find(Builders<BsonDocument>.Filter.Eq("_id", id))
			.As<Account>()
			.FirstOrDefaultAsync();

	public async Task<Account?> CreateNewCharacterAsync(string accountId, Character character)
	{
		var filter = Builders<BsonDocument>.Filter.Eq("_id", accountId);
		var update = Builders<BsonDocument>.Update.Push("characters", character.Id);

		return BsonSerializer.Deserialize<Account>(await AccountsCollection.FindOneAndUpdateAsync(filter, update));
	}

	public async Task<Character?> GetCharacterByNameAsync(string name) =>
		await AccountsCollection.Aggregate()
			.Match(FilterCollectionType(true,
				Builders<BsonDocument>.Filter.Where(x => x["characterName"] == name).RenderToBsonDocument()))
			.As<Character>()
			.FirstOrDefaultAsync();

	public Task<Pagination<Account>?> SearchForCharacter(string characterName, int amountPerPage = 20) =>
		throw new NotImplementedException();

	public async Task SetAccountToken(Account account, string? token) =>
		await AccountsCollection.FindOneAndUpdateAsync(x => x["_id"] == account.Id,
			Builders<BsonDocument>.Update.Set("hashedToken", token));

	public async Task SetAccountRoles(Account account, IEnumerable<Role> webRoles, List<string> gameRoles) =>
		await AccountsCollection.FindOneAndUpdateAsync(x => x["_id"] == account.Id,
			Builders<BsonDocument>.Update
				.Set("webRoles", webRoles.Select(x => x.ToString()))
				.Set("gameRoles", gameRoles));


	public async Task<bool> InstanceAdminExists() =>
		await AccountsCollection!.CountDocumentsAsync(
			Builders<BsonDocument>.Filter.AnyStringIn("webRoles", Role.InstanceAdmin.ToString())) != 0;

	public async Task<string?> CreateInstanceAdmin(SignUp signUp)
	{
		if (await InstanceAdminExists())
		{
			return "Instance admin already exists.";
		}

		var name = signUp.Name.Trim();

		// Ensure that if the roles are messed up duplicate accounts cannot be created
		if (await AccountsCollection!.CountDocumentsAsync(Builders<BsonDocument>.Filter.Eq("name", name)) is not 0)
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
			WebRoles = [Role.Web.ToString(), Role.InstanceAdmin.ToString()],
		};

		await AccountsCollection.InsertOneAsync(account.ToBsonDocument());

		return null;
	}

	public IQueryable<Account> GetAdmins() =>
		AccountsCollection.AsQueryable()
			.AppendStage(PipelineStageDefinitionBuilder.Match<BsonDocument>(FilterCollectionType(true)))
			.As<BsonDocument, Account>();

	private static BsonDocument FilterCollectionType(bool accounts, BsonDocument? extraFilter = null)
	{
		var doc = new BsonDocument { { "_id", new BsonDocument { { "$type", accounts ? "string" : "objectId" }, } } };
		if (extraFilter is not null)
		{
			doc.Merge(extraFilter);
		}

		return doc;
	}
}
