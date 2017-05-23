using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HotFix_Project
{
    public class InstanceClass
    {
        public delegate int TestFunc(long v1, float v2, int v3, int v4);

        private int id;

        public InstanceClass()
        {
            UnityEngine.Debug.Log("!!! InstanceClass::InstanceClass()");
            this.id = 0;
        }

        public InstanceClass(int id)
        {
            UnityEngine.Debug.Log("!!! InstanceClass::InstanceClass() id = " + id);
            this.id = id;
        }

        public int ID
        {
            get { return id; }
        }

        // static method
        public static void StaticFunTest()
        {
//            UnityEngine.Debug.Log("!!! InstanceClass.StaticFunTest()");
            UnityEngine.Debug.Log(" Lazy !!! InstanceClass.StaticFunTest() Lazy");
            var action = new UnityEngine.Events.UnityAction<float>(arg0 => UnityEngine.Debug.Log(arg0));
            var action2 = new UnityEngine.Events.UnityAction<InstanceClass>(arg0 => UnityEngine.Debug.Log(arg0));
            var list = new List<InstanceClass>();
            list.OrderBy(@class => @class);
            list.OrderBy(delegate(InstanceClass @class) { return @class; });
            var list2 = new List<HelloWorld>();
            list2.OrderBy(world => world);
            HelloWorld.UnityActionTest += action;
            HelloWorld.GenericMethod<InstanceClass>();
            HelloWorld.GenericMethod<HelloWorld>();
            UnityEngine.Debug.Log(HelloWorld.Property);
            UnityEngine.Debug.Log(HelloWorld.TestEnum.Test);

            var testAction = new Action<int, float, long>((i, f, arg3) => { });
        }

        public static void StaticFunTest2(int a)
        {
            UnityEngine.Debug.Log("!!! InstanceClass.StaticFunTest2(), a=" + a);
        }

        public static void GenericMethod<T>(T a)
        {
            UnityEngine.Debug.Log("!!! InstanceClass.GenericMethod(), a=" + a);
        }
    }


}
