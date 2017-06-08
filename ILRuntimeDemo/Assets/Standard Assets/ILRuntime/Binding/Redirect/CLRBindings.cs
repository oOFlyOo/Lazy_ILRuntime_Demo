
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Binding.Redirect
{
    internal static class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(AppDomain domain)
        {
            RegisterBindingClass(domain);
            RegisterDelegateConvertor(domain.DelegateManager);
            RegisterDelegate(domain.DelegateManager);
        }

        private static void RegisterBindingClass(AppDomain domain)
        {
            UnityEngine_Component_Binding.Register(domain);
            UnityEngine_Debug_Binding.Register(domain);
            UnityEngine_MonoBehaviour_Binding.Register(domain);
        }

        private static void RegisterDelegateConvertor(DelegateManager dm)
        {
        }

        private static void RegisterDelegate(DelegateManager dm)
        {
        }
    }
}
