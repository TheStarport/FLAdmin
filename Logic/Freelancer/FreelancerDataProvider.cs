namespace Logic.Freelancer;

using Common.Configuration;
using Common.Freelancer;
using LibreLancer.Data;
using Microsoft.Extensions.Logging;

public class FreelancerDataProvider : IFreelancerDataProvider
{
	private FileSystem? Vfs { get; set; }
	private FreelancerIni? FreelancerIni { get; set; }
	private FreelancerData? FreelancerData { get; set; }

	private readonly FLAdminConfiguration _configuration;
	private readonly ILogger _logger;

	public FreelancerDataProvider(ILogger<FreelancerDataProvider> logger, FLAdminConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	public bool Loaded() => Vfs is not null;

	public void Reload()
	{
		if (string.IsNullOrWhiteSpace(_configuration.Server.FreelancerPath))
		{
			return;
		}

		if (!Directory.Exists(_configuration.Server.FreelancerPath))
		{
			_logger.LogWarning("Attempted to load Freelancer data but directory could not be found.");
			return;
		}

		try
		{
			_logger.LogDebug("Loading Freelancer data from path: {Path}", _configuration.Server.FreelancerPath);
			Vfs = FileSystem.FromFolder(_configuration.Server.FreelancerPath);
			FreelancerIni = new FreelancerIni(Vfs);
			FreelancerData = new FreelancerData(FreelancerIni, Vfs);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Unable to load Freelancer data from path: {Path}", _configuration.Server.FreelancerPath);
			Vfs = null;
			FreelancerIni = null;
			FreelancerData = null;
		}
	}

	public FreelancerData? GetData() => FreelancerData;
}
