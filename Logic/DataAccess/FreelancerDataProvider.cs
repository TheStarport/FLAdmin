using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;

namespace FlAdmin.Logic.DataAccess;

using LibreLancer.Data;
using LibreLancer.Data.IO;


public class FreelancerDataProvider(FlAdminConfig config) : IFreelancerDataProvider
{
    private FileSystem? Vfs { get; set; }
    private FreelancerIni? Ini { get; set; }
    private FreelancerData? Data { get; set; }

    public bool Loaded() => Vfs != null;

    public void Reload()
    {
        if (string.IsNullOrWhiteSpace(config.Server.FreelancerPath))
        {
            return;
        }
        if (!Directory.Exists(config.Server.FreelancerPath))
        {
            return;
        }

        try
        {
            Vfs = FileSystem.FromPath(config.Server.FreelancerPath);
            Ini = new FreelancerIni(Vfs);
            Data = new FreelancerData(Ini, Vfs);
        }
        catch (Exception ex)
        {
            //TODO: Logging
            Vfs = null;
            Ini = null; 
            Data = null;
        }
    }

    public FreelancerData? GetFreelancerData() => Data;
    
}