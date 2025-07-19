using UnityEngine;

public class GakManBehavior : MonoBehaviour
{
    private void Start()
    {
        Static.main.AddGakMen(this);
    }
}
