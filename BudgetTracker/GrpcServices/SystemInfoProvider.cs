using System;
using System.ComponentModel;
using BudgetTracker.Model;
using BudgetTracker.Services;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class SystemInfoProvider : GrpcModelProvider<SystemInfo>
    {
        private readonly UpdateService _updateService;
        private readonly ObjectRepository _objectRepository;

        public SystemInfoProvider(UpdateService updateService, ObjectRepository objectRepository)
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
            
            Anchors.Add(new Action(() =>
            {
                _objectRepository.ModelChanged -= ObjectRepositoryChanged;
                _updateService.PropertyChanged -= UpdateServiceChanged;
            }));
        }

        private void UpdateServiceChanged(object sender, PropertyChangedEventArgs e)
        {
            Model.HasNewerVersion = _updateService.HasNewerVersion;
            Model.LatestVersion = _updateService.LatestVersion;
            SendUpdate();
        }

        private void ObjectRepositoryChanged(ModelChangedEventArgs obj)
        {
            if (obj.Entity.GetType().Assembly == GetType().Assembly && (obj.ChangeType == ChangeType.Add || obj.ChangeType == ChangeType.Remove))
            {
                Model.Stats = _objectRepository.Stats();
                SendUpdate();
            }
        }
    }
}