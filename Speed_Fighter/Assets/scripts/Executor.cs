using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Executor : MonoBehaviour
{


    private static readonly List<Action> ActionToExecute = new List<Action>();
    private static readonly List<Action> TempActionToExecute = new List<Action>();
    private static bool hasAction = false;


    private void Update()
    {
        UpdateAction();
    }

    public static void ExecuteAction(Action _action)
    {
        if (_action == null)
        {
            return;
        }

        lock (ActionToExecute)
        {
            ActionToExecute.Add(_action);
            hasAction = true;
        }
    }

    public static void UpdateAction()
    {
        if (hasAction)
        {
            TempActionToExecute.Clear();
            lock (ActionToExecute)
            {
                TempActionToExecute.AddRange(ActionToExecute);
                ActionToExecute.Clear();
                hasAction = false;
            }

            for (int i = 0; i < TempActionToExecute.Count; i++)
            {
                TempActionToExecute[i]();
            }
        }
    }
}
