using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TouchPanels.Devices;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Iot_Led_With_Touch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            InitialGPIO();
        }

        const string CalibrationFilename = "TSC2046";
        private Tsc2046 tsc2046;
        private TouchPanels.TouchProcessor processor;
        private Point lastPosition = new Point(double.NaN, double.NaN);

        IScrollProvider currentScrollItem;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Init();
            // Status.Text = "Init Success";
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (processor != null)
            {
                //Unhooking from all the touch events, will automatically shut down the processor.
                //Remember to do this, or you view could be staying in memory.
                processor.PointerDown -= Processor_PointerDown;
                processor.PointerMoved -= Processor_PointerMoved;
                processor.PointerUp -= Processor_PointerUp;
            }
            base.OnNavigatingFrom(e);
        }

        private async void Init()
        {
            tsc2046 = await TouchPanels.Devices.Tsc2046.GetDefaultAsync();
            try
            {
                await tsc2046.LoadCalibrationAsync(CalibrationFilename);
            }
            catch (System.IO.FileNotFoundException)
            {
                await CalibrateTouch(); //Initiate calibration if we don't have a calibration on file
            }
            catch (System.UnauthorizedAccessException)
            {
                //No access to documents folder
                await new Windows.UI.Popups.MessageDialog("Make sure the application manifest specifies access to the documents folder and declares the file type association for the calibration file.", "Configuration Error").ShowAsync();
                throw;
            }
            //Load up the touch processor and listen for touch events
            processor = new TouchPanels.TouchProcessor(tsc2046);
            processor.PointerDown += Processor_PointerDown;
            processor.PointerMoved += Processor_PointerMoved;
            processor.PointerUp += Processor_PointerUp;
        }

        private void Processor_PointerDown(object sender, TouchPanels.PointerEventArgs e)
        {
            //WriteStatus(e, "Down");
            currentScrollItem = FindElementsToInvoke(e.Position);
            lastPosition = e.Position;
        }
        private void Processor_PointerMoved(object sender, TouchPanels.PointerEventArgs e)
        {
            //  WriteStatus(e, "Moved");
            if (currentScrollItem != null)
            {
                double dx = e.Position.X - lastPosition.X;
                double dy = e.Position.Y - lastPosition.Y;
                if (!currentScrollItem.HorizontallyScrollable) dx = 0;
                if (!currentScrollItem.VerticallyScrollable) dy = 0;

                Windows.UI.Xaml.Automation.ScrollAmount h = Windows.UI.Xaml.Automation.ScrollAmount.NoAmount;
                Windows.UI.Xaml.Automation.ScrollAmount v = Windows.UI.Xaml.Automation.ScrollAmount.NoAmount;
                if (dx < 0) h = Windows.UI.Xaml.Automation.ScrollAmount.SmallIncrement;
                else if (dx > 0) h = Windows.UI.Xaml.Automation.ScrollAmount.SmallDecrement;
                if (dy < 0) v = Windows.UI.Xaml.Automation.ScrollAmount.SmallIncrement;
                else if (dy > 0) v = Windows.UI.Xaml.Automation.ScrollAmount.SmallDecrement;
                currentScrollItem.Scroll(h, v);
            }
            lastPosition = e.Position;
        }
        private void Processor_PointerUp(object sender, TouchPanels.PointerEventArgs e)
        {
            // WriteStatus(e, "Up");
            currentScrollItem = null;
        }

        private IScrollProvider FindElementsToInvoke(Point screenPosition)
        {
            if (_isCalibrating) return null;

            var elements = VisualTreeHelper.FindElementsInHostCoordinates(new Windows.Foundation.Point(screenPosition.X, screenPosition.Y), this, false);
            //Search for buttons in the visual tree that we can invoke
            //If we can find an element button that implements the 'Invoke' automation pattern (usually buttons), we'll invoke it
            foreach (var e in elements.OfType<FrameworkElement>())
            {
                var element = e;
                AutomationPeer peer = null;
                object pattern = null;
                while (true)
                {
                    peer = FrameworkElementAutomationPeer.FromElement(element);
                    if (peer != null)
                    {
                        pattern = peer.GetPattern(PatternInterface.Invoke);
                        if (pattern != null)
                        {
                            break;
                        }
                        pattern = peer.GetPattern(PatternInterface.Scroll);
                        if (pattern != null)
                        {
                            break;
                        }
                    }
                    var parent = VisualTreeHelper.GetParent(element);
                    if (parent is FrameworkElement)
                        element = parent as FrameworkElement;
                    else
                        break;
                }
                if (pattern != null)
                {
                    var p = pattern as Windows.UI.Xaml.Automation.Provider.IInvokeProvider;
                    p?.Invoke();
                    return pattern as IScrollProvider;
                }
            }
            return null;
        }

        private bool _isCalibrating = false; //flag used to ignore the touch processor while calibrating
        private async Task CalibrateTouch()
        {
            _isCalibrating = true;
            var calibration = await TouchPanels.UI.LcdCalibrationView.CalibrateScreenAsync(tsc2046);
            _isCalibrating = false;
            tsc2046.SetCalibration(calibration.A, calibration.B, calibration.C, calibration.D, calibration.E, calibration.F);
            try
            {
                await tsc2046.SaveCalibrationAsync(CalibrationFilename);
            }
            catch (Exception ex)
            {
                //  Status.Text = ex.Message;
            }
        }

        ////......................................................................
        private int count = 0;
        private DispatcherTimer timer;
        private GpioController gpio = null;
        private List<TempStr> list = new List<TempStr>();

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "1";
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "2";
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "3";
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "4";
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "5";
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "6";
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "7";
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "8";
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "9";
        }

        private void button0_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "0";
        }

        private void buttonC_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "";
        }

        private void InitialGPIO()
        {
            try
            {
                int[] pins = new int[] { 26, 19, 13, 6, 21, 20, 16 };
                gpio = GpioController.GetDefault();

                foreach (var i in pins)
                {
                    var pin = gpio.OpenPin(i);
                    pin.SetDriveMode(GpioPinDriveMode.Output);
                    pin.Write(GpioPinValue.High);
                    list.Add(new TempStr { ID = i, TPin = pin });
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {

            if (list.Count <= 0)
            {
                InitialGPIO();
            }

            switch (txtNumber.Text)
            {
                case "1":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.One);
                    break;

                case "2":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Two);
                    break;

                case "3":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Three);
                    break;

                case "4":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Four);
                    break;

                case "5":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Five);
                    break;

                case "6":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Six);
                    break;

                case "7":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Seven);
                    break;

                case "8":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Eight);
                    break;

                case "9":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Nine);
                    break;

                case "0":
                    SevenSegment.TurnOn(list, SevenSegment.Numbers.Zero);
                    break;
            }
        }


        private void buttonCounter_Click(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(1000);
                timer.Tick += Timer_Tick;
                timer.Start();
                buttonCounter.Content = "Stop Counter";
                return;
            }

            if (!timer.IsEnabled)
            {
                timer.Start();
                buttonCounter.Content = "Stop Counter";
                return;
            }

            if (timer.IsEnabled)
            {
                timer.Stop();
                buttonCounter.Content = "Start Counter";
                return;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            try
            {
                SevenSegment.TurnOn(list, (SevenSegment.Numbers)count);
                count++;

                if (count > 9)
                {
                    count = 0;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public class TempStr
        {
            public int ID { get; set; }
            public GpioPin TPin { get; set; }
        }
    }
}
