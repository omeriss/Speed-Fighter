using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Massage
{
    public static Queue<Massage> massages;
    private float desplayTime = 6;
    DateTime exp;
    string massage;

    public Massage(string massage)
    {
        this.massage = massage;
        this.exp = DateTime.Now.AddSeconds(desplayTime);
        massages.Enqueue(this);
        UIManager.instance.Chat.text = UIManager.instance.Chat.text + this.massage + "\n";
    }

    public static void Clearmessages()
    {
        while(massages != null && massages.Count > 0 && massages.Peek().exp <= DateTime.Now)
        {
            int ind = UIManager.instance.Chat.text.IndexOf(massages.Peek().massage + "\n");
            if(ind != -1)
            {
                UIManager.instance.Chat.text = UIManager.instance.Chat.text.Remove(ind, (massages.Peek().massage + "\n").Length);
            }
            else
            {
                Debug.Log("eror in chat");
            }
            //UIManager.instance.Chat.text = UIManager.instance.Chat.text.Replace(massages.Peek().massage+"\n", "");
            massages.Dequeue();
        }
    }
}






public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject ConnectMenue;
    public GameObject AccountMenue;
    public GameObject LogInMenue;
    public GameObject SignUpMenue;
    public GameObject CantConnectMenue;
    public GameObject guestJoin;
    public GameObject MainMenue;
    public GameObject deadPanle;
    public GameObject stopMenue;
    public GameObject gameList;
    public GameObject ButtonObject;

    public Text Chat;
    public InputField chatInput;

    public AudioSource audioSource;
    public AudioClip press;


    //in game
    public Image circle;
    public float timeToFill = 0;
    public float totalTimeToFill = 0;
    public Text bullets;
    public Text hp;
    public Slider sensitivity;
    public Text leaderBoard;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    private DateTime nextUpdate;
    public void Start()
    {
        nextUpdate = DateTime.Now.AddSeconds(1);
        Massage.massages = new Queue<Massage>();
    }



    void UpdateShotCircle()
    {
        if (timeToFill > 0 && totalTimeToFill > 0)
        {
            circle.fillAmount = timeToFill / totalTimeToFill;
            timeToFill -= Time.deltaTime;
        }
        else
        {
            circle.fillAmount = 0;
        }
    }

    void UpdateChatInput()
    {
        if (chatInput.gameObject.activeSelf)
        {
            string massage = chatInput.text;
            chatInput.text = "";
            chatInput.gameObject.SetActive(false);
            if(massage != "")
                ClientSend.SendChatMassage(massage);
        }
        else
        {
            chatInput.gameObject.SetActive(true);
            chatInput.ActivateInputField();
            chatInput.Select();
        }
    }


    void UpdateEscMenue()
    {
        if (Client.instance.isConnected && stopMenue.activeSelf)
        {
            if (GameManager.players.TryGetValue(Client.instance.myId, out PlayerManager managerp))
            {
                cameraController camcon = managerp.gameObject.GetComponent<cameraController>();
                if (camcon != null)
                {
                    camcon.lookSensitivity = sensitivity.value * 200;
                }
            }

        }
        if (Client.instance.isConnected && Input.GetKeyDown(KeyCode.Escape))
        {
            if (stopMenue.activeSelf)
            {
                stopMenue.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                stopMenue.SetActive(true);
                ClientSend.AskLeaderBoard();
                Cursor.lockState = CursorLockMode.None;
                if (GameManager.players.TryGetValue(Client.instance.myId, out PlayerManager managerp))
                {
                    cameraController camcon = managerp.gameObject.GetComponent<cameraController>();
                    if (camcon != null)
                    {
                        sensitivity.value = camcon.lookSensitivity / 200f;
                    }
                }
            }
        }
    }

    public void Update()
    {

        //chat
        if(nextUpdate < DateTime.Now)
        {
            nextUpdate = DateTime.Now.AddSeconds(1);
            Massage.Clearmessages();
        }

        //shot circle
        UpdateShotCircle();


        //chack chat
        if (Client.instance.isConnected && Input.GetKeyDown(KeyCode.Return))
        {
            UpdateChatInput();
        }

        //esc button
        UpdateEscMenue();
    }


    public void OpenPage(GameObject Page)
    {
        Page.SetActive(true);
    }
    public void ClosePage(GameObject Page)
    {
        Page.SetActive(false);
    }


    public void ReloadGames()
    {
        StartCoroutine(WriteGames());
    }

    public IEnumerator WriteGames()
    {
        var findServers = AccountManager.database.Child("game_servers").GetValueAsync();
        yield return new WaitUntil(predicate: () => findServers.IsCompleted);

        
        foreach(var button in gameList.GetComponentsInChildren<click>())
        {
            UnityEngine.GameObject.Destroy(button.gameObject);
        }

        foreach (var s in findServers.Result.Children)
        {
            string ip = s.Child("ip").Value.ToString();
            string name = s.Child("name").Value.ToString();
            string connected = s.Child("connected").Value.ToString();
            int port = int.Parse(s.Child("port").Value.ToString());
            int map = int.Parse(s.Child("map").Value.ToString());
            GameObject button = Instantiate(ButtonObject, gameList.transform);
            button.GetComponent<click>().port = port;
            button.GetComponent<click>().ip = ip;
            button.GetComponent<click>().Name.text = name;
            button.GetComponent<click>().Connected.text = connected+"/16";
            button.GetComponent<click>().map = map;
        }

    }

    public void CloseAndReset(GameObject Page)
    {
        Page.SetActive(false);
        Text[] resetText = Page.GetComponentsInChildren<Text>();
        foreach(Text t in resetText)
        {
            if(t.tag == "erorr")
                t.text = "";
        }
        InputField[] resetInput = Page.GetComponentsInChildren<InputField>();
        foreach(InputField i in resetInput)
        {
            i.text = "";
        }
    }



    public static void SetLoggedInUi()
    {
        GameObject LogInPage = GameObject.Find("Log In Page");
        GameObject SignUpPage = GameObject.Find("Sign Up Page");
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        if(LogInPage != null)
            LogInPage.SetActive(false);
        if(SignUpPage != null)
            SignUpPage.SetActive(false);
        foreach(GameObject g in allObjects)
        {
            if (g.name == "Main Menu") {
                g.SetActive(true);
                Text[] resetText = g.GetComponentsInChildren<Text>();
                foreach (Text t in resetText)
                {
                    if (t.tag == "UserNamePlaceHolder")
                        t.text = AccountManager.user.DisplayName;
                }
                break;
             }

        }
    }

    public void DesplayMassage(string massage)
    {
        new Massage(massage);
    }

    public void StartBulletTimer(float timeToShot)
    {
        totalTimeToFill = timeToShot;
        timeToFill = timeToShot;
    }

    public void ExitGame()
    {
        stopMenue.SetActive(false);
        deadPanle.SetActive(false);
        Client.instance.Disconnect();
        GameManager.instance.StopGame();
        Client.instance.Start();
        if(AccountManager.user != null)
        {
            MainMenue.SetActive(true);
        }
        else
        {
            AccountMenue.SetActive(true);
        }
        GuestJoin.IsGuest = false;
        SceneManager.UnloadSceneAsync(GameManager.instance.map);
    }
    /*
 * old log in
public static void LogInEror()
{
    GameObject LogInPage = GameObject.Find("Log In Page");
    if (LogInPage != null)
    {
        Text[] resetText = LogInPage.GetComponentsInChildren<Text>();
        foreach (Text t in resetText)
        {
            if (t.tag == "erorr")
                t.text = "Wrong mail or password";
        }
    }
}
public static void SignUpEror(bool userEx, bool MailEx)
{
    GameObject SignUpPage = GameObject.Find("Sign Up Page");
    if (SignUpPage != null)
    {
        Text[] resetText = SignUpPage.GetComponentsInChildren<Text>();
        foreach (Text t in resetText)
        {
            if (t.tag == "erorr")
            {
                if(t.name == "Eror_UserName" && userEx)
                {
                    t.text = "user name already used";
                }
                if(t.name == "Eror_Mail" && MailEx)
                {
                    t.text = "mail already used, try to log in";
                }
            }
        }
    }
}*/

    public void ConnectToServer()
    {
        //ConnectMenue.SetActive(false);
        Client.instance.ConnectToServer();
    }

}
