using BandSpy.Models;
using BandSpy.Services;
using Microsoft.Azure.Devices.Client;
using Microsoft.Band;
using Microsoft.Band.Portable;
using Microsoft.Band.Portable.Notifications;
using Microsoft.Band.Portable.Sensors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Microsoft.Azure.Devices;
using Plugin.Geolocator.Abstractions;
using Plugin.Geolocator;


namespace BandSpy.ViewModels
{
    public class BandPageViewModel : BaseViewModel
    {
        public BandPageViewModel()
        {
            Bands = new ObservableCollection<BandDeviceInfo>();

            LoadBandsCommand = new Command(async () => await LoadBands());
			iGeolocator = CrossGeolocator.Current;

			iGeolocator.DesiredAccuracy = 100; //100 is new default

		}
		private IGeolocator iGeolocator;
		BandClient bandClient;
        //DispatcherTimer dt = new DispatcherTimer();
        bool timerIsRunning = false;
        private string _connectedBandName;
        private string _heartRate;
		private double _heartRateDbl;
		private string _statusMessage;
		private string _location;
        private bool running = false;

        private int _activityLevel;

        double latestValue;
        public List<double> HeartRateReadings = new List<double>();
        public string ConnectedBandName
        {
            get { return _connectedBandName; }
            set
            {
                if (value != _connectedBandName)
                {
                    _connectedBandName = value;
                    OnPropertyChanged("ConnectedBandName");
                }
            }
        }

        public string Location
		{
			get { return _location; }
			set
			{
				if (value != _location)
				{
					_location = value;
					OnPropertyChanged("Location");
				}
			}
		}

        public string HeartRate
		{
			get { return _heartRate; }
			set
			{
				if (value != _heartRate)
				{
					_heartRate = value;
					OnPropertyChanged("HeartRate");
				}
			}
		}

		public double HeartRateDbl
		{
			get { return _heartRateDbl; }
			set
			{
				if (value != _heartRateDbl)
				{
					_heartRateDbl = value;
					OnPropertyChanged("HeartRateDbl");
				}
			}
		}
		public string StatusMessage
		{
			get { return _statusMessage; }
			set
			{
				if (value != _statusMessage)
				{
					_statusMessage = value;
					OnPropertyChanged("StatusMessage");
				}
			}
		}
		public int ActivityLevel
        {
            get { return _activityLevel; }
            set
            {
                if (value != _activityLevel)
                {
                    _activityLevel = value;
                    OnPropertyChanged("ActivityLevel");
                    // OnPropertyChanged("ActivityColour");
                }
            }
        }

        private bool _bandFound;
        public bool BandFound
        {
            get { return this._bandFound; }
        }

        public ICommand LoadBandsCommand { get; private set; }

        public ObservableCollection<BandDeviceInfo> Bands { get; private set; }
        public BandDeviceInfo Band { get; private set; }
        public override async Task Prepare()
        {
            await base.Prepare();

            // refresh the list
            LoadBandsCommand.Execute(null);
        }
        public async Task DisconnectBand()
        {
            HeartRateReadings = new List<double>();
            await bandClient.DisconnectAsync();
        }
        public async void SenseHeartRate()
        {
            try
            {
                await FindPairedBands();

                // Connect to Microsoft Band.
                bandClient = await BandClientManager.Instance.ConnectAsync(pairedBand);
                    bool consent = true;

                    if (bandClient.SensorManager.HeartRate.UserConsented != UserConsent.Granted)
                        consent = await bandClient.SensorManager.HeartRate.RequestUserConsent();

                    int samplesReceived = 0;
                    latestValue = 0;

                    running = true;

                    Device.StartTimer(new TimeSpan(0, 0, 1), dt_Tick);
                  

                    if (consent)
                    {
                        bandClient.SensorManager.HeartRate.ReadingChanged += async  (s, args) =>
                        {
                            samplesReceived++;
                            latestValue = args.SensorReading.HeartRate;
                            _heartRate = latestValue.ToString();
							_heartRateDbl = latestValue;
							var position = await iGeolocator.GetPositionAsync(timeoutMilliseconds: 10000);
							 _location = $"lat:{position.Latitude} lon:{position.Longitude}";

						};

                        await bandClient.SensorManager.HeartRate.StartReadingsAsync();
                        // Receive Heart Rate data until user clicks 'Stop Tracking' button.
                        await Task.Run(new Action(keepReading));

                    }
            }
            catch 
            {
                return;
            }
        }

        private void keepReading()
        {
            while (running) { }
        }

        bool dt_Tick()
        {
            HeartRate = latestValue.ToString();

            IEnumerable<Message> messages;
            Message msg = new Message();
            

            int activityLevel = (int)(100 * latestValue / 120);
            if (activityLevel > 100) activityLevel = 100;

            ActivityLevel = activityLevel;

            HeartRateReadings.Add(latestValue);

            int count = HeartRateReadings.Count;
            return running;
        }


        public async void StopSensor()
        {
            try
            {
                await FindPairedBands();
                running = false;
                
                
                bandClient = await BandClientManager.Instance.ConnectAsync(pairedBand);
                await bandClient.SensorManager.HeartRate.StopReadingsAsync();
            }
            catch 
            {
                return;
            }
            finally
            {
                ActivityLevel = 0;
                HeartRate = "";
            }
        }

        public async void SendVibrationOnBand()
        {
            try
            {
                await FindPairedBands();
                await GetAttachedBand();
                if (running == false)
                {
                    bandClient = await BandClientManager.Instance.ConnectAsync(pairedBand);
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.NotificationAlarm);
                }
                else
                {
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.NotificationAlarm);
                }

            }
            catch (BandException ex)
            {
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        IEnumerable<BandDeviceInfo> pairedBands;
        BandDeviceInfo pairedBand;
        public async Task<IEnumerable<BandDeviceInfo>> FindPairedBands()
        {
            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                pairedBands = await BandClientManager.Instance.GetPairedBandsAsync();
                if (pairedBands.Count() < 1)
                {
                    return null;
                }
                ConnectedBandName = pairedBands.ToList()[0].Name;
                return pairedBands;

            }
            catch 
            {
                return null;
            }
        }

        public async Task<BandDeviceInfo> GetAttachedBand()
        {
            Bands.Clear();
            IEnumerable<BandDeviceInfo> bands = await BandClientManager.Instance.GetPairedBandsAsync();
            if (bands == null)
                {
                
                return null;
            }
            else
            {

            }
            var bandlist = bands.ToList();
            if (bandlist.Count == 0)
            {
                return null;
            }
            pairedBand = bandlist[0];
            ConnectedBandName = pairedBand.Name;

            return bandlist[0];
        }

        private async Task LoadBands()
        {
            Bands.Clear();
            IEnumerable<BandDeviceInfo> bands = await BandClientManager.Instance.GetPairedBandsAsync();
            var bandlist = bands.ToList();
            foreach (BandDeviceInfo band in bands)
            {
                Bands.Add(band);
                Band = band;
            }
        }
        private async Task RegisterDeviceIOzT()
        {
            RegisterDeviceSvc rds = new RegisterDeviceSvc();
            //var ss = await rds.RegisterBand(ConnectedBandName);
            //DeviceIOTinfo deviceIOT = new DeviceIOTinfo();
            //deviceIOT.DeviceName = ConnectedBandName;
            //deviceIOT.PrimaryKey = ss;
            //App.Database.SaveDeviceIOT(deviceIOT);
        }
     
    }
}
