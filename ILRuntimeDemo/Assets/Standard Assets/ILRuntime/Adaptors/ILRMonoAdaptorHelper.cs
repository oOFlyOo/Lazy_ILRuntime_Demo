
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Adaptor;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;

public static class ILRMonoAdaptorHelper
{
    private static Dictionary<ILType, Type> _cacheMonoAdaptorTypeDict = new Dictionary<ILType, Type>();
    internal static Type GetMonoAdaptorType(ILType ilType)
    {
        Type adaptorType = null;
        if (!_cacheMonoAdaptorTypeDict.TryGetValue(ilType, out adaptorType))
        {
            foreach (var monoMethodInfo in EnableMethods)
            {
                if (ilType.GetMethod(monoMethodInfo.Name, monoMethodInfo.ParamCount) != null)
                {
                    adaptorType = MonoEnableAdaptorType;
                    break;
                }
            }

            if (adaptorType == null)
            {
                adaptorType = MonoAdaptorType;
            }
            _cacheMonoAdaptorTypeDict[ilType] = adaptorType;
        }

        return adaptorType;
    }

    internal static bool IsMonoAdaptor(this MonoBehaviour mono)
    {
        return (mono as MonoBehaviourAdapter.MonoAdaptor) != null;
    }

    internal static bool IsMonoAdaptor(this Type type)
    {
        return MonoAdaptorType.IsAssignableFrom(type);
    }

#region 获取 component
    private static List<MonoBehaviourAdapter.MonoAdaptor> _tempMonoList = new List<MonoBehaviourAdapter.MonoAdaptor>();
    private static List<MonoBehaviourAdapter.MonoEnableAdaptor> _tempMonoEnableList = new List<MonoBehaviourAdapter.MonoEnableAdaptor>();
    private static List<ILTypeInstance> _tempInstanceList = new List<ILTypeInstance>();

    private static List<ILTypeInstance> CheckInstanceList(List<ILTypeInstance> list)
    {
        list = list ?? _tempInstanceList;
        list.Clear();
        return list;
    }

    private static ILTypeInstance CheckComponentAndClearList(IList list, ILType type)
    {
        MonoBehaviourAdapter.MonoAdaptor adaptor = null;
        for (int i = 0; i < list.Count; i++)
        {
            adaptor = list[i] as MonoBehaviourAdapter.MonoAdaptor;
            if (adaptor.ILInstance.Type == type)
            {
                break;
            }
        }
        list.Clear();

        return adaptor == null ? null : adaptor.ILInstance;
    }

    private static ILTypeInstance[] CheckComponentsAndClearList(IList list, ILType type, List<ILTypeInstance> results)
    {
        var tempList = CheckInstanceList(results);
        for (int i = 0; i < list.Count; i++)
        {
            var adaptor = list[i] as MonoBehaviourAdapter.MonoAdaptor;
            if (adaptor.ILInstance.Type == type)
            {
                tempList.Add(adaptor.ILInstance);
            }
        }
        list.Clear();

        if (results == null)
        {
            var array = tempList.ToArray();
            tempList.Clear();
            return array;
        }
        return null;
    }

    private static IList GetComponentsWithoutCheck(Component component, ILType type)
    {
        var adaptorType = GetMonoAdaptorType(type);
        if (adaptorType == MonoAdaptorType)
        {
            component.GetComponents(_tempMonoList);
            return _tempMonoList;
        }
        component.GetComponents(_tempMonoEnableList);
        return _tempMonoEnableList;
    }

    private static ILTypeInstance PrivateGetComponent(Component component, ILType type)
    {
        var list = GetComponentsWithoutCheck(component, type);

        return CheckComponentAndClearList(list, type);
    }

    private static ILTypeInstance[] PrivateGetComponents(Component component, ILType type, List<ILTypeInstance> results = null)
    {
        var list = GetComponentsWithoutCheck(component, type);

        return CheckComponentsAndClearList(list, type, results);
    }

