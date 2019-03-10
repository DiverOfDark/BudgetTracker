using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace BudgetTracker.Services
{
    public class UpdateService
    {
        private GitHubClient gitHubClient;

        private IEnumerable<Release> versions = null;
        private DateTime lastFetchTime = DateTime.MinValue;
        
        public UpdateService()
        {
            gitHubClient = new GitHubClient(new ProductHeaderValue("DiverOfDark_BudgetTracker", Startup.CommmitHash));
        }

        public async Task<String> GetLatestVersion()
        {
            await RefreshVersions();

            var latestVersion = versions.OrderByDescending(v => v.CreatedAt).First();
            return latestVersion.TagName;
        }

        public async Task<String> GetLatestVersionUrl()
        {
            await RefreshVersions();

            var latestVersion = versions.OrderByDescending(v => v.CreatedAt).First();
            return latestVersion.HtmlUrl;
        }

        public async Task<String> GetCurrentVersion()
        {
            await RefreshVersions();

            var matchingVersion = versions.FirstOrDefault(v => v.TagName == Startup.CommmitHash);
            return matchingVersion?.TagName ?? $"{Startup.CommmitName} / {Startup.CommmitHash}";
        }

        private async Task RefreshVersions()
        {
            if (versions == null || lastFetchTime.AddHours(1) < DateTime.Now)
            {
                var readOnlyList = await gitHubClient.Repository.Release.GetAll("DiverOfDark", "BudgetTracker");
                versions = readOnlyList.ToList();
                lastFetchTime = DateTime.Now;
            }
        }

        public async Task<bool> HasNewerVersion() => await GetCurrentVersion() != await GetLatestVersion();
    }
}