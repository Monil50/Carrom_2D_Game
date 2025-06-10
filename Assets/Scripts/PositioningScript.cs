using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PositioningScript : MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    [SerializeField]
    private GameObject sliderHider, helperCollider;

    [SerializeField]
    Transform StrikerBg;

    bool StrikerAim, playSound = true;

    RaycastHit2D hit;

    Rigidbody2D rb;

    [SerializeField]
    Transform circles;

    private void OnEnable()
    {
        slider.value = 0;
        helperCollider.SetActive(false);
        sliderHider.SetActive(false);
        StrikerBg.localScale = Vector3.zero;
        transform.position = new Vector3(0, -1.8f, 0);
    }

    private void Start()
    {
        slider.onValueChanged.AddListener(StrikerReposition);
        rb = GetComponent<Rigidbody2D>();
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

    void GiveTurn()
    {
        PlayManager.instance.SwitchTurn(false);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(Camera.main.transform.position,hit.point);
        
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);

            if (hit.collider)
            {
                //Debug.Log($"Raycast with {hit.collider.name}");
                if (hit.collider.name == "PlayerStriker")
                { 
                    StrikerAim = true;
                    helperCollider.SetActive(true);
                }
                if (StrikerAim)
                {
                    StrikerBg.LookAt(hit.point);
                }

                float ScaleValue = Vector2.Distance(transform.position, hit.point);
                ScaleValue = ScaleValue > 1.5f ? 1.5f : ScaleValue;
                StrikerBg.localScale = new Vector3(ScaleValue, ScaleValue, ScaleValue);

                //Debug.Log(hit.transform.name);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            rb.AddForce(new Vector3(circles.position.x - transform.position.x, circles.position.y - transform.position.y, 0) * 2000);
            sliderHider.SetActive(true);
            StrikerBg.localScale = Vector3.zero;
            if (rb.velocity.magnitude < 0.2f && StrikerAim)
            {
                helperCollider.SetActive(false);
                
                Invoke(nameof(GiveTurn), 3);
            }
            StrikerAim = false;

            
        }

    }

    public void StrikerReposition(float value)
    {
        transform.position = new Vector3(value, -1.8f, 0);
    }
}
