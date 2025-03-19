using FlAdmin.Common.Models.Database;

namespace FlAdmin.Common.Services;

public interface IValidationService
{
    public bool ValidateCharacter(Character character);
}