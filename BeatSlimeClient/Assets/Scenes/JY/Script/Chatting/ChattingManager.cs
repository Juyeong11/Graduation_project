using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
public class ChattingManager : MonoBehaviour
{
    InputField inputField;

    [SerializeField]
    GameObject image;

    [SerializeField]
    GameObject BackGround;

    [SerializeField]
    Text text;
    [SerializeField]
    Dropdown SendType;

    // 서버에서 메세지가 오면 출력한다.
    // 왔던 메세지들을 저장해두고 보여준다.
    string[] message;
    System.Text.StringBuilder chatmess;
    int curIndex;
    const int messageBoxSize = 10;

    const float MaxShowTime = 5;
    float showTime;
    // Start is called before the first frame update
    void Awake()
    {
        curIndex = 0;
        showTime = 0;
        message = new string[messageBoxSize];
        image.SetActive(true);
        inputField = GetComponentInChildren<InputField>();
        chatmess = new System.Text.StringBuilder();

        image.SetActive(false);

        //inputField.ac;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {

            ShowMess();


            image.SetActive(!image.activeSelf);

            //inputField.Select();
            inputField.ActivateInputField();



            //inputField.ActivateInputField();

            if (inputField.text.Length > 0)
            {
                byte sendtype = (byte)SendType.value;
                if (sendtype == 2)//귓말이면
                {
                    string id = inputField.text.Split(' ')[0];
                    FieldGameManager.Net.SendChatMess(inputField.text, sendtype, int.Parse(id));

                }
                else
                {
                    FieldGameManager.Net.SendChatMess(inputField.text, sendtype, -1);
                }
                //SetMess(inputField.text);
                inputField.text = "";


            }




        }

        if (BackGround.activeSelf && showTime < Time.time)
        {
            CloseMess();
        }

    }

    public void SetMess(string mess)
    {
        curIndex++;
        //curIndex = curIndex % messageBoxSize;
        if (curIndex >= 10)
        {
            int end = chatmess.ToString().IndexOf('\n', 1);
            chatmess.Remove(0, end);
        }
        chatmess.Append('\n' + mess);

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
