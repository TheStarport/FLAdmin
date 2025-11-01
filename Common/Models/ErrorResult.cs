using FlAdmin.Common.Models.Error;

namespace FlAdmin.Common.Models;

public struct ErrorResult()
{
    public ErrorResult(FLAdminErrorCode err, string errMsg) : this()
    {
        this.Errors.Add(new FlAdminError(err, errMsg));
    }
    
    
    public List<FlAdminError> Errors { get; set; } = [];

    public Exception? Exception { get; set; } = null;

    public bool HasErrorCode(FLAdminErrorCode error)
    {
        return this.Errors.Any(err => err.ErrorCode == error);
    }
    
    
}

public struct FlAdminError(FLAdminErrorCode ErrorCode, string Message)
{
    public FLAdminErrorCode ErrorCode { get; set; }
    public string Message { get; set; }
}