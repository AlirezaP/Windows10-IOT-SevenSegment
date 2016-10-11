using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Iot_Led_With_Touch
{
    class SevenSegment
    {
        public enum Numbers
        {
            One=1, Two=2, Three=3, Four=4, Five=5, Six=6, Seven=7, Eight=8, Nine=9, Zero=0
        }
        private static void SetSegment(List<MainPage.TempStr> pins, GpioPinValue val, int num)
        {
            var pin = pins.Where(p => p.ID == num).FirstOrDefault();
            pin.TPin.Write(val);
        }
        public static void TurnOn(List<MainPage.TempStr> pins, Numbers num)
        {
            TurnOffAll(pins);

            var val = GpioPinValue.Low;

            switch (num)
            {

                case Numbers.One:
                    SetSegment(pins, val, 21);
                    SetSegment(pins, val, 13);

                    break;

                case Numbers.Two:
                    SetSegment(pins, val, 26);
                    SetSegment(pins, val, 13);
                    SetSegment(pins, val, 19);
                    SetSegment(pins, val, 16);
                    SetSegment(pins, val, 20);
                    break;


                case Numbers.Three:
                    SetSegment(pins, val, 26);
                    SetSegment(pins, val, 13);
                    SetSegment(pins, val, 19);
                    SetSegment(pins, val, 20);
                    SetSegment(pins, val, 21);
                    break;

                case Numbers.Four:
                    SetSegment(pins, val, 13);
                    SetSegment(pins, val, 19);
                    SetSegment(pins, val, 21);
                    SetSegment(pins, val, 6);
                    break;

                case Numbers.Five:
                    SetSegment(pins, val, 26);
                    SetSegment(pins, val, 20);
                    SetSegment(pins, val, 19);
                    SetSegment(pins, val, 21);
                    SetSegment(pins, val, 6);
                    break;

                case Numbers.Six:
                    SetSegment(pins, val, 6);
                    SetSegment(pins, val, 19);
                    SetSegment(pins, val, 16);
                    SetSegment(pins, val, 20);
                    SetSegment(pins, val, 21);
                    break;

                case Numbers.Seven:
                    SetSegment(pins, val, 26);
                    SetSegment(pins, val, 13);
                    SetSegment(pins, val, 21);
                    break;

                case Numbers.Eight:
                    SetSegment(pins, val, 26);
                    SetSegment(pins, val, 19);
                    SetSegment(pins, val, 13);
                    SetSegment(pins, val, 6);
                    SetSegment(pins, val, 21);
                    SetSegment(pins, val, 20);
                    SetSegment(pins, val, 16);
                    break;

                case Numbers.Nine:
                    SetSegment(pins, val, 26);
                    SetSegment(pins, val, 19);
                    SetSegment(pins, val, 13);
                    SetSegment(pins, val, 6);
                    SetSegment(pins, val, 21);
                    break;

                case Numbers.Zero:
                    SetSegment(pins, val, 26);
                    SetSegment(pins, val, 13);
                    SetSegment(pins, val, 6);
                    SetSegment(pins, val, 21);
                    SetSegment(pins, val, 20);
                    SetSegment(pins, val, 16);
                    break;
            }
        }

        public static void TurnOffAll(List<MainPage.TempStr> pins)
        {
            var val = GpioPinValue.High;
            SetSegment(pins, val, 26);
            SetSegment(pins, val, 19);
            SetSegment(pins, val, 13);
            SetSegment(pins, val, 6);
            SetSegment(pins, val, 21);
            SetSegment(pins, val, 20);
            SetSegment(pins, val, 16);
        }

    }
}
