using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    private Animator animator;

    public J_HexCell selfCoord;

    public float moveSpd = 5.0f;
    public float rotateSpd = 100.0f;

    Vector3 DesPos = new Vector3(0, 0.5f, 0);
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsWalk", false);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {

            selfCoord.plus(-1, 0, 1);
            animator.SetBool("IsWalk", true);

            DesPos = selfCoord.DesPos;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selfCoord.plus(0, -1, 1); 
            animator.SetBool("IsWalk", true);

            DesPos = selfCoord.DesPos;

        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            selfCoord.plus(1, -1, 0); 
            animator.SetBool("IsWalk", true);

            DesPos = selfCoord.DesPos;

        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            selfCoord.plus(-1, 1, 0); 
            animator.SetBool("IsWalk", true);

            DesPos = selfCoord.DesPos;

        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            selfCoord.plus(0, 1, -1); 
            animator.SetBool("IsWalk", true);

            DesPos = selfCoord.DesPos;

        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selfCoord.plus(1, 0, -1); 
            animator.SetBool("IsWalk", true);

            DesPos = selfCoord.DesPos;

        }

        Vector3 pos = transform.position;
        pos = Vector3.MoveTowards(pos, DesPos, 0.5f*Time.deltaTime);
        transform.position = pos;

        if (Vector3.SqrMagnitude(pos - DesPos) <= 0.01)
        {
            animator.SetBool("IsWalk", false);
        }
        transform.LookAt(DesPos);
    }
}
