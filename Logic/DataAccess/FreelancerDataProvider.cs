using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using LibreLancer.Data;
using LibreLancer.Data.IO;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic.DataAccess;

public class FreelancerDataProvider : IFreelancerDataProvider
{
    private readonly FlAdminConfig _config;

    private readonly ILogger<FreelancerDataProvider> _logger;

    public FreelancerDataProvider(ILogger<FreelancerDataProvider> logger, FlAdminConfig config)
    {
        _logger = logger;
        _config = config;
        Reload();
    }

    private FileSystem? Vfs { get; set; }
    private FreelancerIni? Ini { get; set; }
    private FreelancerData? Data { get; set; }

    public bool Loaded()
    {
        return Vfs != null;
    }


    public void Reload()
    {
        if (string.IsNullOrWhiteSpace(_config.Server.FreelancerPath)) return;

        if (!Directory.Exists(_config.Server.FreelancerPath)) return;

        try
        {
            Vfs = FileSystem.FromPath(_config.Server.FreelancerPath);
            Ini = new FreelancerIni(Vfs);
            Data = new FreelancerData(Ini, Vfs);
            Data.LoadData();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Encountered an unexpected error when attempting to fetch Freelancer data.");
            Vfs = null;
            Ini = null;
            Data = null;
        }
    }

    public FreelancerData? GetFreelancerData()
    {
        return Data;
    }
}