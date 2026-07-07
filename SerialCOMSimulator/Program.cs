using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;

namespace SerialCOMSimulator
{
    internal class Program
    {
        static SerialPort? _serial;
        static readonly Random _rng = new Random();

        static string Port = "COM1";
        static int Baud = 9600;
        static double IntervalSeconds = 0.5;
        static double Jitter = 2.0;
        static double MaxWeight = 25000;
        static bool RawMode = true;

        static void Main(string[] args)
        {

            _serial = new SerialPort(Port, Baud)
            {
                NewLine = "\r\n",
                WriteTimeout = 2000
            };

            try
            {
                _serial.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open port {Port}: {ex.Message}");
                return;
            }

            Console.WriteLine($"Streaming simulated scale data on {Port} @ {Baud} baud. Ctrl+C to stop.\n");

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nStopped by user.");
                _serial.Close();
                Environment.Exit(0);
            };

            if (RawMode)
            {
                SimulateRawWrapStream();
                _serial.Close();
                return;
            }
        }

        static void SimulateRawWrapStream()
        {
            const int GapWidth = 7;
            string gap = new string(' ', GapWidth);

            Console.WriteLine("Press Ctrl+C to stop.\n");

            int interval = (int)(IntervalSeconds * 1000);

            // Start somewhere in range and random-walk the value up/down each tick,
            // occasionally resetting toward 0 to simulate vehicle on/off the scale.
            double current = 0;
            double direction = 1;
            double max = MaxWeight > 0 ? MaxWeight : 25000;

            while (true)
            {
                // Random walk step
                double step = RandRange(0, Jitter > 0 ? Jitter * 5 : 100);
                current += direction * step;

                if (current >= max)
                {
                    current = max;
                    direction = -1;
                }
                else if (current <= 0)
                {
                    current = 0;
                    direction = 1;
                }
                else if (_rng.NextDouble() < 0.02) // occasional random reversal
                {
                    direction *= -1;
                }

                long displayValue = (long)Math.Round(current);
                string valueStr = displayValue <= 0 ? "00" : displayValue.ToString(CultureInfo.InvariantCulture);
                string frame = valueStr + gap;

                try
                {
                    _serial!.Write(frame);
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("WARNING: write timeout - is a reader connected on the other end?");
                }

                Console.Write(frame); // echo locally the same way, no newline
                Thread.Sleep(interval);
            }
        }

        static double RandRange(double min, double max) => min + _rng.NextDouble() * (max - min);
    }
}
