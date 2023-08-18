using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class LogIn : MonoBehaviour
{

    public InputField mailField;
    public InputField passwordField;
    public Text ErorText;
    public void TryLogIn()
    {
        string mail = mailField.text;
        string password = passwordField.text;
        if (mail == "")
            ErorText.text = "Enter Mail";
        else if (password == "")
            ErorText.text = "Enter Password";
        else
        {
            ErorText.text = "";
            //ClientSend.SendLogIn(mail, password);
            StartCoroutine(SignUpFireBase(mail, password));
        }

    }


    public IEnumerator SignUpFireBase(string mail, string password)
    {



        var LogIn = AccountManager.auth.SignInWithEmailAndPasswordAsync(mail, password);
        yield return new WaitUntil(predicate: () => LogIn.IsCompleted);



        if (LogIn.Exception == null)
        {
            ErorText.text = "";
            AccountManager.user = LogIn.Result;
            UIManager.SetLoggedInUi();
        }
        else
        {
            ErorText.text = "username or password is incorrect";
        }


    }
}