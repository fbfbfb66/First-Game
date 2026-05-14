using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTool : MonoBehaviour
{
    public readonly Dictionary<string,Coroutine> timeCounterCoroutines = new Dictionary<string, Coroutine>();

    public void StartTimeCounter(float time,string coroutineID)
    {
        if(timeCounterCoroutines.ContainsKey(coroutineID))
        {
            StopCoroutine(timeCounterCoroutines[coroutineID]);
            timeCounterCoroutines.Remove(coroutineID);
        }
        Coroutine coroutine = StartCoroutine(timeCounter(time,coroutineID));
        timeCounterCoroutines.Add(coroutineID, coroutine);
    }
    private IEnumerator timeCounter(float time,string coroutineID)
    {
        yield return new WaitForSeconds(time);
        timeCounterCoroutines.Remove(coroutineID);
    }
}
