using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class SignUp : MonoBehaviour
{
    public InputField userNameField;
    public InputField mailField;
    public InputField passwordField;
    public InputField ConfirmPasswordField;
    public Text userNameEror;
    public Text mailEror;
    public Text passwordEror;
    public Text confirmPasswordEror;

    public void TrySignUp()
    {
        string userName = userNameField.text;
        string mail = mailField.text;
        string password = passwordField.text;
        string confirmPassword = ConfirmPasswordField.text;
        bool ok = true;
        //user name chack
        //todo: chack if username exists in database
        if (userName.Length < 4)
        {
            userNameEror.text = "user name needs to be at least 4 long";
            ok = false;
        }
        else
            userNameEror.text = "";

        //mail chack
        if (mail.LastIndexOf('@') == -1)
        {
            mailEror.text = "invalid mail";
            ok = false;
        }
        else
        {

            mailEror.text = "";
        }

        //password Chack
        if (password.Length < 6)
        {
            passwordEror.text = "password has to be at least 6 long";
            ok = false;
        }
        else if (password.ToLower() == password)
        {
            passwordEror.text = "password need to include an uppercase letter";
            ok = false;
        }
        else
            passwordEror.text = "";

        //confirm Password chack
        if (!(password == confirmPassword))
        {
            confirmPasswordEror.text = "confirm Password needs to be the same as the Password";
            ok = false;
        }
        else
            confirmPasswordEror.text = "";


        if (ok)
        {
            //ClientSend.SendSignUp(userName, mail, password);
            StartCoroutine(SignUpFireBase(mail, password, userName));
        }
    }

    public IEnumerator SignUpFireBase(string mail, string password, string userName)
    {

        var findUserName = AccountManager.database.Child("user_data").GetValueAsync();
        yield return new WaitUntil(predicate: () => findUserName.IsCompleted);

        bool nameExists = false;

        foreach(var u in findUserName.Result.Children)
        {
            if (u.Child("UserName").Exists)
                if (u.Child("UserName").Value.ToString() == userName)
                    nameExists = true;
        }

        if (nameExists)
        {
            userNameEror.text = "username is used";
        }
        else
        {


            var SignUp = AccountManager.auth.CreateUserWithEmailAndPasswordAsync(mail, password);
            yield return new WaitUntil(predicate: () => SignUp.IsCompleted);

            if (SignUp.Exception != null)
            {
                if ((AuthError)((FirebaseException)SignUp.Exception.GetBaseException()).ErrorCode == AuthError.EmailAlreadyInUse)
                {
                    mailEror.text = "mail is already used, try to log in";
                }
                if ((AuthError)((FirebaseException)SignUp.Exception.GetBaseException()).ErrorCode == AuthError.InvalidEmail || (AuthError)((FirebaseException)SignUp.Exception.GetBaseException()).ErrorCode == AuthError.MissingEmail)
                {
                    mailEror.text = "Invalid mail";
                }
                if ((AuthError)((FirebaseException)SignUp.Exception.GetBaseException()).ErrorCode == AuthError.MissingPassword || (AuthError)((FirebaseException)SignUp.Exception.GetBaseException()).ErrorCode == AuthError.WeakPassword)
                {
                    passwordEror.text = "weak password";
                }
            }
            else
            {


                AccountManager.user = SignUp.Result;

                var addusername = AccountManager.user.UpdateUserProfileAsync(new UserProfile { DisplayName = userName });
                var dbAddUserName = AccountManager.database.Child("user_data").Child(AccountManager.user.UserId).Child("UserName").SetValueAsync(userName);

                yield return new WaitUntil(predicate: () => dbAddUserName.IsCompleted && addusername.IsCompleted);

                if (dbAddUserName.Exception == null && addusername.Exception == null)
                {
                    UIManager.SetLoggedInUi();
                }

            }

        }

        




    }

}
