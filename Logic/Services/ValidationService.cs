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
        return true;
    }
    
    
    
    
    
}