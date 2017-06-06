
using System;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Adaptor;
using UnityEngine;

internal class MonoTimeManager : MonoSingleton<MonoTimeManager>
{
    private List<MonoTimeTask> _taskList = new List<MonoTimeTask>();

    protected override void OnInit()
    {
    }

    private enum TaskState
    {
        New,
        Wait,
        Cancel,
    }

    private class MonoTimeTask
    {
        public MonoBehaviourAdapter.MonoAdaptor Adaptor;
        public ILMethod InvokeMethod;
        public float LastInvokeTime;
        public float DelayTime;
        public float RepeatRate;
        public TaskState State;

        public MonoTimeTask(MonoBehaviourAdapter.MonoAdaptor adaptor, ILMethod invokeMethod, float delayTime)
        {
            Adaptor = adaptor;
            InvokeMethod = invokeMethod;
            DelayTime = delayTime;

            State = TaskState.New;
        }

        public MonoTimeTask(MonoBehaviourAdapter.MonoAdaptor adaptor, ILMethod invokeMethod, float delayTime, float repeatRate): this(adaptor, invokeMethod, delayTime)
        {
            RepeatRate = repeatRate;
        }

        public void Invoke()
        {
            ILRuntimeManager.Instance.Domain.Invoke(InvokeMethod, Adaptor, null);
        }
    }

    private MonoTimeTask GetTask(MonoBehaviourAdapter.MonoAdaptor adaptor, string methodName)
    {
        for (int i = 0; i < _taskList.Count; i++)
        {
            var task = _taskList[i];
            if (task.Adaptor == adaptor && task.InvokeMethod.Name == methodName)
            {
                return task;
            }
        }

        return null;
    }

    private ILMethod CheckInvoke(MonoBehaviourAdapter.MonoAdaptor adaptor, string methodName)
    {
        var task = GetTask(adaptor, methodName);
        if (task != null && task.State != TaskState.Cancel)
        {
            Debug.LogError("不支持重复 Invoke");
            return null;
        }

        var method = adaptor.ILInstance.Type.GetMethod(methodName, 0) as ILMethod;
        if (method == null)
        {
            throw new NullReferenceException();
        }
        return method;
    }

    public void Invoke(MonoBehaviourAdapter.MonoAdaptor adaptor, string methodName, float delay)
    {
        var method = CheckInvoke(adaptor, methodName);
        if (method != null)
        {
            _taskList.Add(new MonoTimeTask(adaptor, method, delay));
        }
    }

    public void InvokeRepeating(MonoBehaviourAdapter.MonoAdaptor adaptor, string methodName, float delay,
        float repeatRate)
    {
        var method = CheckInvoke(adaptor, methodName);
        if (method != null)
        {
            _taskList.Add(new MonoTimeTask(adaptor, method, delay, repeatRate));
        }

    }

    public void CancelInvoke(MonoBehaviourAdapter.MonoAdaptor adaptor, string methodName)
    {
        var task = GetTask(adaptor, methodName);
        if (task != null)
        {
            task.State = TaskState.Cancel;
        }
    }

    public void CancelInvoke(MonoBehaviourAdapter.MonoAdaptor adaptor)
    {
        for (int i = 0; i < _taskList.Count; i++)
        {
            var task = _taskList[i];
            if (task.Adaptor == adaptor)
            {
                task.State = TaskState.Cancel;
            }
        }
    }

    public bool IsInvoking(MonoBehaviourAdapter.MonoAdaptor adaptor, string methodName)
    {
        var task = GetTask(adaptor, methodName);
        return task != null && task.State == TaskState.Cancel;
    }

    public bool IsInvoking(MonoBehaviourAdapter.MonoAdaptor adaptor)
    {
        for (int i = 0; i < _taskList.Count; i++)
        {
            var task = _taskList[i];
            if (task.Adaptor == adaptor && task.State != TaskState.Cancel)
            {
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        for (int i = 0; i < _taskList.Count; i++)
        {
            var task = _taskList[i];
            if (task.Adaptor == null)
            {
                task.State = TaskState.Cancel;
                continue;
            }
            if (task.State == TaskState.Wait)
            {
                if (task.DelayTime > 0)
                {
                    task.DelayTime -= deltaTime;
                    if (task.DelayTime <= 0)
                    {
                        task.Invoke();
                        if (task.RepeatRate <= 0)
                        {
                            task.State = TaskState.Cancel;
                        }
                    }
                }
                else if (task.RepeatRate > 0)
                {
                    task.LastInvokeTime += deltaTime;
                    if (task.LastInvokeTime >= task.RepeatRate)
                    {
                        task.Invoke();
                        task.LastInvokeTime = 0;
                    }
                }
            }
            else if (task.State == TaskState.New)
            {
                task.State = TaskState.Wait;
            }
        }

        for (int i = _taskList.Count - 1; i >= 0; i--)
        {
            var task = _taskList[i];
            if (task.State == TaskState.Cancel)
            {
                _taskList.RemoveAt(i);
            }
        }
    }
}
