using UnityEngine;

public class TeleportReference : MonoBehaviour
{
    public TeleportReference target;

    private void Start()
    {
        Static.main.AddTPRef(this);
    }
}
