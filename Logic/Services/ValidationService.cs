using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic.Services;

public class ValidationService : IValidationService
{
    LibreLancer.Data.FreelancerData _freelancerData;
    private readonly  ILogger _logger;

    public ValidationService(IFreelancerDataProvider freelancerDataProvider,ILogger<IValidationService> logger)
    {
        _logger = logger;
        
        if (freelancerDataProvider.GetFreelancerData() is null)
        {
            logger.LogCritical("No freelancer data available for validation service.");
            throw new Exception("No freelancer data available for validation service.");
        }
        
        _freelancerData = freelancerDataProvider.GetFreelancerData()!;
    }
    
    public bool ValidateCharacter(Character character)
    {
        if (ShipExists(character.ShipHash) is false)
        {
            return false;
        }

        if (SystemExists(character.System) is false)
        {
            return false;
        }
        
        

        return true;
    }

    private bool ShipExists(long shipHash)
    {
        return _freelancerData.Ships.Ships.Any(x => LibreLancer.FLHash.CreateID(x.Nickname) == shipHash);
    }

    private bool SystemExists(long systemHash)
    {
        return _freelancerData.Universe.Systems.Any(x => LibreLancer.FLHash.CreateID(x.Nickname) == systemHash);
    }
    
    
}