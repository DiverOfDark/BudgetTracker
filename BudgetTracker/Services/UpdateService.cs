using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Octokit;

namespace BudgetTracker.Services
{
    public class UpdateService : ViewModelBase
    {
        private readonly GitHubClient _gitHubClient;

        public UpdateService()
        {
            _gitHubClient = new GitHubClient(new ProductHeaderValue("DiverOfDark_BudgetTracker", Startup.CommmitHash));
            var timer = new Timer(Init, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            
            Anchors.Add(timer.Dispose);
        }

        private async void Init(object state)
        {
            var readOnlyList = await _gitHubClient.Repository.Branch.Get("DiverOfDark", "BudgetTracker", "master");
            var newVersion = readOnlyList.Commit.Sha;

            if (newVersion != LatestVersion)
            {
                LatestVersion = newVersion;
                OnPropertyChanged(nameof(LatestVersion));
                OnPropertyChanged(nameof(HasNewerVersion));
            }
        }

        public String LatestVersion { get; private set; } = "Unknown";

        public bool HasNewerVersion => LatestVersion != null && Startup.CommmitHash != LatestVersion;
    }
}