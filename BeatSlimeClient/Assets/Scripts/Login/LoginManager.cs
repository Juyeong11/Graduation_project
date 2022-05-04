using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
public class LoginManager : MonoBehaviour
{
    public Animator self;

    public TMPro.TMP_Text ID;
    public TMPro.TMP_Text Password;

    public void Start()
    {
        Network.CreateAndConnect();
    }
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
        //print("DEBUG LOGIN");
        string id = ID.text;//.Remove(ID.text.Length-1,1);

        string idChecker = Regex.Replace(id, @"[^a-zA-Z0-9]{1,20}", "", RegexOptions.Singleline);
        id = id.Remove(ID.text.Length - 1, 1);

        if (id == "_MAPMAKER")
        {
            SceneManager.LoadScene("MapMakingScene");
            return;
        }

        if (id.Equals(idChecker) == false) { 
            print("잘못된 아이디 형식입니다 형식에 맞춰 다시 작성해 주세요(특수 문자 사용 불가능, 글자 수 20이하)"); 
            print(id.Length);
            print(id);
            print(idChecker.Length);
            print(idChecker);
            return; 
        }

        
       
        SceneManager.LoadScene("FieldScene");
        
        Network.SendLogIn(id);
    }
}
