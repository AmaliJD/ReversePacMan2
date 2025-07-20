using UnityEngine;

public class Egg : MonoBehaviour
{
    public bool PowerEgg;

    private void Awake() => Static.main.AddEgg(this);
}
