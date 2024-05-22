using System;
using UnityEngine.Events;

namespace com.brg.Common
{
    [Serializable]
    public class EventWrapper
    {
        public event Action FunctionalEvent;
        public UnityEvent UnityEvent;

        public void Invoke()
        {
            FunctionalEvent?.Invoke();
            UnityEvent?.Invoke();
        }

        public static EventWrapper operator+(EventWrapper o, Action e)
        {
            o.FunctionalEvent += e;
            return o;
        }

        public static EventWrapper operator-(EventWrapper o, Action e)
        {
            o.FunctionalEvent -= e;
            return o;
        }
    }

    [Serializable]
    public class EventWrapper<T0>
    {
        public event Action<T0> FunctionalEvent;
        public UnityEvent<T0> UnityEvent;

        public void Invoke(T0 t)
        {
            FunctionalEvent?.Invoke(t);
            UnityEvent?.Invoke(t);
        }

        public static EventWrapper<T0> operator +(EventWrapper<T0> o, Action<T0> e)
        {
            o.FunctionalEvent += e;
            return o;
        }

        public static EventWrapper<T0> operator -(EventWrapper<T0> o, Action<T0> e)
        {
            o.FunctionalEvent -= e;
            return o;
        }
    }

    [Serializable]
    public class EventWrapper<T0, T1>
    {
        public event Action<T0, T1> FunctionalEvent;
        public UnityEvent<T0, T1> UnityEvent;

        public void Invoke(T0 t0, T1 t1)
        {
            FunctionalEvent?.Invoke(t0, t1);
            UnityEvent?.Invoke(t0, t1);
        }

        public static EventWrapper<T0, T1> operator +(EventWrapper<T0, T1> o, Action<T0, T1> e)
        {
            o.FunctionalEvent += e;
            return o;
        }

        public static EventWrapper<T0, T1> operator -(EventWrapper<T0, T1> o, Action<T0, T1> e)
        {
            o.FunctionalEvent -= e;
            return o;
        }
    }

    [Serializable]
    public class EventWrapper<T0, T1, T2>
    {
        public event Action<T0, T1, T2> FunctionalEvent;
        public UnityEvent<T0, T1, T2> UnityEvent;

        public void Invoke(T0 t0, T1 t1, T2 t2)
        {
            FunctionalEvent?.Invoke(t0, t1,  t2);
            UnityEvent?.Invoke(t0, t1, t2);
        }

        public static EventWrapper<T0, T1, T2> operator +(EventWrapper<T0, T1, T2> o, Action<T0, T1, T2> e)
        {
            o.FunctionalEvent += e;
            return o;
        }

        public static EventWrapper<T0, T1, T2> operator -(EventWrapper<T0, T1, T2> o, Action<T0, T1, T2> e)
        {
            o.FunctionalEvent -= e;
            return o;
        }
    }


    [Serializable]
    public class EventWrapper<T0, T1, T2, T3>
    {
        public event Action<T0, T1, T2, T3> FunctionalEvent;
        public UnityEvent<T0, T1, T2, T3> UnityEvent;

        public void Invoke(T0 t0, T1 t1, T2 t2, T3 t3)
        {
            FunctionalEvent?.Invoke(t0, t1, t2, t3);
            UnityEvent?.Invoke(t0, t1, t2, t3);
        }

        public static EventWrapper<T0, T1, T2, T3> operator +(EventWrapper<T0, T1, T2, T3> o, Action<T0, T1, T2, T3> e)
        {
            o.FunctionalEvent += e;
            return o;
        }

        public static EventWrapper<T0, T1, T2, T3> operator -(EventWrapper<T0, T1, T2, T3> o, Action<T0, T1, T2, T3> e)
        {
            o.FunctionalEvent -= e;
            return o;
        }
    }
}