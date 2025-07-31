using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace WindowsAppService
{
    public sealed class AppService : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private AppServiceConnection _connection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Store deferral to keep the background task alive
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnCanceled;

            if (taskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                _connection = details.AppServiceConnection;
                _connection.RequestReceived += OnRequestReceived;
                _connection.ServiceClosed += OnServiceClosed;
            }
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            Debug.WriteLine("AppService instantiated");
            var messageDeferral = args.GetDeferral();
            ValueSet response;

            try
            {
                var message = args.Request.Message;
                var result = new ValueSet() { { "Status", "OK" } };
                response = result;
            }
            catch (Exception ex)
            {
                response = new ValueSet
                {
                    { "Status", "error" },
                    { "Message", ex.Message }
                };
            }

            await args.Request.SendResponseAsync(response);
            messageDeferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _deferral?.Complete();
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _deferral?.Complete();
        }
    }
}
