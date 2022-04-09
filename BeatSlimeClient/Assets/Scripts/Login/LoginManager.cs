using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public Animator self;

    public TMPro.TMP_Text ID;
    public TMPro.TMP_Text Password;
    // Start is called before the first frame update
    public void scrollToLogin()
    {
        ID.text = "";
        Password.text = "";
        self.SetTrigger("scrollToLogin");
        print("scrollToLogin");
    }

    public void scrollToCenter()
    {
        self.SetTrigger("scrollToCenter");
        
    }

    public void SendLogin()
    {

    }
}
