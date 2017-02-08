using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using BandSpy.Helpers;
using BandSpy.Models;
using BandSpy.ViewModels;
using Microsoft.Band.Portable;
using Plugin.DeviceInfo;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace BandSpy
{
	public partial class BandPage : ContentPage
	{
		BandPageViewModel bandPageViewModel;
		public BandClient BandClient;
		public BandDeviceInfo BandInfo;
		public bool IsMonitoring = true;
		// Instanciate the Remote Monitoring helper class
		RemoteMonitoringDevice Device = new RemoteMonitoringDevice();


		//NewMessageService messageService;
		public BandPage()
		{
			InitializeComponent();
			SetupDeviceForIoTHub();
			bandPageViewModel = new BandPageViewModel();
		}
		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await ConnectBand();
			// we must await
			if (BandInfo != null)
			{
				StartMonitoring();
			}

		}

		public void SetupDeviceForIoTHub()
		{
			// Retreive Settings stored on the device
			Device.DeviceId = "YOURDEVICENAME";
			Device.HostName = "YOURHOST.azure-devices.net";
			Device.DeviceKey = "ENTER Your Key HERE";

			// Add the telemetry data
			Device.AddTelemetry(new TelemetryFormat { Name = "Location", DisplayName = "Location", Type = "string" }, "");
			Device.AddTelemetry(new TelemetryFormat { Name = "HeartRate", DisplayName = "Heart Rate", Type = "double" }, (double)0);


			//Header record
			// This information is sent to the IOT Hub when the device connects
			// This is the place you can specify the metadata for your device. The below fields are not mandatory.
			Device.Model.DeviceProperties.CreatedTime = DateTime.UtcNow.ToString();
			Device.Model.DeviceProperties.UpdatedTime = DateTime.UtcNow.ToString();
			Device.Model.DeviceProperties.FirmwareVersion = "1.0";
			Device.Model.DeviceProperties.InstalledRAM = "Unknown";
			Device.Model.DeviceProperties.Manufacturer = "Unknown";
			Device.Model.DeviceProperties.ModelNumber = CrossDeviceInfo.Current.Model;
			Device.Model.DeviceProperties.Platform = CrossDeviceInfo.Current.Platform.ToString() + " " + CrossDeviceInfo.Current.Version;
			Device.Model.DeviceProperties.Processor = "Unknown";
			Device.Model.DeviceProperties.SerialNumber = CrossDeviceInfo.Current.Id;

			// Attach Callbacks to UI resources
			ButtonConnect.Clicked += ButtonConnect_Clicked;
			ButtonSend.Clicked += ButtonSend_Clicked;

		}
		private bool Beat()
		{
			if (IsMonitoring)
			{
				HRVRate.Text = bandPageViewModel.HeartRate;

				Device.UpdateTelemetryData("HeartRate", bandPageViewModel.HeartRateDbl);

				Device.UpdateTelemetryData("Location", bandPageViewModel.Location);
			}
			return true;
		}
		private void ButtonSend_Clicked(object sender, EventArgs e)
		{
			if (Device.SendTelemetryData)
			{

				Device.SendTelemetryData = false;
				ButtonSend.Text = "Press to send telemetry data";
			}
			else
			{
				Device.SendTelemetryData = true;
				ButtonSend.Text = "Sending telemetry data";
			}
		}

		private void ButtonConnect_Clicked(object sender, EventArgs e)
		{
			if (Device.IsConnected)
			{
				Device.SendTelemetryData = false;
				if (Device.Disconnect())
				{
					ButtonSend.IsEnabled = false;
					ButtonConnect.Text = "Press to connect";
				}
			}
			else
			{
				if (Device.Connect())
				{
					ButtonSend.IsEnabled = true;
					ButtonConnect.Text = "Connected";

				}
			}

		}










		protected override bool OnBackButtonPressed()
		{
			return base.OnBackButtonPressed();
			DisconnectBand();
		}
		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			DisconnectBand();
		}
		private void StartMonitoring()
		{
			//HeartBeating
			Xamarin.Forms.Device.StartTimer(new TimeSpan(0, 0, 1), Beat);

		}
		private async Task ConnectBand()
		{
			await bandPageViewModel.Prepare();
			BandInfo = await bandPageViewModel.GetAttachedBand();
			if (BandInfo == null)
			{
				heartIcon.IconFont = "f05e";
				IsMonitoring = false;

				await DisplayAlert("Band", "A Microsoft Band is not paired with this phone", "OK");
			}
			else
			{
				await Pulse();
				await bandPageViewModel.GetAttachedBand();
				BindingContext = bandPageViewModel;
				bandPageViewModel.SenseHeartRate();
				IsMonitoring = true;
				HRVRate.Opacity = 1;
				heartIcon.IconFont = "f004";
			}

		}
		private async Task DisconnectBand()
		{
			IsMonitoring = false;
			bandPageViewModel.ConnectedBandName = "Paused";
			await bandPageViewModel.DisconnectBand();
			BandClient = null;
			BandInfo = null;
		}

		private async Task<bool> Pulse()
		{
			if (BandInfo != null)
			{
				await heartIcon.FadeTo(0, 300);
				await heartIcon.FadeTo(1, 300);
			}
			return true;
		}


		async void StopClicked(object sender, EventArgs e)
		{
			await DisconnectBand();
		}





	}
}
