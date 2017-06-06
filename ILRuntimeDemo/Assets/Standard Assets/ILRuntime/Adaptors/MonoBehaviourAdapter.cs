
using System;
using System.Collections;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace ILRuntime.Runtime.Adaptor
{
    public class MonoBehaviourAdapter : CrossBindingAdaptor
    {

        public override Type BaseCLRType
        {
            get { return typeof (MonoBehaviour); }
        }

        public override Type AdaptorType
        {
            get { return typeof (MonoAdaptor); }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain,
            ILTypeInstance instance)
        {
            throw new NotSupportedException("MonoBehaviour 不支持 new");
        }

        public class MonoAdaptor : MonoBehaviour, CrossBindingAdaptorType
        {
            private static Dictionary<ILType, Dictionary<string, ILMethod>> _cacheMonoMethods = new Dictionary<ILType, Dictionary<string, ILMethod>>();

            private static void InitMonoMethods(ILType ilType)
            {
                if (!_cacheMonoMethods.ContainsKey(ilType))
                {
                    var methodDict = new Dictionary<string, ILMethod>();
                    _cacheMonoMethods[ilType] = methodDict;
                    foreach (var pair in ILRMonoAdaptorHelper.AllMethodDict)
                    {
                        var info = pair.Value;
                        var method = ilType.GetMethod(info.Name, info.ParamCount) as ILMethod;
                        if (method != null)
                        {
                            methodDict[info.Name] = method;
                        }
                    }
                }
            }

            protected ILTypeInstance _instance;
            private ILRuntime.Runtime.Enviorment.AppDomain _appdomain;

            public ILTypeInstance ILInstance
            {
                get { return _instance; }
            }

            public Dictionary<string, ILMethod> MonoMethodDict
            {
                get { return _cacheMonoMethods[_instance.Type]; }
            }

            public virtual void Init(ILTypeInstance instance, ILRuntime.Runtime.Enviorment.AppDomain domain)
            {
                _instance = instance;
                _appdomain = domain;

                instance.CLRInstance = this;

                InitMonoMethods(instance.Type);

                MonoMessageFactory.RegisterMonoMessage(this);

                if (isActiveAndEnabled)
                {
                    Awake();
                }
            }

            public void ReceiveMessage(string methodName, params object[] arg)
            {
                ILMethod method;
                if (_cacheMonoMethods[_instance.Type].TryGetValue(methodName, out method))
                {
                    _appdomain.Invoke(method, _instance, arg);
                }
            }

            private void Awake()
            {
                //Unity会在ILRuntime准备好这个实例前调用Awake，所以这里暂时先不掉用
                if (_instance != null)
                {
                    ReceiveMessage(ILRMonoAdaptorHelper.Awake, null);
                }
            }

            private void Start()
            {
                ReceiveMessage(ILRMonoAdaptorHelper.Start, null);
            }

            private void OnDestroy()
            {
                ReceiveMessage(ILRMonoAdaptorHelper.OnDestroy, null);

                MonoMessageFactory.UnRegisterMonoMessage(this);
            }
        }

        public class MonoEnableAdaptor : MonoAdaptor
        {
            public override void Init(ILTypeInstance instance, AppDomain domain)
            {
                base.Init(instance, domain);

                if (isActiveAndEnabled)
                {
                    // 这里潜规则了，别在别的地方重复调用
                    OnEnable();
                }
            }

            private void OnEnable()
            {
                //Unity会在ILRuntime准备好这个实例前调用 OnEnable，所以这里暂时先不掉用
                if (_instance != null)
                {
                    ReceiveMessage(ILRMonoAdaptorHelper.OnEnable, null);
                }
            }

            private void OnDisable()
            {
                ReceiveMessage(ILRMonoAdaptorHelper.OnDisable, null);
            }
        }
    }
}