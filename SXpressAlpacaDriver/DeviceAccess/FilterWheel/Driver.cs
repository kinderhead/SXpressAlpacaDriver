using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;

namespace SXpressAlpacaDriver.DeviceAccess.FilterWheel
{
    public class Driver(IFilterWheelController controller) : IFilterWheelV3
    {
        private readonly IFilterWheelController Controller = controller;

        public event Action<Driver>? OnChange;

        public bool Connecting { get; private set; }

        public List<StateValue> DeviceState => [new(DateTime.Now), new("Position", Position)];

        // Could cache the value but I don't care enough to do that
        public int[] FocusOffsets => [.. Enumerable.Repeat(0, FilterCount)];
        public string[] Names => [.. Enumerable.Range(1, FilterCount).Select(i => $"Filter {i}")];

        public short FilterCount { get; private set; } = 0;

        private short _position { get; set { field = value; OnChange?.Invoke(this); } } = -1;
        public short Position
        {
            get => Failed && ServerSettings.FilterWheel.ThrowIfFail ? throw new ASCOM.DriverException("Previous move failed to reach the target.") : _position;
            set
            {
                if (value >= FilterCount || value < 0) throw new ASCOM.DriverException($"Filter position {value} is invalid.");
                if (ServerSettings.FilterWheel.DebugCrash) throw new ASCOM.DriverException("Debug crash.");

                _position = -1;
                Move(value);
            }
        }

        public bool Failed { get; private set; } = false;

        public bool Connected { get; set { field = value; OnChange?.Invoke(this); } } = false;

        public string Description => "Alpaca Starlight Xpress Universal Filter Wheel Driver";
        public string DriverInfo => $"Alpaca Starlight Xpress Universal Filter Wheel Driver v{DriverVersion}";
        public string DriverVersion => "1.0";
        public short InterfaceVersion => 3;
        public string Name => "Starlight Xpress Universal Filter Wheel Driver";

        public IList<string> SupportedActions => [];

        public async void Connect()
        {
            if (Connected)
            {
                Program.Logger.LogWarning("Already connected to filter wheel.");
                return;
            }

            Program.Logger.LogInformation("Connecting to filter wheel.");

            Connecting = true;

            Controller.Connect();

            var query = await Controller.Query();
            FilterCount = query.Size;
            _position = query.Position;

            Connecting = false;
            Connected = true;
        }

        public async void Disconnect()
        {
            if (!Connected)
            {
                Program.Logger.LogWarning("Already disconnected from filter wheel.");
                return;
            }

            Program.Logger.LogInformation("Disconnecting from filter wheel.");

            Connecting = true;
            Controller.Disconnect();
            Connecting = false;

            Connected = false;
        }

        public async void Query()
        {
            var query = await Controller.Query();
            _position = query.Position;
        }

        public async void Move(short position)
        {
            try
            {
                Program.Logger.LogInformation($"Moving filter wheel to position {position}.");

                await Controller.Move(position);

                for (int i = 0; i < ServerSettings.FilterWheel.Timeout; i++)
                {
                    var query = await Controller.Query();

                    if (query.Position == position)
                    {
                        _position = query.Position;
                        Program.Logger.LogInformation("Movement successful.");
                        Failed = false;
                        return;
                    }

                    await Task.Delay(1000);
                }

                Program.Logger.LogWarning("Movement failed. Falling back to given position.");
                _position = position;
                Failed = true;
            }
            catch (Exception e)
            {
                Program.Logger.LogError(e.Message);
            }
        }

        // Looks like I can ignore these

        public void Dispose()
        {
            
        }

        public string Action(string actionName, string actionParameters)
        {
            throw new NotImplementedException();
        }

        public void CommandBlind(string command, bool raw = false)
        {
            throw new NotImplementedException();
        }

        public bool CommandBool(string command, bool raw = false)
        {
            throw new NotImplementedException();
        }

        public string CommandString(string command, bool raw = false)
        {
            throw new NotImplementedException();
        }
    }
}
