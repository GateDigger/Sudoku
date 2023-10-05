using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Sudoku.Core
{
    public class Slice<S, T>
    {
        readonly S sliceData;
        public Slice(S sliceData)
        {
            status = ExecutionStatus.Pending;
            this.sliceData = sliceData;
        }

        ExecutionStatus status;
        public ExecutionStatus Status
        {
            get
            {
                return status;
            }
        }

        T result;
        public T Result
        {
            get
            {
                if (!(status == ExecutionStatus.Finished || status != ExecutionStatus.Interrupted))
                    throw new InvalidOperationException();

                return result;
            }
        }

        public S Start()
        {
            if (status != ExecutionStatus.Pending)
                throw new InvalidOperationException();

            status = ExecutionStatus.InProgress;
            return sliceData;
        }

        public void Finish(T result)
        {
            if (status != ExecutionStatus.InProgress)
                throw new InvalidOperationException();

            status = ExecutionStatus.Finished;
            this.result = result;
        }

        public void Interrupt(T result)
        {
            if (status != ExecutionStatus.InProgress)
                throw new InvalidOperationException();

            status = ExecutionStatus.Interrupted;
            this.result = result;
        }
    }

    public class SliceExecutionController<S, T> : IDisposable
    {
        readonly ConcurrentBag<Slice<S, T>> queuedSlices,
            takenSlices;
        public SliceExecutionController()
        {
            status = ExecutionStatus.Initialization;
            mode = ExecutionMode.OSPT;
            interruptFlag = false;
            sliceCount = 0;
            queuedSlices = new ConcurrentBag<Slice<S, T>>();
            takenSlices = new ConcurrentBag<Slice<S, T>>();
        }

        volatile ExecutionStatus status;
        public ExecutionStatus Status
        {
            get
            {
                return status;
            }
        }

        ExecutionMode mode;
        public ExecutionMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (status != ExecutionStatus.Initialization)
                    throw new InvalidOperationException();
                mode = value;
            }
        }

        volatile bool interruptFlag;
        public bool InterruptFlag
        {
            get
            {
                return interruptFlag;
            }
        }

        /// <summary>
        /// Switches the interrupt flag
        /// </summary>
        public void Interrupt()
        {
            interruptFlag = true;
        }

        int sliceCount;
        public int SliceCount
        {
            get
            {
                return sliceCount;
            }
        }

        int taskCount;
        public int TaskCount
        {
            get
            {
                return mode == ExecutionMode.OSPT ? sliceCount : taskCount;
            }
            set
            {
                if (status != ExecutionStatus.Initialization)
                    throw new InvalidOperationException();
                mode = ExecutionMode.FNOT;
                taskCount = value;
            }
        }

        /// <summary>
        /// ST#1a, ST#1a/ST#1b/ST#2 follows
        /// </summary>
        /// <param name="sliceData">The data for the to be created slice</param>
        /// <exception cref="InvalidOperationException">If the controller was past initialization</exception>
        public void CreateSlice(S sliceData)
        {
            if (status != ExecutionStatus.Initialization)
                throw new InvalidOperationException();

            sliceCount++;
            queuedSlices.Add(new Slice<S, T>(sliceData));
        }

        /// <summary>
        /// ST#1b, ST#1a/ST#1b/ST#2 follows
        /// </summary>
        /// <param name="slice">The slice to be added to execution bag</param>
        /// <exception cref="InvalidOperationException">If the controller was past initialization</exception>
        public void EnqueueSlice(Slice<S, T> slice)
        {
            if (status != ExecutionStatus.Initialization || slice.Status != ExecutionStatus.Pending)
                throw new InvalidOperationException();

            sliceCount++;
            queuedSlices.Add(slice);
        }

        volatile CountdownEvent countdown;
        /// <summary>
        /// ST#2, MT#1 follows
        /// </summary>
        /// <param name="substitutor">controller => (() => Method(controller, ...))</param>
        public void ProcessAllSlices(Substitutor<S, T> substitutor)
        {
            if (status != ExecutionStatus.Initialization)
                throw new InvalidOperationException();

            int taskCount = TaskCount;
            countdown = new CountdownEvent(taskCount);
            status = ExecutionStatus.InProgress;

            for (int index = taskCount; index > 0; index--)
                Task.Run(substitutor(this));

            countdown.Wait();

            if (status != ExecutionStatus.Interrupted)
                status = ExecutionStatus.Finished;
        }

        /// <summary>
        /// MT#1, MT#2/MT#4 follows
        /// </summary>
        /// <param name="slice">The slice</param>
        /// <returns>Whether a slice was successfully taken from the bag</returns>
        public bool TryTakeSlice(out Slice<S, T> slice)
        {
            if (status != ExecutionStatus.InProgress)
                throw new InvalidOperationException();

            if (queuedSlices.TryTake(out slice))
            {
                takenSlices.Add(slice);
                return true;
            }
            return false;
        }

        /// <summary>
        /// MT#2, MT#3 follows
        /// </summary>
        /// <param name="slice">The slice</param>
        /// <returns>The slice data to process</returns>
        public S StartSlice(Slice<S, T> slice)
        {
            return slice.Start();
        }

        /// <summary>
        /// MT#3, MT#1/MT#4 follows
        /// </summary>
        /// <param name="slice">Slice reference</param>
        /// <param name="result">Calculation result</param>
        public void FinishSlice(Slice<S, T> slice, T result)
        {
            if (interruptFlag)
                slice.Interrupt(result);
            else
                slice.Finish(result);
        }

        /// <summary>
        /// MT#4, ST#3 follows
        /// </summary>
        public void FinishTask()
        {
            countdown.Signal();
        }

        /// <summary>
        /// ST#3
        /// </summary>
        /// <returns>The bag of finished slices with results to process</returns>
        public ConcurrentBag<Slice<S, T>> GetSliceCollection()
        {
            if (status == ExecutionStatus.Initialization)
                return queuedSlices;

            if (!(status == ExecutionStatus.Finished || status == ExecutionStatus.Interrupted))
                throw new InvalidOperationException();
            
            return takenSlices;
        }

        public void Dispose()
        {
            countdown.Dispose();
        }
    }

    public enum ExecutionStatus
    {
        Initialization, Pending, InProgress, Finished, Interrupted
    }

    public enum ExecutionMode
    {
        OSPT, //One slice per task
        FNOT //Fixed number of tasks
    }

    public delegate Action Substitutor<S, T>(SliceExecutionController<S, T> controller);
}