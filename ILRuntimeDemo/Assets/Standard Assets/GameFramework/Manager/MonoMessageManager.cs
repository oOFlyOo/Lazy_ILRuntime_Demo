
using System;

public class MonoMessageManager: MonoSingleton<MonoMessageManager>
{
    public event Action OnApplicationQuitEvent;

    protected override void OnInit()
    {
    }

    private void OnApplicationQuit()
    {
        if (OnApplicationQuitEvent != null)
        {
            OnApplicationQuitEvent();
        }
    }
}
