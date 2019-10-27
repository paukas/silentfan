using OpenHardwareMonitor.Hardware;

namespace gputempmon
{
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
}
