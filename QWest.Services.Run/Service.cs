﻿using QWest.Admin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Utilities.Utilities;

namespace QWest.Services.Run {
    public abstract class Service {
        public static IEnumerable<Service> GenerateServices(Action<Service, string> log) {
            yield return new ApiService(log);
            yield return new WebSerivce(log);
            yield return new EmailSerivce(log);
            yield return new AdminService(log);
        }

        private Action<Service, string> _log;

        public Service(Action<Service, string> log) {
            _log = log;
        }

        protected void Log(string str) {
            _log(this, str);
        }

        public abstract string Name { get; }

        public abstract Task Run();
    }

    public abstract class NodeService : Service {
        public NodeService(Action<Service, string> log) : base(log) {

        }
        public override Task Run() {
            string location = SolutionLocation + "\\" + Name;
            return Task.Factory.StartNew(() => {
                DynamicShell("npm i", Log, location).WaitForExit();
                DynamicShell("npm start", Log, location).WaitForExit();
            });
        }
    }

    public class ApiService : Service {
        public ApiService(Action<Service, string> log) : base(log) {

        }
        public override string Name {
            get {
                return "QWest.Api";
            }
        }

        public override Task Run() {
            return Api.Program.Run();
        }
    }

    public class WebSerivce : NodeService {
        public WebSerivce(Action<Service, string> log) : base(log) {

        }
        public override string Name {
            get {
                return "QWest.Web";
            }
        }
    }

    public class EmailSerivce : NodeService {
        public EmailSerivce(Action<Service, string> log) : base(log) {

        }
        public override string Name {
            get {
                return "QWest.Email";
            }
        }
    }

    public class AdminService : Service {
        public AdminService(Action<Service, string> log) : base(log) {

        }
        public override string Name {
            get {
                return "QWest.Admin";
            }
        }

        public override Task Run() {
            return Task.FromResult(true).ContinueWith(x => {
                new Application().Run(new MainWindow());
            }, new StaTaskScheduler(1));
        }

        //class is stoken from here https://github.com/dotnet/samples/blob/9ae31f531a5f82928134f2ba6f67144e92603e01/csharp/parallel/ParallelExtensionsExtras/TaskSchedulers/StaTaskScheduler.cs
        public sealed class StaTaskScheduler : TaskScheduler, IDisposable {
            /// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
            private BlockingCollection<Task> _tasks;
            /// <summary>The STA threads used by the scheduler.</summary>
            private readonly List<Thread> _threads;

            /// <summary>Initializes a new instance of the StaTaskScheduler class with the specified concurrency level.</summary>
            /// <param name="numberOfThreads">The number of threads that should be created and used by this scheduler.</param>
            public StaTaskScheduler(int numberOfThreads) {
                // Validate arguments
                if (numberOfThreads < 1) throw new ArgumentOutOfRangeException(nameof(numberOfThreads));

                // Initialize the tasks collection
                _tasks = new BlockingCollection<Task>();

                // Create the threads to be used by this scheduler
                _threads = Enumerable.Range(0, numberOfThreads).Select(i =>
                {
                    var thread = new Thread(() =>
                    {
                        // Continually get the next task and try to execute it.
                        // This will continue until the scheduler is disposed and no more tasks remain.
                        foreach (var t in _tasks.GetConsumingEnumerable()) {
                            TryExecuteTask(t);
                        }
                    }) {
                        IsBackground = true
                    };
                    thread.SetApartmentState(ApartmentState.STA);
                    return thread;
                }).ToList();

                // Start all of the threads
                _threads.ForEach(t => t.Start());
            }

            /// <summary>Queues a Task to be executed by this scheduler.</summary>
            /// <param name="task">The task to be executed.</param>
            protected override void QueueTask(Task task) =>
                // Push it into the blocking collection of tasks
                _tasks.Add(task);

            /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
            /// <returns>An enumerable of all tasks currently scheduled.</returns>
            protected override IEnumerable<Task> GetScheduledTasks() =>
                // Serialize the contents of the blocking collection of tasks for the debugger
                _tasks.ToArray();

            /// <summary>Determines whether a Task may be inlined.</summary>
            /// <param name="task">The task to be executed.</param>
            /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
            /// <returns>true if the task was successfully inlined; otherwise, false.</returns>
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) =>
                // Try to inline if the current thread is STA
                Thread.CurrentThread.GetApartmentState() == ApartmentState.STA &&
                    TryExecuteTask(task);

            /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
            public override int MaximumConcurrencyLevel => _threads.Count;

            /// <summary>
            /// Cleans up the scheduler by indicating that no more tasks will be queued.
            /// This method blocks until all threads successfully shutdown.
            /// </summary>
            public void Dispose() {
                if (_tasks != null) {
                    // Indicate that no new tasks will be coming in
                    _tasks.CompleteAdding();

                    // Wait for all threads to finish processing tasks
                    foreach (var thread in _threads) thread.Join();

                    // Cleanup
                    _tasks.Dispose();
                    _tasks = null;
                }
            }
        }
    }
}
