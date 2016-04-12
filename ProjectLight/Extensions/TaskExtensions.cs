using System.Threading.Tasks;

namespace ProjectLight.Extensions
{
    public static class TaskExtensions
    {
        public static Task Forget(this Task task)
        {
            task.ConfigureAwait(false);
            return task;
        }

        public static Task<T> Forget<T>(this Task<T> task)
        {
            task.ConfigureAwait(false);
            return task;
        }
    }
}
