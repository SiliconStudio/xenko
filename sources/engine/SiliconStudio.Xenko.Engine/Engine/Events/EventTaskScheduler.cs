using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Engine.Events
{
    /// <summary>
    /// Simple passthru scheduler to avoid the default dataflow TaskScheduler.Default usage
    /// This also makes sure we fire events at proper required order/timing
    /// </summary>
    internal class EventTaskScheduler : TaskScheduler
    {
        public static readonly EventTaskScheduler Scheduler = new EventTaskScheduler();

        protected override void QueueTask(Task task)
        {
            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }
    }
}