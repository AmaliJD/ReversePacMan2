using UnityEngine;

public class HomeCell : MonoBehaviour
{
    public bool isEntrance;
    private void Start()
    {
        Static.main.AddHomeCell(this, isEntrance);
    }
}
