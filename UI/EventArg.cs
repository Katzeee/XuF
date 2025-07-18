using System;
using UnityEngine;

namespace Xuf.Common
{
    /// <summary>
    /// Abstract base class for all event arguments.
    /// </summary>
    [Serializable]
    public abstract class EventArgBase
    {
        // Base class for event arguments. No members are required here.
    }
}

namespace Xuf.UI
{



    /// <summary>
    /// Integer event argument.
    /// </summary>
    [Serializable]
    public class IntEventArg : EventArgBase
    {
        public int value;
    }

    /// <summary>
    /// Float event argument.
    /// </summary>
    [Serializable]
    public class FloatEventArg : EventArgBase
    {
        public float value;
    }

    /// <summary>
    /// String event argument.
    /// </summary>
    [Serializable]
    public class StringEventArg : EventArgBase
    {
        public string value;
    }

    /// <summary>
    /// Boolean event argument.
    /// </summary>
    [Serializable]
    public class BoolEventArg : EventArgBase
    {
        public bool value;
    }

    /// <summary>
    /// GameObject event argument.
    /// </summary>
    [Serializable]
    public class GameObjectEventArg : EventArgBase
    {
        public GameObject value;
    }
}
