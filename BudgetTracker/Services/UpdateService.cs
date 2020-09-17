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
        private readonly Timer _timer;

        public UpdateService()
        {
            _gitHubClient = new GitHubClient(new ProductHeaderValue("DiverOfDark_BudgetTracker", Startup.CommmitHash));
            _timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            
            Anchors.Add(_timer.Dispose);
        }

        private void TimerCallback(object state)
        {
            _timer.Change(-1, -1);
            Init().GetAwaiter().GetResult();
            _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromHours(1));
        }

        private async Task Init()
        {
            try
            {
                var readOnlyList = await _gitHubClient.Repository.Branch.Get("DiverOfDark", "BudgetTracker", "master");
                var newVersion = readOnlyList.Commit.Sha;

                if (newVersion != LatestVersion)
                {
                    LatestVersion = newVersion;
                    OnPropertyChanged(nameof(LatestVersion));
                    OnPropertyChanged(nameof(HasNewerVersion));
                }
            } catch(Exception ex) {}
        }

        public String LatestVersion { get; private set; } = "Unknown";

        public bool HasNewerVersion => LatestVersion != null && Startup.CommmitHash != LatestVersion;
    }
}