namespace Common.Freelancer;

using LibreLancer.Data;

public interface IFreelancerDataProvider
{
	bool Loaded();
	void Reload();
	FreelancerData? GetData();
}
