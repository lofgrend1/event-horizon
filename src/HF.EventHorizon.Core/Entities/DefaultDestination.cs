using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.Core.Entities
{
    public class DefaultDestination : IDestination
    {
        public bool Publish(string address, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PublishAsync(string address, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}