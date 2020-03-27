namespace gputempmon
{
    interface IArduino
    {
        void Dispose();
        string ReadLogLine();
        void UpdateDutyCycle(string fanId, int dutyCycle);
    }
}