using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix_Project
{
    class SomeMonoBehaviour : MonoBehaviour
    {
        float time;
        public string TestStr = "dklsjflksdjfkljsdklfjsdkjfk";
        void Awake()
        {
            Debug.LogError(TestStr);
            Debug.Log("!! SomeMonoBehaviour.Awake");
        }

        void Start()
        {
            Debug.Log("!! SomeMonoBehaviour.Start");

            Invoke("TestInvoke", 3);
            StartCoroutine(TestCoroutine());
        }

        void Update()
        {
            if(Time.time - time > 1)
            {
                Debug.Log("!! SomeMonoBehaviour.Update, t=" + Time.time);
                time = Time.time;
            }
        }

        public void Test()
        {
            Debug.Log("SomeMonoBehaviour");
        }

        private void TestInvoke()
        {
            Debug.LogError("TestInvoke");
        }

        private IEnumerator TestCoroutine()
        {
            yield return new WaitForSeconds(3);
            Debug.LogError("TestCoroutine");
        }
    }

    class SomeMonoBehaviour2 : MonoBehaviour
    {
        public GameObject TargetGO;
        public Texture2D Texture;
        public void Test2()
        {
            Debug.Log("!!! SomeMonoBehaviour2.Test2");
        }

        void Start()
        {
            InvokeRepeating("TestInvokeRepeating", 3, 3);
        }

        private void TestInvokeRepeating()
        {
            Debug.LogError("TestInvokeRepeating");
        }

        private void OnEnable()
        {
            Debug.Log("!!! SomeMonoBehaviour2.OnEnable");
        }

        private void OnDisable()
        {
            Debug.Log("!!! SomeMonoBehaviour2.OnDisable");
        }

        private void OnApplicationQuit()
        {
            Debug.Log("!!! SomeMonoBehaviour2.OnApplicationQuit");
        }
    }

    class EmptyMonoBehaviour: MonoBehaviour
    {
    }

    public class TestMonoBehaviour
    {
        public static void RunTest(GameObject go)
        {
            go.AddComponent<SomeMonoBehaviour>();
            var some = go.AddComponent<SomeMonoBehaviour>();
            var coms = some.GetComponents(some.GetType());
            Debug.LogError(coms.Length);
        }

        public static void RunTest2(GameObject go)
        {
            go.AddComponent<SomeMonoBehaviour2>();
            var mb = go.GetComponent<SomeMonoBehaviour2>();
            var mb1 = mb.GetComponent(typeof(SomeMonoBehaviour)) as SomeMonoBehaviour;
            Debug.LogError(mb1.TestStr);
            go.AddComponent<EmptyMonoBehaviour>();
            var coms = mb.GetComponents(typeof(EmptyMonoBehaviour));
            Debug.LogError(coms.Length);
            Debug.Log("!!!TestMonoBehaviour.RunTest2 mb= " + mb);
//            mb.Test2();
        }
    }
}
