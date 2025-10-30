using FlAdmin.Common.Models.Error;

namespace FlAdmin.Common.Models;

public struct ErrorResult()
{
    public List<FLAdminError> Errors { get; set; } = new();

    public Exception? Exception { get; set; } = null;
}

public struct FLAdminError(FLAdminErrorCode ErrorCode, string Message)
{
    
    
    public FLAdminErrorCode ErrorCode { get; set; }
    public string Message { get; set; }
}