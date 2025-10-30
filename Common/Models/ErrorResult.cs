using FlAdmin.Common.Models.Error;

namespace FlAdmin.Common.Models;

public struct ErrorResult()
{
    public List<Tuple<FLAdminError, string>> Errors { get; set; } = new();

    public Exception? Exception { get; set; } = null;
}


