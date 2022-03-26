using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
public class ChattingManager : MonoBehaviour
{
    InputField inputField;
    Image image;

    [SerializeField]
    GameObject BackGround;

    [SerializeField]
    Text text;

    // 서버에서 메세지가 오면 출력한다.
    // 왔던 메세지들을 저장해두고 보여준다.
    string[] message;
    System.Text.StringBuilder chatmess;
    int curIndex;
    const int messageBoxSize = 10;

    const float MaxShowTime = 2;
    float showTime;
    // Start is called before the first frame update
    void Awake()
    {
        curIndex = 0;
        showTime = 0;
        message = new string[messageBoxSize];   
        inputField = GetComponent<InputField>();
        image = GetComponent<Image>();
        chatmess = new System.Text.StringBuilder();

        //inputField.ac;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            
            //inputField.ActivateInputField();

            if(inputField.text.Length > 0)
            {
                FieldGameManager.Net.SendChatMess(inputField.text);
                //SetMess(inputField.text);
                inputField.text = "";
                

            }
            inputField.Select();
            image.enabled = !image.enabled;

           
        }

        if(BackGround.activeSelf && showTime < Time.time)
        {
            CloseMess();
        }

    }

    public void SetMess(string mess)
    {
        message[curIndex] = mess;
        curIndex++;
        curIndex = curIndex % messageBoxSize;

        chatmess.Append('\n' + mess );

        text.text = chatmess.ToString();
        ShowMess();
    }

    public void ShowMess()
    {
        BackGround.SetActive(true);

        showTime = Time.time + MaxShowTime;

    }
    public void CloseMess()
    {
        BackGround.SetActive(false);
        text.text = "";
        chatmess.Clear();
        curIndex = 0;
    }
}
