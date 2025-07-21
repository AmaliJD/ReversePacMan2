using EX;
using GLG;
using UnityEngine;

public class TeleportReference : MonoBehaviour
{
    public TeleportReference target;

    private void Awake()
    {
        if (target == null)
            target = this;

        Static.main.AddTPRef(this);
    }

    public void Gizmos()
    {
        if (target == null)
            return;

        GLGizmos.SetColor(new Color(.5f, 1, 0, .1f));
        GLGizmos.DrawBezier(transform.position, target.transform.position, 1);
    }
}
