using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.UI.Popups;
using Windows.ApplicationModel;
using Windows.System.Display;
using Windows.ApplicationModel.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Foundation;


namespace vid2_25022021846pm
{
    public sealed partial class MainPage : Page
    {
        private MediaCapture erMediaCapture;
        private ErStates erRecordingState;
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        public string fileName;
        private StorageFile erStorageFile;
        private readonly string erVideoFileName = "VidRecorder.mp4";
        private DispatcherTimer erTimer;
        private TimeSpan erElapsedTime;


        public MainPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += Application_Resuming;
        }

        #region Suspending, Resuming
        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Dispose();
            deferral.Complete();
        }

        private async void Application_Resuming(object sender, object o)
        {
            await InitializeMediaCapture();
        }

        #endregion Suspending, Resuming

        #region Navigation
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeMediaCapture();
            UpdateRecordingState(ErStates.None);
            InitTimer();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Dispose();
        }

        #endregion Navigation       

        #region Init, Failure, Limit

        private async Task InitializeMediaCapture()
        {
            try
            {
                erMediaCapture = new MediaCapture();
                var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.AudioAndVideo
                };
                await erMediaCapture.InitializeAsync(settings);
                erMediaCapture.Failed += MediaCaptureOnFailed;
                erMediaCapture.RecordLimitationExceeded += MediaCaptureOnRecordLimitationExceeded;

            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. One of the causes of failure " +
                    "to preview is that the computer's camera and microphone are disabled. Please " +
                    "enable the computer's camera and microphone when using this app.");
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }


        private async void MediaCaptureOnFailed(MediaCapture sender, MediaCaptureFailedEventArgs erroreventargs)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var warningMessage = new MessageDialog(String.Format("The video capture failed: {0}", erroreventargs.Message), "Capture Failed");
                await warningMessage.ShowAsync();
            });
        }

        private async void MediaCaptureOnRecordLimitationExceeded(MediaCapture sender)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await erMediaCapture.StopRecordAsync();
                var warningMessage = new MessageDialog(String.Format("The video capture has exceeded its maximum length: {0}",
                    "Capture Halted"));
                await warningMessage.ShowAsync();
            });
        }


        #endregion Init, Failure, Limit        

        #region Timer
        public void InitTimer()
        {
            erTimer = new DispatcherTimer();
            erTimer.Interval = new TimeSpan(0, 0, 0, 1);
            erTimer.Tick += ErTimerOnTick;
        }

        private void ErTimerOnTick(object sender, object e)
        {
            erElapsedTime = erElapsedTime.Add(erTimer.Interval);
            Duration.DataContext = erElapsedTime;
        }

        #endregion Timer

        #region Buttons
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "HamburgerButton")
            {
                AlterHamburger();
            }

            else if ((sender as Button).Name == "FaqButton")
            {
                AlterFaq();
            }

            else if ((sender as Button).Name == "PreviewButton")
            {
                AlterPreview();
            }

            else if ((sender as Button).Name == "RecordButton")
            {
                AlterRecord();
            }

            else if ((sender as Button).Name == "PauseButton")
            {
                AlterPause();
            }

            else if ((sender as Button).Name == "ResumeButton")
            {
                AlterResume();
            }

            else if ((sender as Button).Name == "StopButton")
            {
                AlterStop();
                CleanResources();
            }

            else if ((sender as Button).Name == "ResetButton")
            {
                AlterReset();
            }


        }

        #endregion Buttons

        #region Dispose
        private void Dispose()
        {
            if (erMediaCapture != null)
            {
                erMediaCapture.Dispose();
                erMediaCapture = null;
            }

            if (ErCaptureElem.Source != null)
            {
                ErCaptureElem.Source.Dispose();
                ErCaptureElem.Source = null;
            }
        }

        #endregion Dispose 

        #region CleanResources
        private async void CleanResources()
        {
            ErCaptureElem.Source = null;
            await erMediaCapture.StopPreviewAsync();

            if (StopButton.IsEnabled)
            {
                await erMediaCapture.StopRecordAsync();
            }
            erMediaCapture.Dispose();
        }

        #endregion CleanResources

        #region Status
        private async void UpdateRecordingState(ErStates recordingState)
        {
            erRecordingState = recordingState;
            string statusMessage = string.Empty;

            switch (recordingState)
            {
                case ErStates.Stopped:
                    StopButton.IsEnabled = false;
                    PauseButton.IsEnabled = false;
                    ResumeButton.IsEnabled = false;
                    RecordButton.IsEnabled = false;
                    PreviewButton.IsEnabled = false;
                    statusMessage = "Status:   Stop and   Save";
                    break;

                case ErStates.Previewing:
                    PreviewButton.IsEnabled = false;
                    PauseButton.IsEnabled = false;
                    ResumeButton.IsEnabled = false;
                    StopButton.IsEnabled = false;
                    RecordButton.IsEnabled = true;
                    statusMessage = "Status:  Camera  Ready";
                    break;

                case ErStates.Recording:
                    RecordButton.IsEnabled = false;
                    PauseButton.IsEnabled = true;
                    ResumeButton.IsEnabled = false;
                    StopButton.IsEnabled = true;
                    PreviewButton.IsEnabled = false;
                    statusMessage = "Status:  Recording";
                    break;

                case ErStates.Pause:
                    PauseButton.IsEnabled = false;
                    ResumeButton.IsEnabled = true;
                    RecordButton.IsEnabled = false;
                    StopButton.IsEnabled = true;
                    PreviewButton.IsEnabled = false;
                    statusMessage = "Status:  Paused";
                    break;

                case ErStates.Resume:
                    ResumeButton.IsEnabled = false;
                    PauseButton.IsEnabled = true;
                    RecordButton.IsEnabled = false;
                    StopButton.IsEnabled = true;
                    PreviewButton.IsEnabled = false;
                    statusMessage = "Status:  Recording   Resumed";
                    break;

                case ErStates.None:
                    StopButton.IsEnabled = false;
                    PauseButton.IsEnabled = false;
                    ResumeButton.IsEnabled = false;
                    RecordButton.IsEnabled = false;
                    PreviewButton.IsEnabled = true;
                    break;


                default:
                    throw new ArgumentOutOfRangeException("recordingState");
            }

            await UpdateStatus(statusMessage);
        }

        private async Task UpdateStatus(string status)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            Status.Text = status);
        }

        #endregion Status

        #region AlterHamburger
        private void AlterHamburger()
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        #endregion AlterHamburger

        #region AlterFaq
        private void AlterFaq()
        {
            this.Frame.Navigate(typeof(FaqPage));
        }

        #endregion AlterFaq

        #region AlterPreview
        private async void AlterPreview()
        {
            try
            {
                if (erRecordingState != ErStates.Recording &&
                    erRecordingState != ErStates.Previewing)
                {
                    ErCaptureElem.Source = erMediaCapture;
                    await erMediaCapture.StartPreviewAsync();
                    ErCaptureElem.FlowDirection = FlowDirection.RightToLeft;
                    displayRequest.RequestActive();
                    UpdateRecordingState(ErStates.Previewing);
                }
            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. One of the causes of failure " +
                    "to preview is that the computer's camera and microphone are disabled. Please " +
                    "enable the computer's camera and microphone when using this app.");
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }

        #endregion AlterPreview

        #region AlterRecord
        private async void AlterRecord()
        {

            try
            {
                RecordButton.IsEnabled = false;
                String fileName;
                fileName = erVideoFileName;
                erStorageFile = await Windows.Storage.KnownFolders.VideosLibrary.CreateFileAsync(fileName,
                Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                MediaEncodingProfile recordProfile = null;
                recordProfile = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.HD1080p);
                await erMediaCapture.StartRecordToStorageFileAsync(recordProfile, erStorageFile);
                UpdateRecordingState(ErStates.Recording);
                erTimer.Start();
            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. One possible cause of this error " +
                    "is that this app was minimized while recording. Please don't minimize this app while " +
                    "recording. Try refreshing this app.");
                {
                    _ = await dialog.ShowAsync();
                }

            }

        }

        #endregion AlterRecord

        #region AlterPause       
        private async void AlterPause()
        {
            try
            {
                await erMediaCapture.PauseRecordAsync(Windows.Media.Devices.MediaCapturePauseBehavior.ReleaseHardwareResources);
                UpdateRecordingState(ErStates.Pause);
                erTimer.Stop();
            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. One possible cause of this error " +
                    "is that this app was minimized while recording. Please don't minimize this app while " +
                    "recording. Try refreshing this app.");
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }

        #endregion AlterPause

        #region AlterResume
        private async void AlterResume()
        {
            try
            {
                await erMediaCapture.ResumeRecordAsync();
                UpdateRecordingState(ErStates.Resume);
                erTimer.Start();
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. One possible cause of this error " +
                    "is that this app was minimized while recording. Please don't minimize this app while " +
                    "recording. Try refreshing this app.");

                {
                    _ = await dialog.ShowAsync();
                }

            }
        }

        #endregion AlterResume

        #region AlterStop
        private async void AlterStop()
        {
            try
            {
                await erMediaCapture.StopRecordAsync();
                UpdateRecordingState(ErStates.Stopped);
                await UpdateStatus(String.Format("Status:   Stopped & Saved to Videos Folder"));
                erTimer.Stop();
            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. One possible cause of this error " +
                    "is that this app was minimized while recording. Please don't minimize this app while " +
                    "recording. Try refreshing this app.");
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }

        #endregion AlterStop       

        #region AlterReset 
        private async void AlterReset()
        {
            try
            {
                var result = await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");

                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    var msgBox = new MessageDialog("Restart Failed", result.ToString());
                    await msgBox.ShowAsync();
                }
            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong.");
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }
        #endregion AlterReset              


    }
}