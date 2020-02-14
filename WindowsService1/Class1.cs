using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace System.Threading.Tasks
{


    /// <summary>
    /// Proper extensions for Task class which makes it easy to use with it (mostly taken from ASP.NET Web Stack source: http://aspnetwebstack.codeplex.com)
    /// </summary>
    internal static class TaskHelperExtensions
    {


        /// <summary>
        /// A version of task.Unwrap that is optimized to prevent unnecessarily capturing the
        /// execution context when the antecedent task is already completed.
        /// </summary>
        internal static Task FastUnwrap(this Task<Task> task)
        {


            Task innerTask = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
            return innerTask ?? task.Unwrap();
        }


        /// <summary>
        /// A version of task.Unwrap that is optimized to prevent unnecessarily capturing the
        /// execution context when the antecedent task is already completed.
        /// </summary>
        internal static Task<TResult> FastUnwrap<TResult>(this Task<Task<TResult>> task)
        {


            Task<TResult> innerTask = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
            return innerTask ?? task.Unwrap();
        }


        /// <summary>
        /// Calls the given continuation, after the given task has completed, if the task successfully ran
        /// to completion (i.e., was not cancelled and did not fault).
        /// </summary>
        internal static Task<TOuterResult> Then<TOuterResult>(this Task task, Func<Task<TOuterResult>> continuation, CancellationToken cancellationToken = default(CancellationToken), bool runSynchronously = false)
        {


            return task.ThenImpl(t => continuation(), cancellationToken, runSynchronously);
        }


        private static Task<TOuterResult> ThenImpl<TTask, TOuterResult>(this TTask task, Func<TTask, Task<TOuterResult>> continuation, CancellationToken cancellationToken, bool runSynchronously) where TTask : Task
        {

            //We want to stay on th same thread if we can.
            //If the task is already completed, we avoid unnecessary continuations
            if (task.IsCompleted)
            {


                //Check if the task is in the Faulted state or not
                //If the condition is met, then return with a faulted task which contains the errors
                if (task.IsFaulted)
                {


                    return TaskHelpers.FromErrors<TOuterResult>(task.Exception.InnerExceptions);
                }


                //Check if the task is in the Canceled state or not. Also check if the cancellation is requested or not.
                //If the condition is met, then return with a canceled task
                if (task.IsCanceled || cancellationToken.IsCancellationRequested)
                {


                    return TaskHelpers.Canceled<TOuterResult>();
                }


                //Check if the task is in the RanToCompletion state which indicates the success
                if (task.Status == TaskStatus.RanToCompletion)
                {


                    try
                    {


                        return continuation(task);
                    }
                    catch (Exception ex)
                    {


                        return TaskHelpers.FromError<TOuterResult>(ex);
                    }
                }
            }


            // Split into a continuation method so that we don't create a closure unnecessarily
            return ThenImplContinuation(task, continuation, cancellationToken, runSynchronously);
        }


        private static Task<TOuterResult> ThenImplContinuation<TOuterResult, TTask>(TTask task, Func<TTask, Task<TOuterResult>> continuation, CancellationToken cancallationToken, bool runSynchronously = false) where TTask : Task
        {


            //Grap the syncContext first
            SynchronizationContext syncContext = SynchronizationContext.Current;


            TaskCompletionSource<Task<TOuterResult>> tcs = new TaskCompletionSource<Task<TOuterResult>>();


            task.ContinueWith(innerTask =>
            {


                //Check if the task is in the Faulted state or not
                //If the condition is met, then try to set exceptions on the TCS
                if (innerTask.IsFaulted)
                {


                    tcs.TrySetException(innerTask.Exception.InnerExceptions);
                }


                //Check if the task is in the Canceled state or not. Also check if the cancellation is requested or not.
                //If the condition is met, then try set the TCS as canceled
                else if (innerTask.Status == TaskStatus.Canceled || cancallationToken.IsCancellationRequested)
                {


                    tcs.TrySetCanceled();
                }


                //If we get here, this means that the innerTask.Status is RanToCompletion
                else
                {


                    //Firstly, check if the syncContext is null or not.
                    //If not null, we want to invoke the continuation at the context thread
                    if (syncContext != null)
                    {


                        syncContext.Post(state =>
                        {


                            try
                            {
                                tcs.TrySetResult(continuation(task));
                            }
                            catch (Exception ex)
                            {


                                tcs.TrySetException(ex);
                            }
                        }, null);
                    }
                    else
                    {


                        tcs.TrySetResult(continuation(task));
                    }
                }


            }, runSynchronously ? TaskContinuationOptions.ExecuteSynchronously : TaskContinuationOptions.None);


            return tcs.Task.FastUnwrap();
        }
    }

    internal static class TaskHelpers
    {

        private static readonly Task<object> _completedTaskReturningNull = FromResult<object>(null);
        private static readonly Task _defaultCompleted = FromResult<AsyncVoid>(default(AsyncVoid));

        internal static Task<object> NullResult()
        {

            return _completedTaskReturningNull;
        }

        /// <summary>
        /// Returns a successful completed task with the given result.  
        /// </summary>
        internal static Task<TResult> FromResult<TResult>(TResult result)
        {

            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        /// <summary>
        /// Returns an error task. The task is Completed, IsCanceled = False, IsFaulted = True
        /// </summary>
        internal static Task FromError(Exception exception)
        {

            return FromError<AsyncVoid>(exception);
        }

        /// <summary>
        /// Returns an error task of the given type. The task is Completed, IsCanceled = False, IsFaulted = True
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        internal static Task<TResult> FromError<TResult>(Exception exception)
        {

            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>
        /// Returns an error task of the given type. The task is Completed, IsCanceled = False, IsFaulted = True
        /// </summary>
        internal static Task FromErrors(IEnumerable<Exception> exceptions)
        {

            return FromErrors<AsyncVoid>(exceptions);
        }

        /// <summary>
        /// Returns an error task of the given type. The task is Completed, IsCanceled = False, IsFaulted = True
        /// </summary>
        internal static Task<TResult> FromErrors<TResult>(IEnumerable<Exception> exceptions)
        {

            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exceptions);
            return tcs.Task;
        }

        /// <summary>
        /// Returns a completed task that has no result. 
        /// </summary>
        internal static Task Completed()
        {

            return _defaultCompleted;
        }

        /// <summary>
        /// Returns a canceled Task. The task is completed, IsCanceled = True, IsFaulted = False.
        /// </summary>
        internal static Task Canceled()
        {

            return Canceled<AsyncVoid>();
        }

        /// <summary>
        /// Returns a canceled Task of the given type. The task is completed, IsCanceled = True, IsFaulted = False.
        /// </summary>
        internal static Task<TResult> Canceled<TResult>()
        {

            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        /// <summary>
        /// Replacement for Task.Factory.StartNew when the code can run synchronously. 
        /// We run the code immediately and avoid the thread switch. 
        /// This is used to help synchronous code implement task interfaces.
        /// </summary>
        /// <param name="action">action to run synchronouslyt</param>
        /// <param name="token">cancellation token. This is only checked before we run the task, and if cancelled, we immediately return a cancelled task.</param>
        /// <returns>a task who result is the result from Func()</returns>
        /// <remarks>
        /// Avoid calling Task.Factory.StartNew.         
        /// This avoids gotchas with StartNew:
        /// - ensures cancellation token is checked (StartNew doesn't check cancellation tokens).
        /// - Keeps on the same thread. 
        /// - Avoids switching synchronization contexts.
        /// Also take in a lambda so that we can wrap in a try catch and honor task failure semantics.        
        /// </remarks>
        internal static Task RunSynchronously(Action action, CancellationToken token = default(CancellationToken))
        {

            if (token.IsCancellationRequested)
            {

                return Canceled();
            }

            try
            {

                action();
                return Completed();
            }
            catch (Exception e)
            {

                return FromError(e);
            }

        }

        /// <summary>
        /// Replacement for Task.Factory.StartNew when the code can run synchronously. 
        /// We run the code immediately and avoid the thread switch. 
        /// This is used to help synchronous code implement task interfaces.
        /// </summary>
        /// <typeparam name="TResult">type of result that task will return.</typeparam>
        /// <param name="func">function to run synchronously and produce result</param>
        /// <param name="cancellationToken">cancellation token. This is only checked before we run the task, and if cancelled, we immediately return a cancelled task.</param>
        /// <returns>a task who result is the result from Func()</returns>
        /// <remarks>
        /// Avoid calling Task.Factory.StartNew.         
        /// This avoids gotchas with StartNew:
        /// - ensures cancellation token is checked (StartNew doesn't check cancellation tokens).
        /// - Keeps on the same thread. 
        /// - Avoids switching synchronization contexts.
        /// Also take in a lambda so that we can wrap in a try catch and honor task failure semantics.        
        /// </remarks>
        internal static Task<TResult> RunSynchronously<TResult>(Func<TResult> func, CancellationToken token = default(CancellationToken))
        {

            if (token.IsCancellationRequested)
            {

                return Canceled<TResult>();
            }

            try
            {

                return FromResult(func());
            }
            catch (Exception e)
            {

                return FromError<TResult>(e);
            }
        }

        /// <summary>
        /// Overload of RunSynchronously that avoids a call to Unwrap(). 
        /// This overload is useful when func() starts doing some synchronous work and then hits IO and 
        /// needs to create a task to finish the work. 
        /// </summary>
        /// <typeparam name="TResult">type of result that Task will return</typeparam>
        /// <param name="func">function that returns a task</param>
        /// <param name="cancellationToken">cancellation token. This is only checked before we run the task, and if cancelled, we immediately return a cancelled task.</param>
        /// <returns>a task, created by running func().</returns>
        internal static Task<TResult> RunSynchronously<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken = default(CancellationToken))
        {

            if (cancellationToken.IsCancellationRequested)
            {

                return Canceled<TResult>();
            }

            try
            {

                return func();
            }
            catch (Exception e)
            {

                return FromError<TResult>(e);
            }
        }

        /// <summary>
        /// Used as the T in a "conversion" of a Task into a Task{T}
        /// </summary>
        private struct AsyncVoid { }
    }
}
