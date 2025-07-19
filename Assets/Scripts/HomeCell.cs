using UnityEngine;

public class HomeCell : MonoBehaviour
{
    public bool isEntrance;
    private void Awake()
    {
        Static.main.AddHomeCell(this, isEntrance);
    }
}
