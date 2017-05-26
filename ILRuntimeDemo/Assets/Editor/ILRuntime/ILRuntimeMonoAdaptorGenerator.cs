
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class ILRuntimeMonoAdaptorGenerator
{
    public static readonly MonoMethodInfo[] GeneratedMethods =
    {
        new MonoMethodInfo(ILRMonoAdaptorHelper.Update, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.LateUpdate, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.FixedUpdate, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnGUI, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnDrawGizmos, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnDrawGizmosSelected, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnValidate, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.Reset, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTransformChildrenChanged, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTransformParentChanged, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnAudioFilterRead, 2, "float[] data, int channels"),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnApplicationFocus, 1, "bool hasFocus"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnApplicationPause, 1, "bool pauseStatus"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnApplicationQuit, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnBecameVisible, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnBecameInvisible, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnPostRender, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnPreCull, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnPreRender, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnRenderImage, 2, "RenderTexture src, RenderTexture dest"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnRenderObject, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnWillRenderObject, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseDown, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseDrag, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseEnter, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseExit, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseOver, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseUp, 0),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnMouseUpAsButton, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnAnimatorIK, 1, "int layerIndex"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnAnimatorMove, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnParticleCollision, 1, "GameObject other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnParticleTrigger, 0),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionEnter2D, 1, "Collision2D other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionExit2D, 1, "Collision2D other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionStay2D, 1, "Collision2D other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnJointBreak2D, 1, "Joint2D brokenJoint"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerEnter2D, 1, "Collider2D other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerExit2D, 1, "Collider2D other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerStay2D, 1, "Collider2D other"),

        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionEnter, 1, "Collision other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionExit, 1, "Collision other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnCollisionStay, 1, "Collision other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnControllerColliderHit, 1, "ControllerColliderHit hit"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnJointBreak, 1, "float breakForce"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerEnter, 1, "Collider other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerExit, 1, "Collider other"),
        new MonoMethodInfo(ILRMonoAdaptorHelper.OnTriggerStay, 1, "Collider other"),
    };

    public static void Generate()
    {
        ClearOutputPath(ILRuntimePaths.AdaptorCodePath);

        var sb = new StringBuilder();
        var paramSb = new StringBuilder();
        foreach (var method in GeneratedMethods)
        {
            sb.Length = 0;
            paramSb.Length = 0;

            var defParam = !string.IsNullOrEmpty(method.ParamDef);
            if (defParam)
            {
                var matches = Regex.Match(method.ParamDef, @"\w+ (\w+)");
                for (int i = 1; i < matches.Groups.Count; i++)
                {
                    paramSb.Append(i == 1 ? matches.Groups[i].Value : ", " + matches.Groups[i].Value);
                }
            }
            else
            {
                paramSb.Append("null");
            }

            sb.Append(string.Format(@"
using UnityEngine;

namespace ILRuntime.Adaptor.MonoMessage.Generated
{{
    public class MonoMessage_{0}: MonoMessageBase
    {{
        protected override string InfoName
        {{
            get {{ return ILRMonoAdaptorHelper.{0}; }}
        }}

        private void {0}({1})
        {{
            ReceiveMessage(this, {2});
        }}
    }}
}}
", method.Name, method.ParamDef, paramSb));

            FileHelper.WriteAllText(GetAdaptorCodePath(string.Format("MonoMessage_{0}", method.Name)), sb.ToString());
        }
    }

    private static void ClearOutputPath(string outputPath)
    {
        FileHelper.CreateDirectory(outputPath);
        foreach (var file in Directory.GetFiles(outputPath, "*.cs"))
        {
            File.Delete(file);
        }
    }

    private static string GetAdaptorCodePath(string fileName)
    {
        return string.Format("{0}/{1}.cs", ILRuntimePaths.AdaptorCodePath, fileName);
    }

}
