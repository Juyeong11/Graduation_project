using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldInputManager : MonoBehaviour
{
    public List<GameObject> Menus;
    private int menuDuplicate;

    void Awake()
    {
        menuDuplicate = 0;
    }
 
    void Update ()
    {
        if( Input.GetMouseButtonDown(1))  // 마우스가 클릭 되면
        {
            //애니메이션으로 바꿀거
            Menus[menuDuplicate].GetComponent<BillboardUI>().GetOff();
            menuDuplicate ^= 1;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                //print("something!");
                if (hit.collider.tag == "Slimes")
                {
                    Menus[menuDuplicate].transform.position = hit.transform.position;
                    Menus[menuDuplicate].GetComponent<BillboardUI>().GetOn(hit.transform);
                    //print("hit!");
                }
                else if (hit.collider.tag == "Cells")
                {
                    //A* 이동
                }
            }


        }
    }
}
