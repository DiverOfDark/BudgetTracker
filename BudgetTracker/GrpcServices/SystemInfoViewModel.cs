using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    // TODO use shared state
    public class SystemInfoViewModel : GrpcViewModelBase<SystemInfo>
    {
        private readonly UpdateService _updateService;
        private Timer _timer;
        private readonly SystemInfo _model;

        public SystemInfoViewModel(UpdateService updateService, ObjectRepository objectRepository, ILogger<SystemInfoViewModel> logger) : base(objectRepository, logger)
        {
            _updateService = updateService;
            _model = new SystemInfo
            {
                IsProduction = Startup.IsProduction,
                CurrentVersion = Startup.CommmitHash,
                HasNewerVersion = _updateService.HasNewerVersion,
                LatestVersion = _updateService.LatestVersion,
                LaunchTime = Startup.LaunchTime.ToLocalTime().ToString("G"),
                Stats = ObjectRepository.Stats()
            };
        }

        protected override Task Init()
        {
            _updateService.PropertyChanged += UpdateServiceChanged;
            _timer = new Timer(100) {AutoReset = false};
            _timer.Elapsed += SendStatsUpdate;
            
            Anchors.Add(() =>
            {
                _updateService.PropertyChanged -= UpdateServiceChanged;
                _timer.Elapsed -= SendStatsUpdate;
            });
            Anchors.Add(_timer.Dispose);
            SendUpdate(_model);

            return Task.CompletedTask;
        }

        private void SendStatsUpdate(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            _model.Stats = ObjectRepository.Stats();
            SendUpdate(_model);
        }

        private void UpdateServiceChanged(object sender, PropertyChangedEventArgs e)
        {
            _model.HasNewerVersion = _updateService.HasNewerVersion;
            _model.LatestVersion = _updateService.LatestVersion;
            SendUpdate(_model);
        }

        protected override void OnModelRepositoryChanged(ModelChangedEventArgs obj)
        {
            if ((obj.ChangeType == ChangeType.Add || obj.ChangeType == ChangeType.Remove) && obj.Entity.GetType().Assembly == typeof(SystemInfoViewModel).Assembly)
            {
                _timer.Enabled = true;
            }
        }
    }
}