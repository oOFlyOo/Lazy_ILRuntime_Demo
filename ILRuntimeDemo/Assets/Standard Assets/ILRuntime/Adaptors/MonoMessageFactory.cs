
using System;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Adaptor;
using UnityEngine;

public static class MonoMessageFactory
{
    private static readonly Dictionary<string, Type> _monoMethodTypeDict;

    static MonoMessageFactory()
    {
        _monoMethodTypeDict = new Dictionary<string, Type>();
        foreach (var monoMethodInfo in ILRMonoAdaptorHelper.CheckMethods)
        {
            _monoMethodTypeDict[monoMethodInfo.Name] =
                Type.GetType("ILRuntime.Adaptor.MonoMessage.Generated.MonoMessage_" + monoMethodInfo.Name);
        }
    }

public static void RegisterMonoMessage(MonoBehaviourAdapter.MonoAdaptor adaptor)
    {
        foreach (var pair in adaptor.MonoMethodDict)
        {
            Type msgType;
            if (_monoMethodTypeDict.TryGetValue(pair.Key, out msgType))
            {
                var msgBase = AddOrGetComponent(adaptor, msgType);
                msgBase.AddMonoAdaptor(adaptor);
            }
        }
    }

    public static void UnRegisterMonoMessage(MonoBehaviourAdapter.MonoAdaptor adaptor)
    {
        foreach (var pair in adaptor.MonoMethodDict)
        {
            Type msgType;
            if (_monoMethodTypeDict.TryGetValue(pair.Key, out msgType))
            {
                var msgBase = adaptor.GetComponent(msgType) as MonoMessageBase;
                if (msgBase != null)
                {
                    msgBase.RemoveMonoAdaptor(adaptor);

                    // 这里可以考虑缓存
                    if (msgBase.MonoAdaptorCount <= 0)
                    {
                        UnityEngine.Object.Destroy(msgBase);
                    }
                }
                else
                {
                    throw new MissingComponentException(msgType.FullName);
                }
            }
        }
    }

    private static MonoMessageBase AddOrGetComponent(MonoBehaviourAdapter.MonoAdaptor adaptor, Type type)
    {
        var com = adaptor.GetComponent(type);
        if (com == null)
        {
            com = adaptor.gameObject.AddComponent(type);
        }

        return com as MonoMessageBase;
    }
}
