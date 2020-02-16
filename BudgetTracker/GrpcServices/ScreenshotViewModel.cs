using System.Timers;
using Google.Protobuf;

namespace BudgetTracker.GrpcServices
{
    public class ScreenshotViewModel : GrpcViewModelBase<Screenshot>
    {
        private readonly Chrome _chrome;

        public ScreenshotViewModel(Chrome chrome)
        {
            Model = new Screenshot();
            _chrome = chrome;
            var timer = new Timer(1000);
            timer.Elapsed += SendScrenshot;
            timer.Start();
            Anchors.Add(timer.Dispose);
            Anchors.Add(() => timer.Elapsed -= SendScrenshot);
        }

        private void SendScrenshot(object sender, ElapsedEventArgs e)
        {
            if (_chrome.HasDriver)
            {
                var screenShot = _chrome.Driver.GetScreenshot().AsByteArray;
                Model.Contents = ByteString.CopyFrom(screenShot);
                SendUpdate();
            }
        }
    }
}