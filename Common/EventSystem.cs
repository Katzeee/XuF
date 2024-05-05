using System;
using System.Collections.Generic;

namespace Xuf
{
    namespace Common
    {
        public class EventSystem<T> where T : struct, Enum
        {
            public Dictionary<T, Action> events;

        }

    }

}
