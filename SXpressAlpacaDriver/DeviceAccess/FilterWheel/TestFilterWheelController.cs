using System;

namespace SXpressAlpacaDriver.DeviceAccess.FilterWheel
{
    public class TestFilterWheelController(short size) : IFilterWheelController
    {
        private readonly short size = size;
        private short position;

        public void Connect()
        {
            
        }

        public void Disconnect()
        {
            
        }

        public async Task Move(short position)
        {
            this.position = -1;

            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                this.position = position;
            });
        }

        public async Task<QueryResult> Query() => new() { Position = position, Size = size };
    }
}
