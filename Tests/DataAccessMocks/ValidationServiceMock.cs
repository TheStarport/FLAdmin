using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;

namespace FlAdmin.Tests.DataAccessMocks;

public class ValidationServiceMock : IValidationService
{
    public bool ValidateCharacter(Character character)
    {
        return true;
    }
}