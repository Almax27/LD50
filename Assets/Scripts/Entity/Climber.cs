using UnityEngine;
using System.Collections;

public class Climber : MonoBehaviour {

    public bool canClimb = false;
    public bool atTopOfLadder = false;
    public LayerMask climbingMask = new LayerMask();
    public float lockX = 0;

    public Vector2 size = Vector2.one;

    void FixedUpdate()
    {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
        var climbable = Physics2D.OverlapArea(pos2D - (size*0.5f), pos2D + (size*0.5f), climbingMask);
        var climbableAbove = Physics2D.OverlapArea(pos2D - new Vector2(size.x * 0.5f, 0), pos2D + (size*0.5f), climbingMask);
        if (climbable)
        {
            canClimb = true;
            lockX = climbable.bounds.center.x;
            atTopOfLadder = !climbableAbove;
        }
        else
        {
            canClimb = false;
            atTopOfLadder = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size.x, size.y, 0));
    }
}
