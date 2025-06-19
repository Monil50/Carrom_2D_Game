using System.Collections;
using UnityEngine;

public class DelayAction : MonoBehaviour
{
    // Example: delay in seconds
    public float delayTime = 2f;

    void Start()
    {
        // Call your custom function after delayTime seconds
        StartCoroutine(DelayCoroutine());
    }

    IEnumerator DelayCoroutine()
    {
        yield return new WaitForSeconds(delayTime);

        // Your delayed action here
        Debug.Log("Action performed after delay!");

        // Example: enable an object
        // someGameObject.SetActive(true);

        // Or call another method
        // DoSomething();
    }

    // You can also use this to trigger delay from other scripts
    public void StartDelay(float time)
    {
        StartCoroutine(DelayWithParameter(time));
    }

    IEnumerator DelayWithParameter(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("Delayed action from public method!");
    }
}
