using System;
using System.Collections.Generic;

using Oxide.Core.Logging;

namespace Oxide.Core.HookSystem
{  
    public enum ConflictResolutionType { Ignore, Warn }

    public struct NoArg { }

    public struct HookReturnValue<T>
    {
        public readonly T Value;
        public readonly bool HasValue;
        public HookReturnValue(T value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }
    }

    /// <summary>
    /// Represents a hook with a return value and an argument
    /// </summary>
    public sealed class Hook<TReturn, TArg>
        where TReturn : struct
        where TArg : struct
    {
        // The delegate
        public delegate HookReturnValue<TReturn> CallbackDelegate(TArg arg);

        // All subscribers to this hook
        private List<CallbackDelegate> subscribers;

        // How to resolve conflicts
        private ConflictResolutionType conflictResolution;

        // The name of this hook
        private string name;

        // Are we currently in execution?
        private bool inHook;

        /// <summary>
        /// Initialises a new instance of the Hook class
        /// </summary>
        public Hook(ConflictResolutionType conflictResolution, string name)
        {
            // Initialise
            subscribers = new List<CallbackDelegate>();
            this.conflictResolution = conflictResolution;
            this.name = name;
        }

        /// <summary>
        /// Calls this hook
        /// </summary>
        /// <returns></returns>
        public HookReturnValue<TReturn> Call(TArg arg)
        {
            if (inHook)
            {
                Interface.GetMod().RootLogger.Write(LogType.Warning, "Hook recursion warning - a subscriber of hook {0} caused the hook to refire!", name);
                return new HookReturnValue<TReturn>(default(TReturn), false);
            }
            TReturn value = default(TReturn);
            bool found = false;
            inHook = true;
            int cnt = subscribers.Count;
            for (int i = 0; i < cnt; i++)
            {
                CallbackDelegate d = subscribers[i];
                HookReturnValue<TReturn> val = d(arg);
                if (val.HasValue)
                {
                    if (found)
                    {
                        switch (conflictResolution)
                        {
                            case ConflictResolutionType.Warn:
                                Interface.GetMod().RootLogger.Write(LogType.Warning, "Conflict warning - multiple values returned for hook {0}!", name);
                                break;
                        }
                    }
                    else
                    {
                        found = true;
                        value = val.Value;
                    }
                }
            }
            inHook = false;
            return new HookReturnValue<TReturn>(value, found);
        }

        /// <summary>
        /// Subscribes to this hook
        /// </summary>
        /// <param name="callback"></param>
        public void Subscribe(CallbackDelegate callback)
        {
            if (inHook)
            {
                Interface.GetMod().RootLogger.Write(LogType.Warning, "Hook subscribe warning - a subscriber of hook {0} tried to add another subscriber!", name);
                return;
            }
            subscribers.Add(callback);
        }

        /// <summary>
        /// Unsubscribes from this hook
        /// </summary>
        /// <param name="callback"></param>
        public void Unsubscribe(CallbackDelegate callback)
        {
            if (inHook)
            {
                Interface.GetMod().RootLogger.Write(LogType.Warning, "Hook unsubscribe warning - a subscriber of hook {0} tried to remove a subscriber!", name);
                return;
            }
            subscribers.Remove(callback);
        }

    }

    /// <summary>
    /// Represents a hook with no return value and an argument
    /// </summary>
    /// <typeparam name="TArg"></typeparam>
    public sealed class Hook<TArg>
        where TArg : struct
    {
        // The delegate
        public delegate void CallbackDelegate(TArg arg);

        // All subscribers to this hook
        private List<CallbackDelegate> subscribers;

        // The name of this hook
        private string name;

        // Are we currently in execution?
        private bool inHook;

        /// <summary>
        /// Initialises a new instance of the Hook class
        /// </summary>
        public Hook(string name)
        {
            // Initialise
            subscribers = new List<CallbackDelegate>();
            this.name = name;
        }

        /// <summary>
        /// Calls this hook
        /// </summary>
        /// <returns></returns>
        public void Call(TArg arg)
        {
            if (inHook)
            {
                Interface.GetMod().RootLogger.Write(LogType.Warning, "Hook recursion warning - a subscriber of hook {0} caused the hook to refire!", name);
                return;
            }
            inHook = true;
            int cnt = subscribers.Count;
            for (int i = 0; i < cnt; i++)
            {
                subscribers[i](arg);
            }
            inHook = false;
        }

        /// <summary>
        /// Subscribes to this hook
        /// </summary>
        /// <param name="callback"></param>
        public void Subscribe(CallbackDelegate callback)
        {
            if (inHook)
            {
                Interface.GetMod().RootLogger.Write(LogType.Warning, "Hook subscribe warning - a subscriber of hook {0} tried to add another subscriber!", name);
                return;
            }
            subscribers.Add(callback);
        }

        /// <summary>
        /// Unsubscribes from this hook
        /// </summary>
        /// <param name="callback"></param>
        public void Unsubscribe(CallbackDelegate callback)
        {
            if (inHook)
            {
                Interface.GetMod().RootLogger.Write(LogType.Warning, "Hook unsubscribe warning - a subscriber of hook {0} tried to remove a subscriber!", name);
                return;
            }
            subscribers.Remove(callback);
        }
    }

    
}
