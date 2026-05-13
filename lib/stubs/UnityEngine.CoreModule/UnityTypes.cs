// UnityEngine.CoreModule stub — compile-time only, no runtime use.
using System;

namespace UnityEngine
{
    public class Object
    {
        public static void DontDestroyOnLoad(Object target) => throw new NotImplementedException();
    }

    public class Component : Object
    {
        public GameObject gameObject => throw new NotImplementedException();
    }

    public class Behaviour : Component { }

    public class MonoBehaviour : Behaviour { }

    public class GameObject : Object { }

    public class Debug
    {
        public static void Log(object message) => throw new NotImplementedException();
        public static void LogWarning(object message) => throw new NotImplementedException();
        public static void LogError(object message) => throw new NotImplementedException();
    }

    public class Time
    {
        public static float realtimeSinceStartup => throw new NotImplementedException();
    }

    public class Mathf
    {
        public static int RoundToInt(float f) => throw new NotImplementedException();
    }
}
