using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using OpenQA.Selenium.Support.Extensions;

namespace BudgetTracker.GrpcServices
{
    // TODO use shared state
    public class ScreenshotViewModel : GrpcViewModelBase<Screenshot>
    {
        private readonly Chrome _chrome;
        private readonly Screenshot _model;

        public ScreenshotViewModel(Chrome chrome, IHttpContextAccessor accessor): base(accessor)
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
            return base.Init();
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