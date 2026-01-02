using System;

namespace StarlightDriver.DeviceAccess.FilterWheel
{
    public interface IFilterWheelController
    {
        public void Connect();
        public void Disconnect();
        public Task<QueryResult> Query();
        public Task Move(short position);
    }

    public struct QueryResult
    {
        public required short Position;
        public required short Size;
    }
}
