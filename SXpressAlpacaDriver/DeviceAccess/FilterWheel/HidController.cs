using System.Text;
using ASCOM;
using HidSharp;

namespace SXpressAlpacaDriver.DeviceAccess.FilterWheel
{
    public class HidController : IFilterWheelController
    {
        private HidStream? stream = null;

        public void Connect()
        {
            var dev = DeviceList.Local.GetHidDeviceOrNull(VENDOR_ID, PRODUCT_ID) ?? throw new DriverException("Failed to locate filter wheel. Is it connected?");
            stream = dev.Open();
        }

        public void Disconnect()
        {
            stream?.Dispose();
        }

        public async Task Move(short position)
        {
            await Write([1, (byte)(0x80 | (position + 1)), 0]);
            await Read(new byte[3]); // Ignore result
        }

        public async Task<QueryResult> Query()
        {
            if (stream is null) throw new DriverException("Filter wheel is not connected.");
            await Write([0, 0, 0]);

            var res = new byte[3];
            await Read(res);

            return new() { Position = (short)(res[1] - 1), Size = res[2] };
        }

        private async Task Write(byte[] mem)
        {
            if (stream is null) throw new DriverException("Filter wheel is not connected.");
            await stream.WriteAsync(mem);

            var builder = new StringBuilder("Wrote to filter wheel:\n");

            foreach (var i in mem)
            {
                builder.Append($"0x{i:X2}\n");
            }
            
            builder.Length--;

            Program.Logger?.Log(ASCOM.Common.Interfaces.LogLevel.Information, builder.ToString());
        }

        private async Task Read(byte[] mem)
        {
            if (stream is null) throw new DriverException("Filter wheel is not connected.");
            await stream.ReadExactlyAsync(mem);

            var builder = new StringBuilder("Read from filter wheel:\n");

            foreach (var i in mem)
            {
                builder.Append($"0x{i:X2}\n");
            }

            builder.Length--;

            Program.Logger?.Log(ASCOM.Common.Interfaces.LogLevel.Information, builder.ToString());
        }

        public const int PRODUCT_ID = 0x0920;
        public const int VENDOR_ID = 0x1278;
    }
}
