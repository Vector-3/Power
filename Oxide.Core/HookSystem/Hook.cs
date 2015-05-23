using System;
using System.Collections.Generic;

using Oxide.Core.Logging;

namespace Oxide.Core.HookSystem
{  
    public enum ConflictResolutionType { Ignore, Warn }

    /// <summary>
    /// Represents a hook
    /// </summary>
    public sealed class Hook<TReturn, TArg>
        where TReturn : struct
        where TArg : struct
    {
        // The delegate
        public delegate TReturn? CallbackDelegate(TArg arg);

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
        public TReturn? Call(TArg arg)
        {
            if (inHook)
            {
                Interface.GetMod().RootLogger.Write(LogType.Warning, "Hook recursion warning - a subscriber of hook {0} caused the hook to refire!", name);
                return null;
            }
            TReturn value = default(TReturn);
            bool found = false;
            inHook = true;
            int cnt = subscribers.Count;
            for (int i = 0; i < cnt; i++)
            {
                CallbackDelegate d = subscribers[i];
                TReturn? val = d(arg);
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
                        val = val.Value;
                    }
                }
            }
            inHook = false;
            if (found)
                return value;
            else
                return null;
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
