using SuperTools.Core.Exceptions;
using System.Threading;
using System;

namespace SuperTools.Core
{
    internal static class Ensure
    {
        internal static void PositiveOrZero(string parameterName, int number)
        {
            if (number < 0)
            {
                throw new SuperToolException($"{parameterName} can't be < 0.");
            }
        }

        internal static void NotNull(Timer timer)
        {
            if (timer == null)
            {
                throw new SuperToolException("Wrong utilization of timer, trying to access disposed object.");
            }
        }

        internal static void NotNull(Action callback)
        {
            if (callback == null)
            {
                throw new SuperToolException("Invalid callback informed.");
            }
        }
    }
}
