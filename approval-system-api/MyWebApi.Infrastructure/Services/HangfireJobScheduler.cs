    using Hangfire;
using MyWebApi.Domain.Interfaces;
using System.Linq.Expressions;

namespace MyWebApi.Infrastructure.Services
{
    public class HangfireJobScheduler : IJobScheduler
    {
        public string Enqueue(Expression<Action> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }

        public string Enqueue<T>(Expression<Action<T>> methodCall)
        {
            return BackgroundJob.Enqueue<T>(methodCall);
        }

        public string Schedule(Expression<Action> methodCall, TimeSpan delay)
        {
            return BackgroundJob.Schedule(methodCall, delay);
        }

        public void AddRecurring(string jobId, Expression<Action> methodCall, string cronExpression)
        {
            RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
        }
    }
}
