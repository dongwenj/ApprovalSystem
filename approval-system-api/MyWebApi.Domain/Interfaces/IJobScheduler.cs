using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyWebApi.Domain.Interfaces
{
    public interface IJobScheduler
    {
        //馬上執行
        string Enqueue(Expression<Action> methodCall);

        //馬上執行
        string Enqueue<T>(Expression<Action<T>> methodCall);

        //延遲執行
        string Schedule(Expression<Action> methodCall, TimeSpan delay);

        //定期執行
        void AddRecurring(string jobId, Expression<Action> methodCall, string cronExpression);
    }
}
