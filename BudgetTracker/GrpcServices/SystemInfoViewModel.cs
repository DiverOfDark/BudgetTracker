using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Http;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    // TODO use shared state
    public class SystemInfoViewModel : GrpcViewModelBase<SystemInfo>
    {
        private readonly UpdateService _updateService;
        private readonly ObjectRepository _objectRepository;
        private Timer _timer;
        private readonly SystemInfo _model;

        public SystemInfoViewModel(UpdateService updateService, ObjectRepository objectRepository, IHttpContextAccessor accessor): base(accessor)
        {
            _updateService = updateService;
            _objectRepository = objectRepository;
            _model = new SystemInfo
            {
                IsProduction = Startup.IsProduction,
                CurrentVersion = Startup.CommmitHash,
                HasNewerVersion = _updateService.HasNewerVersion,
                LatestVersion = _updateService.LatestVersion,
                LaunchTime = Startup.LaunchTime.ToLocalTime().ToString("G"),
                Stats = _objectRepository.Stats()
            };
        }

        protected override Task Init()
        {
            _objectRepository.ModelChanged += ObjectRepositoryChanged;
            _updateService.PropertyChanged += UpdateServiceChanged;
            _timer = new Timer(100) {AutoReset = false};
            _timer.Elapsed += SendStatsUpdate;
            
            Anchors.Add(() =>
            {
                _objectRepository.ModelChanged -= ObjectRepositoryChanged;
                _updateService.PropertyChanged -= UpdateServiceChanged;
                _timer.Elapsed -= SendStatsUpdate;
            });
            Anchors.Add(_timer.Dispose);
            SendUpdate(_model);

            return base.Init();
        }

        private void SendStatsUpdate(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            _model.Stats = _objectRepository.Stats();
            SendUpdate(_model);
        }

        private void UpdateServiceChanged(object sender, PropertyChangedEventArgs e)
        {
            _model.HasNewerVersion = _updateService.HasNewerVersion;
            _model.LatestVersion = _updateService.LatestVersion;
            SendUpdate(_model);
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