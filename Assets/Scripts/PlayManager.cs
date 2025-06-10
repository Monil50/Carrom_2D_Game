using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Turn currentTurn = Turn.User;
    public GameObject PlayerStriker, CompStriker, scoreManager;

    [SerializeField]
    private GameObject pucksGO;

    public Rigidbody2D[] pucks;

    public static PlayManager instance;


    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
        pucks = pucksGO.GetComponentsInChildren<Rigidbody2D>();
    }

    //public bool switching = true;

    private void Start()
    {
        SwitchTurn(true);
    }


    public void SwitchTurn(bool flag)
    {
        //Debug.Log("Switched");
        bool steadyState = true;
        float total_velocity = 0;
        while (steadyState)
        {
            foreach(var p in pucks)
            {
                total_velocity += p.velocity.magnitude;
            }
            if (total_velocity < 0.2f) 
                steadyState = false;
        }

        PlayerStriker.SetActive(flag);
        CompStriker.SetActive(!flag);
        if (currentTurn == Turn.User) currentTurn = Turn.Comp;
        else currentTurn = Turn.User;
    }


}
