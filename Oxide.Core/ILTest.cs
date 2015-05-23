using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Oxide.Core.HookSystem;

namespace Oxide.Core
{
    class ILTest
    {
        public struct NestedHookArgs
        {
            public readonly int a, b;
            public NestedHookArgs(int a, int b)
            {
                this.a = a;
                this.b = b;
            }
        }
        public readonly Hook<int, NestedHookArgs> NestedHook;

        public ILTest()
        {
            NestedHook = new Hook<int, NestedHookArgs>(ConflictResolutionType.Warn, "NestedHook");
        }

        private int SomeMethodWhichDoesThings(int a, int b)
        {
            int c = a + b;
            return c;
        }

        private int SomeMethodWhichDoesThingsHooked(int a, int b)
        {
            int c = a + b;
            int? hookReturnValue = NestedHook.Call(new NestedHookArgs(a, b));
            if (hookReturnValue.HasValue) return hookReturnValue.Value;
            return c;
        }

        

    }
}
