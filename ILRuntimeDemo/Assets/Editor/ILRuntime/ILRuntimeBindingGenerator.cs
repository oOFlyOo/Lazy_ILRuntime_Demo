


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

public class ILRuntimeBindingGenerator
{
    public static void CLRBinding()
    {
        var domain = ILRuntimeManager.Create().Domain;

        var moduleDef = ILRuntimeBindingHelper.ReadModule(ILRuntimePaths.AssemblyCSharpPath);
        GenerateFramworkMessage(moduleDef);

        var refDict = ILRuntimeBindingHelper.GetAllMonoCecilReference(domain, moduleDef);
        var typeList = ILRuntimeBindingHelper.GetAllReference(domain, refDict);
        CLRBindingCode(typeList);

        ILRuntimeManager.DestroyInstance();
    }

    private static void GenerateFramworkMessage(ModuleDefinition moduleDef)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        foreach (var reference in moduleDef.AssemblyReferences)
        {
            sb.AppendLine(reference.ToString());
        }

        var domain = AppDomain.CreateDomain(typeof (ILRuntimeBindingGenerator).FullName);
        var frameworkAssembly = domain.Load(FileHelper.ReadAllBytes(ILRuntimePaths.FrameworkyCSharpPath));
        var types = frameworkAssembly.GetExportedTypes();
        var typeList = types.ToList();
        typeList.Sort((type, type1) =>
        {
            return type.FullName.CompareTo(type1.FullName);
        });
        foreach (var type in typeList)
        {
            sb.AppendLine();
            sb.AppendLine(type.ToString());
            foreach (var fieldInfo in type.GetFields())
            {
                sb.AppendLine(fieldInfo.ToString());
            }
            foreach (var constructorInfo in type.GetConstructors())
            {
                sb.AppendLine(constructorInfo.ToString());
            }
            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                sb.AppendLine(methodInfo.ToString());
            }
        }

        FileHelper.WriteAllText(ILRuntimePaths.FrameworkMessagePath, sb.ToString());
    }


    private static void CLRBindingCode(List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        ClearOutputPath(ILRuntimePaths.BindingCodePath);

        GenerateCLRBindingCode(typeInfoList);

        GenerateCLRBindingMessage(typeInfoList);
    }


    private static void ClearOutputPath(string outputPath)
    {
        FileHelper.CreateDirectory(outputPath);
        foreach (var file in Directory.GetFiles(outputPath, "*.cs"))
        {
            File.Delete(file);
        }
    }

    private static void GenerateCLRBindingMessage(List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        typeInfoList.Sort((info, typeInfo) =>
        {
            return info.DeclaringType.FullName.CompareTo(typeInfo.DeclaringType.FullName);
        });

        var sb = new StringBuilder();
        foreach (var info in typeInfoList)
        {
            sb.AppendLine();
            sb.AppendLine(info.ToString());
        }

        FileHelper.WriteAllText(ILRuntimePaths.BindingCodeMessagePath, sb.ToString());
    }

    private static void GenerateCLRBindingCode(List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        var sb = new StringBuilder();

        sb.Append(@"
namespace ILRuntime.Binding.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
");
        sb.Append(@"
        }
    }
}
");

        File.WriteAllText(GetBindingCodePath("CLRBindings"), sb.ToString());
    }

    private static string GetBindingCodePath(string fileName)
    {
        return string.Format("{0}/{1}.cs", ILRuntimePaths.BindingCodePath, fileName);
    }
}
