using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerAim : MonoBehaviour
{
    //angle
    //power

    [SerializeField] Transform StrikerBg;
    Rigidbody2D rb;

    [SerializeField]
    private float minX = -10f, maxX= 10f, minY = -10f, maxY = 10f;

    [SerializeField]
    Transform circles;

    bool playSound = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    { 
        transform.position = new Vector3(0, 1.8f, 0);
        GamePlay();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log($"Collision detected with {collision.gameObject.name}");
        if (collision.gameObject.GetComponent<Puck_Value>() != null && playSound && collision.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude > 0.2f)
        {
            playSound = false;
            Invoke("SoundActive", 2);
            //Debug.Log("Play Sound");
            gameObject.GetComponent<AudioSource>().Play();
        }
    }
    private void SoundActive()
    {
        playSound = true;
    }

    public void GamePlay()
    {
        //position
        Invoke("SetPosition", 2);


        //angle
        //StartCoroutine(WasteTime(3));

        //power
       


        //Invoke("CallSwitch", 3);

    }


    public void SetPosition()
    {
        float xPos = Random.Range(-1.4f, 1.4f);
        transform.position = new Vector3(xPos, transform.position.y, 0);
        //Debug.Log("Position of striker is: " + xPos);
        //Debug.Log("Position set");

        Invoke("SetAngle", 2);
    }


    public void SetAngle()
    {
        Vector2 hitpoint = new Vector2(Random.Range(minX,maxX), Random.Range(minY,maxY));
        Debug.Log("The hitpoint is "+hitpoint);
        Debug.DrawLine(transform.position, hitpoint);


        float ScaleValue = Vector2.Distance(transform.position, hitpoint);
        ScaleValue = ScaleValue > 1.5f ? 1.5f : ScaleValue;
        StrikerBg.localScale = new Vector3(ScaleValue, ScaleValue, ScaleValue);
        StrikerBg.LookAt(hitpoint);
        //Debug.Log("Angle set");

        Invoke("ApplyForce", 2);

    }

    public void ApplyForce()
    {
        rb.AddForce(new Vector3(circles.position.x - transform.position.x, circles.position.y - transform.position.y, 0) * 2000);
        //Debug.Log("Force applied");

        StrikerBg.localScale = Vector3.zero;
        Invoke("GiveTurn", 3);
    }
    void GiveTurn()
    {
        PlayManager.instance.SwitchTurn(true);
    }

    //void CallSwitch()
    //{
    //    playActive = true;
    //    PlayManager.instance.SwitchTurn();
    //}


}
