﻿
using System;
using System.Collections;
using System.Collections.Generic;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Adaptor;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

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

    private static IList GetComponentsWithoutCheck(GameObject go, ILType type)
    {
        var adaptorType = GetMonoAdaptorType(type);
        if (adaptorType == MonoAdaptorType)
        {
            go.GetComponents(_tempMonoList);
            return _tempMonoList;
        }
        go.GetComponents(_tempMonoEnableList);
        return _tempMonoEnableList;
    }

    private static ILTypeInstance PrivateGetComponent(GameObject go, ILType type)
    {
        var list = GetComponentsWithoutCheck(go, type);

        return CheckComponentAndClearList(list, type);
    }

    private static ILTypeInstance[] PrivateGetComponents(GameObject go, ILType type, List<ILTypeInstance> results = null)
    {
        var list = GetComponentsWithoutCheck(go, type);

        return CheckComponentsAndClearList(list, type, results);
    }

    internal static object GetComponent(GameObject go, Type type)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponent(go, (type as ILRuntimeType).ILType);
        }
        return go.GetComponent(type);
    }

    internal static object GetComponent(GameObject go, IType type)
    {
        return GetComponent(go, type is ILType ? type.ReflectionType : type.TypeForCLR);
    }

    internal static object GetComponents(GameObject go, Type type)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponents(go, (type as ILRuntimeType).ILType, null);
        }
        return go.GetComponents(type);
    }

    internal static object GetComponents(GameObject go, IType type)
    {
        return GetComponents(go, type is ILType ? type.ReflectionType : type.TypeForCLR);
    }

    private static IList GetComponentsInChildrenWithoutCheck(GameObject go, ILType type, bool includeInactive)
    {
        var adaptorType = GetMonoAdaptorType(type);
        if (adaptorType == MonoAdaptorType)
        {
            go.GetComponentsInChildren(includeInactive, _tempMonoList);
            return _tempMonoList;
        }
        go.GetComponentsInChildren(includeInactive, _tempMonoEnableList);
        return _tempMonoEnableList;
    }

    private static ILTypeInstance PrivateGetComponentInChildren(GameObject go, ILType type, bool includeInactive)
    {
        var list = GetComponentsInChildrenWithoutCheck(go, type, includeInactive);

        return CheckComponentAndClearList(list, type);
    }

    private static ILTypeInstance[] PrivateGetComponentsInChildren(GameObject go, ILType type, bool includeInactive, List<ILTypeInstance> results = null)
    {
        var list = GetComponentsInChildrenWithoutCheck(go, type, includeInactive);

        return CheckComponentsAndClearList(list, type, results);
    }

    internal static object GetComponentInChildren(GameObject go, Type type, bool includeInactive = false)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentInChildren(go, (type as ILRuntimeType).ILType, includeInactive);
        }
        return go.GetComponentInChildren(type, includeInactive);
    }

    internal static object GetComponentInChildren(GameObject go, IType type, bool includeInactive = false)
    {
        return GetComponentInChildren(go, type is ILType ? type.ReflectionType : type.TypeForCLR, includeInactive);
    }

    internal static object GetComponentsInChildren(GameObject go, Type type, bool includeInactive = false)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentsInChildren(go, (type as ILRuntimeType).ILType, includeInactive, null);
        }
        return go.GetComponentsInChildren(type, includeInactive);
    }

    internal static object GetComponentsInChildren(GameObject go, IType type, bool includeInactive = false)
    {
        return GetComponentsInChildren(go, type is ILType ? type.ReflectionType : type.TypeForCLR, includeInactive);
    }

    private static IList GetComponentsInParentWithoutCheck(GameObject go, ILType type, bool includeInactive)
    {
        var adaptorType = GetMonoAdaptorType(type);
        if (adaptorType == MonoAdaptorType)
        {
            go.GetComponentsInParent(includeInactive, _tempMonoList);
            return _tempMonoList;
        }
        go.GetComponentsInParent(includeInactive, _tempMonoEnableList);
        return _tempMonoEnableList;
    }

    private static ILTypeInstance PrivateGetComponentInParent(GameObject go, ILType type, bool includeInactive = false)
    {
        var list = GetComponentsInParentWithoutCheck(go, type, includeInactive);

        return CheckComponentAndClearList(list, type);
    }

    private static ILTypeInstance[] PrivateGetComponentsInParent(GameObject go, ILType type, bool includeInactive, List<ILTypeInstance> results = null)
    {
        var list = GetComponentsInParentWithoutCheck(go, type, includeInactive);

        return CheckComponentsAndClearList(list, type, results);
    }

    internal static object GetComponentInParent(GameObject go, Type type)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentInParent(go, (type as ILRuntimeType).ILType);
        }
        return go.GetComponentInParent(type);
    }

    internal static object GetComponentInParent(GameObject go, IType type)
    {
        return GetComponentInParent(go, type is ILType ? type.ReflectionType : type.TypeForCLR);
    }

    internal static object GetComponentsInParent(this GameObject go, Type type, bool includeInactive = false)
    {
        if (type is ILRuntimeType)
        {
            return PrivateGetComponentsInParent(go, (type as ILRuntimeType).ILType, includeInactive, null);
        }
        return go.GetComponentsInParent(type, includeInactive);
    }

    internal static object GetComponentsInParent(GameObject go, IType type, bool includeInactive = false)
    {
        return GetComponentsInParent(go, type is ILType ? type.ReflectionType : type.TypeForCLR, includeInactive);
    }

    internal static object AddComponent(GameObject go, Type type, AppDomain domain)
    {
        if (type is ILRuntimeType)
        {
            var ilType = (type as ILRuntimeType).ILType;
            //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
            var ilInstance = new ILTypeInstance(ilType, false);//手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
            //接下来创建Adapter实例
            var monoAdaptorType = GetMonoAdaptorType(ilType);
            var clrInstance = go.AddComponent(monoAdaptorType) as MonoBehaviourAdapter.MonoAdaptor;
            //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
            clrInstance.Init(ilInstance, domain);

            return ilInstance;
        }
        return go.AddComponent(type);
    }

    internal static object AddComponent(GameObject go, IType type, AppDomain domain)
    {
        return AddComponent(go, type is ILType ? type.ReflectionType : type.TypeForCLR, domain);
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
