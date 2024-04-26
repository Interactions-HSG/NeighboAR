using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;

public class PlayerScript : MonoBehaviour
{


    public bool RebuildLoopVariable = true;
    public HTTPListener HTTPListener;
    //private bool firing2;


    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(RebuildLoop());
        //PointerUtils.SetGazePointerBehavior(PointerBehavior.AlwaysOn); //the little eye gaze pointer cirlce is forced on. Just keeping this for now during testing.
    }

    // Update is called once per frame
    void Update()
    {

    }


    void FixedUpdate()
    {


    }


    /*public IEnumerator RebuildLoop()
    {
        while (RebuildLoopVariable)
        {
            GetComponent<Grapher>().CreateGraph(HTTPListener.AllNodes);
            yield return new WaitForSeconds(5);
        };
        

    }
    */

}