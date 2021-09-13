using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Extensions;
using Extensions.Threading.Generic;
using Extensions.Threading;

namespace LightConquer_Project
{
    public static class ThreadPoll
    {
        #region Funcs
        public static void Execute(Action action, uint timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            GenericThreadPool.Subscribe(new Extensions.Threading.LazyDelegate(action, timeOut, priority));
        }
        public static void Execute<T>(Action<T> action, T param, uint timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            GenericThreadPool.Subscribe<T>(new Extensions.Threading.Generic.LazyDelegate<T>(action, timeOut, priority), param);
        }
        public static IDisposable Subscribe(Action action, uint period = 1, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe(new Extensions.Threading.TimerRule(action, period, priority));
        }
        public static IDisposable Subscribe<T>(Action<T> action, T param, uint timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe<T>(new Extensions.Threading.Generic.TimerRule<T>(action, timeOut, priority), param);
        }
        public static IDisposable Subscribe<T>(Extensions.Threading.Generic.TimerRule<T> rule, T param, Extensions.Threading.StaticPool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(Extensions.Threading.Generic.TimerRule<T> rule, T param)
        {
            return GenericThreadPool.Subscribe<T>(rule, param);
        }
        #endregion


        public static TimerRule<ServerSockets.SecuritySocket> ConnectionReceive, ConnectionSend, ConnectionReview;
        public static StaticPool GenericThreadPool;
        public static StaticPool ReceivePool, SendPool;

        public static void Create()
        {
            GenericThreadPool = new StaticPool(16).Run();
            ReceivePool = new StaticPool(64).Run();
            SendPool = new StaticPool(32).Run();
            ConnectionReceive = new TimerRule<ServerSockets.SecuritySocket>(connectionReceive, 1);
            ConnectionSend = new TimerRule<ServerSockets.SecuritySocket>(connectionSend, 1);
            ConnectionReview = new TimerRule<ServerSockets.SecuritySocket>(_ConnectionReview, 1000);
        }
        public static void connectionReceive(ServerSockets.SecuritySocket wrapper)
        {
            if (wrapper.ReceiveBuffer())
            {
                wrapper.HandlerBuffer();
            }
        }
        public static void connectionSend(ServerSockets.SecuritySocket wrapper)
        {
            //   ServerSockets.SecuritySocket.TrySend(wrapper);
        }
        public static void _ConnectionReview(ServerSockets.SecuritySocket wrapper)
        {
            wrapper.CheckUp();
        }

    }
}
