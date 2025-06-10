using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyPucks : MonoBehaviour
{

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Debug.Log($"{gameObject.name} triggered by {collision.gameObject.name}");
        float vel = collision.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
        //Debug.Log($"{collision.gameObject.name} has magnitude :  {vel}");


        if ((collision.gameObject.name != "PlayerStriker" && collision.gameObject.name != "CompStriker")  && vel < 0.2f)
        {
            int value = collision.gameObject.GetComponent<Puck_Value>().Value;
            
            PlayManager.instance.pucks = PlayManager.instance.pucks.Where(val => val.name != collision.gameObject.name).ToArray();

            GetComponent<AudioSource>().Play();

            Destroy(collision.gameObject);
            PlayManager.instance.scoreManager.GetComponent<ScoreManager>().AddScore(value);
        }
    }

}
