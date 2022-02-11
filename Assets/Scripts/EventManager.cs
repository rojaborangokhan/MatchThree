using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    public static Action<int> OnFiveMatch;

    public static void TriggerFiveMatch(int num)
    {
        OnFiveMatch?.Invoke(num);
    }
}
