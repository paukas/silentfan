using OpenHardwareMonitor.Hardware;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;

namespace gputempmon
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Start!");
            var computer = new Computer()
            {
                //CPUEnabled = true,
                //FanControllerEnabled = true,
                //MainboardEnabled = true,
                GPUEnabled = true
            };
            var serial = new SerialPort("COM3");

            serial.Open();
            computer.Open();

            UpdateVisitor updateVisitor = new UpdateVisitor();
            TemperaturePrintingVisitor temperaturePrintingVisitor = new TemperaturePrintingVisitor();
            SerialTemperatureVisitor serialTemperatureVisitor = new SerialTemperatureVisitor(serial);
            try
            {
                while (true)
                {
                    var stopwatch = Stopwatch.StartNew();
                    Console.Clear();
                    computer.Accept(updateVisitor);
                    computer.Accept(temperaturePrintingVisitor);
                    computer.Accept(serialTemperatureVisitor);
                    stopwatch.Stop();
                    Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}");

                    Task.Delay(350).Wait();
                }
            }
            finally
            {
                computer.Close();
                serial.Close();
            }

            Console.WriteLine("The end...");
        }
    }

    internal class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitParameter(IParameter parameter)
        {
        }

        public void VisitSensor(ISensor sensor)
        {
        }
    }

    internal abstract class SensorVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Traverse(this);
        }

        public void VisitParameter(IParameter parameter)
        {
            parameter.Traverse(this);
        }

        public abstract void VisitSensor(ISensor sensor);
    }

    internal class SerialTemperatureVisitor : SensorVisitor
    {
        private SerialPort _serial;

        public SerialTemperatureVisitor(SerialPort serial)
        {
            _serial = serial;
        }

        public override void VisitSensor(ISensor sensor)
        {
            if (sensor.SensorType == SensorType.Temperature)
            {
                _serial.WriteLine($"{sensor.Value}");
            }
        }
    }

    internal class TemperaturePrintingVisitor : SensorVisitor
    {
        public override void VisitSensor(ISensor sensor)
        {
            if (sensor.SensorType == SensorType.Temperature)
            {
                Console.WriteLine($"{sensor.Name}: {sensor.Value}");
            }
        }
    }
}
