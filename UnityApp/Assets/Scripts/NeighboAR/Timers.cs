using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class Timers : MonoBehaviour
{

    public GameObject MainCamera;
    public AudioClip BellClip;
    public AudioClip SelectClip;
    public GameObject KnowledgeGraph;
    public GazeDataFromHL2ExampleUsingARETT GazeDataFromHL2ExampleUsingARETT;

    public bool ToggleTimerThink;
    Timer t_think = new Timer();

    public bool ToggleTimerThinkGraph;
    Timer t_think_graph = new Timer();

    public bool ToggleTimerCabinet;
    Timer t_cabinet = new Timer();

    bool kg_manipulator_off;
    public bool XyzBellCheck;


    private bool bell;


    // Start is called before the first frame update
    void Start()
    {
        t_think.Interval = 5000; // In milliseconds
        t_think.AutoReset = false; // Stops it from repeating
        t_think.Elapsed += new ElapsedEventHandler(t_think_elapsed);

        t_think_graph.Interval = 5000; // In milliseconds
        t_think_graph.AutoReset = false; // Stops it from repeating
        t_think_graph.Elapsed += new ElapsedEventHandler(t_think_elapsed_graph);

        t_cabinet.Interval = 30000; // In milliseconds
        t_cabinet.AutoReset = false; // Stops it from repeating
        t_cabinet.Elapsed += new ElapsedEventHandler(t_cabinet_elapsed);




    }


    void t_think_elapsed(object sender, ElapsedEventArgs e)
    {
        bell = true;
        GazeDataFromHL2ExampleUsingARETT.UserHasBegunSearch = true;
    }

    void t_think_elapsed_graph(object sender, ElapsedEventArgs e)
    {
        bell = true;
        GazeDataFromHL2ExampleUsingARETT.UserHasBegunSearch = true;
        //kg_manipulator_off = true; keeping this off for now so the graph is NOT turned off after timer.
    }

    void t_cabinet_elapsed(object sender, ElapsedEventArgs e)
    {
        bell = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (ToggleTimerThink)
        {
            ToggleTimerThink = false;
            t_think.Start();
        }

        if (ToggleTimerThinkGraph)
        {
            ToggleTimerThinkGraph = false;
            KnowledgeGraph.SetActive(true);
            t_think_graph.Start();
        }

        if (ToggleTimerCabinet)
        {
            ToggleTimerCabinet = false;
            t_cabinet.Start();
        }


        if (bell)
        {
            bell = false;
            MainCamera.GetComponent<AudioSource>().Play();
        }

        if (XyzBellCheck)
        {
            XyzBellCheck = false;
            //MainCamera.GetComponent<AudioSource>().clip = SelectClip;
            //MainCamera.GetComponent<AudioSource>().Play();
            //MainCamera.GetComponent<AudioSource>().clip = BellClip;
        }



        if (kg_manipulator_off)
        {
            kg_manipulator_off = false;
            KnowledgeGraph.SetActive(false);
            
        }


        /*
        if (kg_manipulator_on)
        {
            kg_manipulator_on = false;
            KnowledgeGraph.SetActive(true);
            
        }
        */
    }




}

