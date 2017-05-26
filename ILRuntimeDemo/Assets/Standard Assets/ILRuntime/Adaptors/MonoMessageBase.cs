
using System.Collections.Generic;
using ILRuntime.Runtime.Adaptor;
using UnityEngine;

public abstract class MonoMessageBase: MonoBehaviour
{
    private HashSet<MonoBehaviourAdapter.MonoAdaptor> _monoAdaptors = new HashSet<MonoBehaviourAdapter.MonoAdaptor>();

    public int MonoAdaptorCount
    {
        get { return _monoAdaptors.Count; }
    }

    protected abstract string InfoName { get; }

    public void AddMonoAdaptor(MonoBehaviourAdapter.MonoAdaptor adaptor)
    {
        _monoAdaptors.Add(adaptor);
    }

    public void RemoveMonoAdaptor(MonoBehaviourAdapter.MonoAdaptor adaptor)
    {
        _monoAdaptors.Remove(adaptor);
    }

    protected static void ReceiveMessage(MonoMessageBase msgBase, params object[] arg)
    {
        var runAdaptorList = new List<MonoBehaviourAdapter.MonoAdaptor>();
        foreach (var monoAdaptor in msgBase._monoAdaptors)
        {
            // 担心某些比较特殊的状况，不过这样判断估计还是不够齐全，也只能将就了
            if (monoAdaptor.isActiveAndEnabled)
            {
                runAdaptorList.Add(monoAdaptor);
            }
        }

        var msgInfo = ILRMonoAdaptorHelper.AllMethodDict[msgBase.InfoName];
        foreach (var monoAdaptor in runAdaptorList)
        {
            monoAdaptor.ReceiveMessage(msgInfo.Name, arg);
        }
    }
}
