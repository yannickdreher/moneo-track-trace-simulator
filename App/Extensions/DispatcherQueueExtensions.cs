using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace App.Extensions
{
    public static class DispatcherQueueExtensions
    {
        public static Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcher, Func<T> function)
        {
            var tcs = new TaskCompletionSource<T>();

            dispatcher.TryEnqueue(() =>
            {
                try
                {
                    var result = function();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}