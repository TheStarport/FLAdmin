namespace FlAdmin.Common.DataAccess;

using LibreLancer.Data;

public interface IFreelancerDataProvider
{
    bool Loaded();
    void Reload();
    FreelancerData? GetFreelancerData();
}