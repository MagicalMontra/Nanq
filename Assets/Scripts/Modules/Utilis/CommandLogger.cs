using VitalRouter;
using UnityEngine;
using System.Threading.Tasks;

namespace SETHD.Utilis
{
    public class CommandLogger : ICommandInterceptor
    {
        public async ValueTask InvokeAsync<T>(T command, PublishContext context, PublishContinuation<T> next) where T : ICommand
        {
            Debug.Log($"Start command {typeof(T)}");	
            await next(command, context);		
            Debug.Log($"End command {typeof(T)}");
        }
    }
}
