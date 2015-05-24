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
        public static readonly Hook<int, NestedHookArgs> NestedHook;

        public static readonly Hook<NoArg> BasicHook;

        static ILTest()
        {
            NestedHook = new Hook<int, NestedHookArgs>(ConflictResolutionType.Warn, "NestedHook");
            BasicHook = new Hook<NoArg>("BasicHook");
        }

        private int SomeMethodWhichDoesThings(int a, int b)
        {
            int c = a + b;
            return c;
        }

        private int SomeMethodWhichDoesThingsHooked(int a, int b)
        {
            int c = a + b;
            HookReturnValue<int> hookReturnValue = NestedHook.Call(new NestedHookArgs(a, b));
            if (hookReturnValue.HasValue) return hookReturnValue.Value;
            return c;
        }

        private void SomeMethodWhichIsPrettyBasic()
        {
            Console.WriteLine("Basic shit");
            BasicHook.Call(new NoArg());
        }

        

    }
}
