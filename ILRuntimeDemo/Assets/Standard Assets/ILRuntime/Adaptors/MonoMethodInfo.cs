
public class MonoMethodInfo
{
    public string Name;
    public int ParamCount;
    public MonoMethodInfo(string name, int paramCount)
    {
        Name = name;
        ParamCount = paramCount;
    }

    public string ParamDef;
    public MonoMethodInfo(string name, int paramCount, string paramDef)
    {
        Name = name;
        ParamCount = paramCount;
        ParamDef = paramDef;
    }
}
