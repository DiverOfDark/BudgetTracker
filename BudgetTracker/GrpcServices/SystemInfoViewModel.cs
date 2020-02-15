using System.ComponentModel;
using System.Timers;
using BudgetTracker.Model;
using BudgetTracker.Services;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class SystemInfoViewModel : GrpcViewModelBase<SystemInfo>
    {
        private readonly UpdateService _updateService;
        private readonly ObjectRepository _objectRepository;
        private readonly Timer _timer;

        public SystemInfoViewModel(UpdateService updateService, ObjectRepository objectRepository)
        {
            _updateService = updateService;
            _objectRepository = objectRepository;
            Model = new SystemInfo
            {
                IsProduction = Startup.IsProduction,
                CurrentVersion = Startup.CommmitHash,
                HasNewerVersion = _updateService.HasNewerVersion,
                LatestVersion = _updateService.LatestVersion,
                LaunchTime = Startup.LaunchTime.ToLocalTime().ToString("G"),
                Stats = _objectRepository.Stats()
            };

            _objectRepository.ModelChanged += ObjectRepositoryChanged;
            _updateService.PropertyChanged += UpdateServiceChanged;
            _timer = new Timer(0.5) {AutoReset = false};
            _timer.Elapsed += SendStatsUpdate;
            
            Anchors.Add(() =>
            {
                _objectRepository.ModelChanged -= ObjectRepositoryChanged;
                _updateService.PropertyChanged -= UpdateServiceChanged;
                _timer.Elapsed -= SendStatsUpdate;
            });
            Anchors.Add(_timer.Dispose);
        }

        private void SendStatsUpdate(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            Model.Stats = _objectRepository.Stats();
            SendUpdate();
        }

        private void UpdateServiceChanged(object sender, PropertyChangedEventArgs e)
        {
            Model.HasNewerVersion = _updateService.HasNewerVersion;
            Model.LatestVersion = _updateService.LatestVersion;
            SendUpdate();
        }

        private void ObjectRepositoryChanged(ModelChangedEventArgs obj)
        {
            if ((obj.ChangeType == ChangeType.Add || obj.ChangeType == ChangeType.Remove) && obj.Entity.GetType().Assembly == typeof(SystemInfoViewModel).Assembly)
            {
                _timer.Enabled = true;
            }
        }
    }
}