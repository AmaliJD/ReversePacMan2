using UnityEngine;

public class GakManBehavior : MonoBehaviour
{
    private void Awake()
    {
        Static.main.AddGakMen(this);
    }
}
