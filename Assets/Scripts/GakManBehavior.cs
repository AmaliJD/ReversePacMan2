using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MovementController))]
public class GakManBehavior : MonoBehaviour
{
    MovementController movementController;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        Static.main.AddGakMen(this);
    }

    public void BehaviorUpdate()
    {
        Visuals();
    }

    void Visuals()
    {
        if (movementController.GetMoveDirection() == Vector2.right && transform.localScale.x != 1)
            transform.localScale = new Vector3(1, 1, 1);
        else if (movementController.GetMoveDirection() == Vector2.left && transform.localScale.x != -1)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void GetEgg(Egg egg)
    {
        int eggIndex = Static.main.eggs.IndexOf(egg);

        Static.main.eggPositions.RemoveAt(eggIndex);
        Static.main.eggs.RemoveAt(eggIndex);
        egg.gameObject.SetActive(false);

        Static.main.eggsCollected++;

        if (egg.PowerEgg)
        {
            Static.main.ScareGhosts();
        }

        Static.main.eggs.Remove(egg);
    }

    public void InstantEatTouchingGhosts()
    {
        Collider2D[] ghostColliders = Physics2D.OverlapCircleAll(transform.position, GetComponent<CircleCollider2D>().radius, 1 << LayerMask.NameToLayer("Ghost"));
        foreach (Collider2D collider in ghostColliders)
        {
            GhostBehavior ghost = collider.GetComponent<GhostBehavior>();
            if (ghost.state == GhostBehavior.GhostState.Scared)
                ghost.state = GhostBehavior.GhostState.Eaten;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Egg")
            GetEgg(collision.GetComponent<Egg>());
        else if (collision.tag == "Ghost")
        {
            GhostBehavior ghost = collision.GetComponent<GhostBehavior>();
            if (ghost.state == GhostBehavior.GhostState.Scared)
                ghost.state = GhostBehavior.GhostState.Eaten;
        }
    }
}
