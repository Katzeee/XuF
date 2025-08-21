using System;
using UnityEngine;

namespace Xuf.Core
{
    /// <summary>
    /// Abstract base class for all event arguments.
    /// </summary>
    [Serializable]
    public abstract class CEventArgBase { }

    /// <summary>
    /// Empty event argument for no-argument events.
    /// </summary>
    [Serializable]
    public sealed class CEmptyEventArg : CEventArgBase { }

    /// <summary>
    /// Integer event argument.
    /// </summary>
    [Serializable]
    public class CIntEventArg : CEventArgBase
    {
        public int value;
    }

    /// <summary>
    /// Float event argument.
    /// </summary>
    [Serializable]
    public class CFloatEventArg : CEventArgBase
    {
        public float value;
    }

    /// <summary>
    /// String event argument.
    /// </summary>
    [Serializable]
    public class CStringEventArg : CEventArgBase
    {
        public string value;
    }

    /// <summary>
    /// Boolean event argument.
    /// </summary>
    [Serializable]
    public class CBoolEventArg : CEventArgBase
    {
        public bool value;
    }

    /// <summary>
    /// Transform event argument.
    /// </summary>
    [Serializable]
    public class CTransformEventArg : CEventArgBase
    {
        public Transform transform;
    }

    /// <summary>
    /// GameObject event argument.
    /// </summary>
    [Serializable]
    public class CGameObjectEventArg : CEventArgBase
    {
        public GameObject gameObject;
    }

    /// <summary>
    /// Transform and int event argument.
    /// </summary>
    [Serializable]
    public class CTransformIntEventArg : CEventArgBase
    {
        public Transform transform;
        public int intValue;
    }

    /// <summary>
    /// Transform and float event argument.
    /// </summary>
    [Serializable]
    public class CTransformFloatEventArg : CEventArgBase
    {
        public Transform transform;
        public float floatValue;
    }

    /// <summary>
    /// Transform and string event argument.
    /// </summary>
    [Serializable]
    public class CTransformStringEventArg : CEventArgBase
    {
        public Transform transform;
        public string stringValue;
    }

    /// <summary>
    /// Transform and bool event argument.
    /// </summary>
    [Serializable]
    public class CTransformBoolEventArg : CEventArgBase
    {
        public Transform transform;
        public bool boolValue;
    }
}