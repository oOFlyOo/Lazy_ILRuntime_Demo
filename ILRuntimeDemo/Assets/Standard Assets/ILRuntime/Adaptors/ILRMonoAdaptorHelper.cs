
using System;
using System.Collections.Generic;
using System.Linq;
using ILRuntime.CLR.TypeSystem;

public static class ILRMonoAdaptorHelper
{
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


    public static Type GetMonoAdaptorType(ILType ilType)
    {
        foreach (var monoMethodInfo in EnableMethods)
        {
            if (ilType.GetMethod(monoMethodInfo.Name, monoMethodInfo.ParamCount) != null)
            {
                return MonoEnableAdaptorType;
            }
        }

        return MonoAdaptorType;
    }
}