    internal static object GetComponent(Component component, Type type)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponent(component, (type as ILRuntimeType).ILType);
        }
        return component.GetComponent(type);
    }

    internal static object GetComponent(Component component, IType type)
    {
        return GetComponent(component, type is ILType ? type.ReflectionType : type.TypeForCLR);
    }

    internal static object GetComponents(Component component, Type type)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponents(component, (type as ILRuntimeType).ILType, null);
        }
        return component.GetComponents(type);
    }

    internal static object GetComponents(Component component, IType type)
    {
        return GetComponents(component, type is ILType ? type.ReflectionType : type.TypeForCLR);
    }

    private static IList GetComponentsInChildrenWithoutCheck(Component component, ILType type, bool includeInactive)
    {
        var adaptorType = GetMonoAdaptorType(type);
        if (adaptorType == MonoAdaptorType)
        {
            component.GetComponentsInChildren(includeInactive, _tempMonoList);
            return _tempMonoList;
        }
        component.GetComponentsInChildren(includeInactive, _tempMonoEnableList);
        return _tempMonoEnableList;
    }

    private static ILTypeInstance PrivateGetComponentInChildren(Component component, ILType type, bool includeInactive)
    {
        var list = GetComponentsInChildrenWithoutCheck(component, type, includeInactive);

        return CheckComponentAndClearList(list, type);
    }

    private static ILTypeInstance[] PrivateGetComponentsInChildren(Component component, ILType type, bool includeInactive, List<ILTypeInstance> results = null)
    {
        var list = GetComponentsInChildrenWithoutCheck(component, type, includeInactive);

        return CheckComponentsAndClearList(list, type, results);
    }

    internal static object GetComponentInChildren(Component component, Type type, bool includeInactive = false)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentInChildren(component, (type as ILRuntimeType).ILType, includeInactive);
        }
        return component.GetComponentInChildren(type, includeInactive);
    }

    internal static object GetComponentInChildren(Component component, IType type, bool includeInactive = false)
    {
        return GetComponentInChildren(component, type is ILType ? type.ReflectionType : type.TypeForCLR, includeInactive);
    }

    internal static object GetComponentsInChildren(Component component, Type type, bool includeInactive = false)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentsInChildren(component, (type as ILRuntimeType).ILType, includeInactive, null);
        }
        return component.GetComponentsInChildren(type, includeInactive);
    }

    internal static object GetComponentsInChildren(Component component, IType type, bool includeInactive = false)
    {
        return GetComponentsInChildren(component, type is ILType ? type.ReflectionType : type.TypeForCLR, includeInactive);
    }

    private static IList GetComponentsInParentWithoutCheck(Component component, ILType type, bool includeInactive)
    {
        var adaptorType = GetMonoAdaptorType(type);
        if (adaptorType == MonoAdaptorType)
        {
            component.GetComponentsInParent(includeInactive, _tempMonoList);
            return _tempMonoList;
        }
        component.GetComponentsInParent(includeInactive, _tempMonoEnableList);
        return _tempMonoEnableList;
    }

    private static ILTypeInstance PrivateGetComponentInParent(Component component, ILType type, bool includeInactive = false)
    {
        var list = GetComponentsInParentWithoutCheck(component, type, includeInactive);

        return CheckComponentAndClearList(list, type);
    }

    private static ILTypeInstance[] PrivateGetComponentsInParent(Component component, ILType type, bool includeInactive, List<ILTypeInstance> results = null)
    {
        var list = GetComponentsInParentWithoutCheck(component, type, includeInactive);

        return CheckComponentsAndClearList(list, type, results);
    }

    internal static object GetComponentInParent(Component component, Type type)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentInParent(component, (type as ILRuntimeType).ILType);
        }
        return component.GetComponentInParent(type);
    }

    internal static object GetComponentInParent(Component component, IType type)
    {
        return GetComponentInParent(component, type is ILType ? type.ReflectionType : type.TypeForCLR);
    }

    internal static object GetComponentsInParent(Component component, Type type, bool includeInactive = false)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentsInParent(component, (type as ILRuntimeType).ILType, includeInactive, null);
        }
        return component.GetComponentsInParent(type, includeInactive);
    }

    internal static object GetComponentsInParent(Component component, IType type, bool includeInactive = false)
    {
        return GetComponentsInParent(component, type is ILType ? type.ReflectionType : type.TypeForCLR, includeInactive);
    }


    #endregion

    #region 记录用
    private static Dictionary<string, MonoMethodInfo> _allMethodDict;
    public static Dictionary<string, MonoMethodInfo> AllMethodDict
    {
        get
        {
            if (_allMethodDict == null)
            {
                _allMethodDict = new Dictionary<string, MonoMethodInfo>();
                foreach (var info in BaseMethods)
                {
                    _allMethodDict[info.Name] = info;
                }
                foreach (var info in EnableMethods)
                {
                    _allMethodDict[info.Name] = info;
                }
                foreach (var info in CheckMethods)
                {
                    _allMethodDict[info.Name] = info;
                }
            }
            return _allMethodDict;
        }
    }

    public static readonly MonoMethodInfo[] BaseMethods =
    {
        new MonoMethodInfo(Awake, 0),
        new MonoMethodInfo(Start, 0),
        new MonoMethodInfo(OnDestroy, 0),
    };

    public static readonly MonoMethodInfo[] EnableMethods =
    {
        new MonoMethodInfo(OnEnable, 0),
        new MonoMethodInfo(OnDisable, 0),
    };

    public static readonly MonoMethodInfo[] CheckMethods =
    {
        new MonoMethodInfo(ILRMonoAdaptorHelper.Update, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.LateUpdate, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.FixedUpdate, 0),

//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnGUI, 0),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnDrawGizmos, 0),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnDrawGizmosSelected, 0),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnValidate, 0),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.Reset, 0),

//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTransformChildrenChanged, 0),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTransformParentChanged, 0),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnAudioFilterRead, 2),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnApplicationFocus, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnApplicationPause, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnApplicationQuit, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnBecameVisible, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnBecameInvisible, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnPostRender, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnPreCull, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnPreRender, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnRenderImage, 2),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnRenderObject, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnWillRenderObject, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseDown, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseDrag, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseEnter, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseExit, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseOver, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseUp, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseUpAsButton, 0),

//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnAnimatorIK, 1),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnAnimatorMove, 0),

//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnParticleCollision, 1),
//        new MonoMethodInfo(ILRMonoAdaptorHelper.OnParticleTrigger, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionEnter2D, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionExit2D, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionStay2D, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnJointBreak2D, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerEnter2D, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerExit2D, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerStay2D, 1),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionEnter, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionExit, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionStay, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnControllerColliderHit, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnJointBreak, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerEnter, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerExit, 1),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerStay, 1),
    };
    #endregion

    #region 通用的几个
    public static readonly Type MonoAdaptorType = typeof(ILRuntime.Runtime.Adaptor.MonoBehaviourAdapter.MonoAdaptor);

    public const string Awake = "Awake";
    public const string Start = "Start";
    public const string OnDestroy = "OnDestroy";
    #endregion

    #region 通用的，但是考虑效率放这
    public static readonly Type MonoEnableAdaptorType = typeof(ILRuntime.Runtime.Adaptor.MonoBehaviourAdapter.MonoEnableAdaptor);

    public const string OnEnable = "OnEnable";
    public const string OnDisable = "OnDisable";
    #endregion

    #region 调用频繁
    public const string Update = "Update";
    public const string LateUpdate = "LateUpdate";
    public const string FixedUpdate = "FixedUpdate";
    public const string OnGUI = "OnGUI";
    public const string OnDrawGizmos = "OnDrawGizmos";
    public const string OnDrawGizmosSelected = "OnDrawGizmosSelected";
    public const string OnValidate = "OnValidate";
    public const string Reset = "Reset";
    public const string OnTransformChildrenChanged = "OnTransformChildrenChanged";
    public const string OnTransformParentChanged = "OnTransformParentChanged";
    public const string OnAudioFilterRead = "OnAudioFilterRead";

    #endregion

    #region 系统相关
    public const string OnApplicationFocus = "OnApplicationFocus";
    public const string OnApplicationPause = "OnApplicationPause";
    public const string OnApplicationQuit = "OnApplicationQuit";
    #endregion

    #region 渲染相关
    public const string OnBecameVisible = "OnBecameVisible";
    public const string OnBecameInvisible = "OnBecameInvisible";
    public const string OnPostRender = "OnPostRender";
    public const string OnPreCull = "OnPreCull";
    public const string OnPreRender = "OnPreRender";
    public const string OnRenderImage = "OnRenderImage";
    public const string OnRenderObject = "OnRenderObject";
    public const string OnWillRenderObject = "OnWillRenderObject";
    #endregion

    #region 点击相关
    public const string OnMouseDown = "OnMouseDown";
    public const string OnMouseDrag = "OnMouseDrag";
    public const string OnMouseEnter = "OnMouseEnter";
    public const string OnMouseExit = "OnMouseExit";
    public const string OnMouseOver = "OnMouseOver";
    public const string OnMouseUp = "OnMouseUp";
    public const string OnMouseUpAsButton = "OnMouseUpAsButton";
    #endregion

    #region 动作相关
    public const string OnAnimatorIK = "OnAnimatorIK";
    public const string OnAnimatorMove = "OnAnimatorMove";
    #endregion

    #region 粒子相关
    public const string OnParticleCollision = "OnParticleCollision";
    public const string OnParticleTrigger = "OnParticleTrigger";
    #endregion

    #region 2D 碰撞相关
    public const string OnCollisionEnter2D = "OnCollisionEnter2D";
    public const string OnCollisionExit2D = "OnCollisionExit2D";
    public const string OnCollisionStay2D = "OnCollisionStay2D";
    public const string OnJointBreak2D = "OnJointBreak2D";
    public const string OnTriggerEnter2D = "OnTriggerEnter2D";
    public const string OnTriggerExit2D = "OnTriggerExit2D";
    public const string OnTriggerStay2D = "OnTriggerStay2D";
    #endregion

    #region 3D 碰撞相关
    public const string OnCollisionEnter = "OnCollisionEnter";
    public const string OnCollisionExit = "OnCollisionExit";
    public const string OnCollisionStay = "OnCollisionStay";
    public const string OnControllerColliderHit = "OnControllerColliderHit";
    public const string OnJointBreak = "OnJointBreak";
    public const string OnTriggerEnter = "OnTriggerEnter";
    public const string OnTriggerExit = "OnTriggerExit";
    public const string OnTriggerStay = "OnTriggerStay";
    #endregion
}
