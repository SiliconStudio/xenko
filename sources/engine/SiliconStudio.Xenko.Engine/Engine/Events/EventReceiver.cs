using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SiliconStudio.Xenko.Engine.Events
{
    public class EventReceiver<T>
    {
        private readonly BufferBlock<T> block;

        public EventReceiver(EventKey<T> key, bool buffered = false)
        {
            if (buffered)
            {
                block = new BufferBlock<T>(new DataflowBlockOptions
                {
                    BoundedCapacity = 1
                });
            }
            else
            {
                block = new BufferBlock<T>();
            }

            key.Connect(block);
        }

        public async Task<T> Receive()
        {
            return await block.ReceiveAsync();
        }
    }
}