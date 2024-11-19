using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services.Database;
using FlAdmin.Tests.DataAccessMocks;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlAdmin.Tests;

public class CharacterServiceTests
{
    private CharacterService _characterService;

    public CharacterServiceTests()
    {
        ICharacterDataAccess characterDataAccess = new CharacterDataAccessMock();
        IAccountDataAccess accountDataAccess = new AccountDataAccessMock();
        IValidationService validationService = new ValidationServiceMock();


        _characterService = new CharacterService(characterDataAccess, accountDataAccess, validationService,
            new FlAdminConfig(), new NullLogger<CharacterService>());
    }
    
    
    
    
}