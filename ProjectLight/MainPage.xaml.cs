﻿using ProjectLight.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ProjectLight
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly DeviceCapturer _capture;
        private BitmapTransformer _transformer;
        private readonly LightSender _lightSender;
        private readonly DisplayRequest _displayRequest = new DisplayRequest();
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        private bool _configLoaded;
        private bool _initialized;

        public MainPage()
        {
            this.InitializeComponent();
            _capture = new DeviceCapturer();
            _lightSender = new LightSender();
            LightConfigText.Text = "LightLayout.xml";
            SetCaptureReader();
        }

        #region Handlers
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var devices = await _capture.GetDevicesAsync();
            var binding = new Binding
            {
                Mode = BindingMode.OneTime,
                Source = devices
            };

            deviceSelector.SetBinding(ComboBox.ItemsSourceProperty, binding);
        }
        private async void deviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedId = deviceSelector.SelectedValue as string;
            await _capture.SetSelectedDevice(selectedId);
            _initialized = true;
            SetCaptureReader();
        }

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            await _capture.StartCapture();
        }
        private async void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (_capture.IsPreviewing)
            {
                await StopPreviewAsync();
                PreviewButton.Content = "Start Preview";
            }
            else
            {
                await _capture.InitializeCapture();
                await StartPreviewAsync();
                PreviewButton.Content = "Stop Preview";
            }
        }
        private async void LoadLightConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var location = LightConfigText.Text;
            var reader = new LightConfigurationReader();
            var model = await reader.CreateFromFileAsync(location);
            _transformer = new BitmapTransformer(model, _lightSender);
            _capture.Transformer = _transformer;
            _configLoaded = true;
            SetCaptureReader();
        }

        private void SetCaptureReader()
        {
            CaptureButton.IsEnabled = _configLoaded && _initialized;
        }

        #endregion

        #region Preview
        private async Task StartPreviewAsync()
        {
            // Prevent the device from sleeping while the preview is running
            _displayRequest.RequestActive();

            // Set the preview source in the UI and mirror it if necessary
            PreviewControl.Source = _capture.MediaCapture;
            PreviewControl.FlowDirection = true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Start the preview
            try
            {
                await _capture.MediaCapture.StartPreviewAsync();
                _capture.IsPreviewing = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when starting the preview: {0}", ex.ToString());
            }

            // Initialize the preview to the current orientation
            if (_capture.IsPreviewing)
            {
                int rotationDegrees = 0;
                var props = _capture.MediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
                props.Properties.Add(RotationKey, rotationDegrees);
                await _capture.MediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
            }
            PreviewButton.Content = "Stop Preview";
        }
        private async Task StopPreviewAsync()
        {
            await _capture.MediaCapture.StopPreviewAsync();
            _capture.IsPreviewing = false;
        }
        #endregion


    }
}
