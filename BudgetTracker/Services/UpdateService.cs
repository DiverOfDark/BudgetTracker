using System;
using System.Threading.Tasks;
using Octokit;

namespace BudgetTracker.Services
{
    public class UpdateService
    {
        private readonly GitHubClient _gitHubClient;

        private DateTime _lastFetchTime = DateTime.MinValue;
        private string _masterSha;

        public UpdateService()
        {
            _gitHubClient = new GitHubClient(new ProductHeaderValue("DiverOfDark_BudgetTracker", Startup.CommmitHash));
        }

        public async Task<String> GetLatestVersion()
        {
            await RefreshVersions();

            return _masterSha;
        }

        private async Task RefreshVersions()
        {
            if (_masterSha == null || _lastFetchTime.AddHours(1) < DateTime.Now)
            {
                var readOnlyList = await _gitHubClient.Repository.Branch.Get("DiverOfDark", "BudgetTracker", "master");
                _masterSha = readOnlyList.Commit.Sha;
                _lastFetchTime = DateTime.Now;
            }
        }

        public async Task<bool> HasNewerVersion() => Startup.CommmitHash != await GetLatestVersion();
    }
}