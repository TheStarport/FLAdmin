using FlAdmin.Common.Models.Error;

namespace FlAdmin.Common.Models;

public class ErrorResult
{
    
    
    private List<Tuple<FLAdminError, string>> Errors { get; set; } = new List<Tuple<FLAdminError, string>>();
}