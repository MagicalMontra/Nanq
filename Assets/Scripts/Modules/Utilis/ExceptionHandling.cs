using System;
using UnityEngine;
using VitalRouter;
using System.Threading.Tasks;

namespace SETHD.Utilis
{
    public class ExceptionHandling : ICommandInterceptor
    {
        public async ValueTask InvokeAsync<T>(  
            T command,  
            PublishContext context,  
            PublishContinuation<T> next)  
            where T : ICommand  
        {  
            try
            {
                await next(command, context);
            }
            catch (Exception ex)
            {
                Debug.Log($"Exception! {ex.Message}");			
            }
        }
    }
}