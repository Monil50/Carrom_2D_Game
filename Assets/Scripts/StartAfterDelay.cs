using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartAfterDelay : MonoBehaviour
{
    public float DelayTime;
    public UnityEvent DelayEvent; 
    
    void Start()
    {
        StartCoroutine(StartDelay());
    }

   
    void Update()
    {
        
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(DelayTime);
        DelayEvent.Invoke();
    }
}
