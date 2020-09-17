using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BudgetTracker.Model;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Support.Extensions;

namespace BudgetTracker.GrpcServices
{
    // TODO use shared state
    public class ScreenshotViewModel : GrpcViewModelBase<Screenshot>
    {
        private readonly Chrome _chrome;
        private readonly Screenshot _model;

        public ScreenshotViewModel(Chrome chrome, ObjectRepository objectRepository, ILogger<ScreenshotViewModel> logger) : base(objectRepository, logger)
        {
            _chrome = chrome;
            _model = new Screenshot();
        }

        protected override Task Init()
        {
            var timer = new Timer(1000);
            timer.Elapsed += SendScrenshot;
            timer.Start();
            SendScrenshot(null, null);
            Anchors.Add(timer.Dispose);
            Anchors.Add(() => timer.Elapsed -= SendScrenshot);
            return Task.CompletedTask;
        }

        private void SendScrenshot(object sender, ElapsedEventArgs e)
        {
            if (_chrome.HasDriver)
            {
                var screenShot = _chrome.Driver.TakeScreenshot().AsByteArray;

                if (!_model.Contents.SequenceEqual(screenShot))
                {
                    _model.Contents = ByteString.CopyFrom(screenShot);
                    SendUpdate(_model);
                }
            }
        }
    }
}