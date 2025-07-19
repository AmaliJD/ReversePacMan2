using UnityEngine;

public class HomeCell : MonoBehaviour
{
    private void Start() => Static.main.AddHomeCell(this);
}
