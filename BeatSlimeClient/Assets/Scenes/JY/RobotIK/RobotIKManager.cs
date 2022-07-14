using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class RobotIKManager : MonoBehaviour
{
    public static RobotIKManager instance = null;
    Animator animator;
    public bool ikActive = false;
    public Transform objTarget;

    public Rig aimWeight;
    public MultiAimConstraint LeftHand;
    public MultiAimConstraint RightHand;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        animator = GetComponent<Animator>();
        aimWeight.weight = 0;
    }
    void SetTarget(ref Transform target)
    {
        objTarget = target;
        LeftHand.data.sourceObjects.SetTransform(0, target);
        RightHand.data.sourceObjects.SetTransform(0, target);
    }
   
}
