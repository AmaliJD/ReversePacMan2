using UnityEngine;

public class ScatterTarget : MonoBehaviour
{
    private void Awake()
    {
        Static.main.AddScatterTargets(this);
    }
}
