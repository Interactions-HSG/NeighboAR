// Unity HTTP listener
// Largely based on https://gist.github.com/amimaro/10e879ccb54b2cacae4b81abea455b10
// No license specified!

using Microsoft.MixedReality.Toolkit.Utilities.GameObjectManagement;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
//using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
//using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
//using UnityEditor.VersionControl;
//using UnityEditorInternal;
using UnityEngine;
//using static UnityEditor.PlayerSettings;

public class HTTPListener : MonoBehaviour
{

	private HttpListener listener;
	private Thread listenerThread;
	private string keyReplacement;
	private float xCoord;
	private float yCoord;

	private string nodeIdentifier = null;

    public Dictionary<string, (float, float)> Node1Dict = new Dictionary<string, (float, float)>();
    public Dictionary<string, (float, float)> Node2Dict = new Dictionary<string, (float, float)>();
    public Dictionary<string, (float, float)> Node3Dict = new Dictionary<string, (float, float)>();
    public Dictionary<string, (float, float)> Node4Dict = new Dictionary<string, (float, float)>();

    public Dictionary<string, Dictionary<string, (float, float)>> AllNodes = new Dictionary<string, Dictionary<string, (float, float)>>(); //contents filled later below
    public List<GameObject> Unfiltered_Objects = new List<GameObject>();


    private NameValueCollection weirdDict;
	public Dictionary<string, (float, float)> ThingTable = new Dictionary<string, (float, float)>();


    public AOICreator AOICreator;

    public PlayerScript PlayerScript;

    public GameObject KnowledgeGraph;

    public NameValueCollection message;

    public SetGoal SetGoal;

    public Grapher Grapher;

    public GazeDataFromHL2ExampleUsingARETT GazeDataFromHL2ExampleUsingARETT;

    public Timers Timers;

    public ResetGraph ResetGraph;

    void Start()
	{
		//contents filled
		AllNodes.Add("x", Node1Dict);
		AllNodes.Add("y", Node2Dict);
        AllNodes.Add("z", Node3Dict);
        AllNodes.Add("w", Node4Dict);



        Debug.Log("start HTTPListener script");

		listener = new HttpListener();

        // Set the HoloLenses IP address.
        // Change the port if needed. 
        //listener.Prefixes.Add("http://localhost:5050/");

        //new automatic IP system:
        var ipAddress = GetIP4Address();
        listener.Prefixes.Add($"http://{ipAddress}:5050/");

        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		listener.Start();

		listenerThread = new Thread(StartListener);
		listenerThread.Start();
		Debug.Log("Server Started");



	}

	private void StartListener()
	{
		while (true)
		{
            var result = listener.BeginGetContext(ListenerCallback, listener);
            result.AsyncWaitHandle.WaitOne();
		}
	}

    private static string GetIP4Address()
    {
        string IP4Address = String.Empty;
        foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (IPA.AddressFamily == AddressFamily.InterNetwork)
            {
                IP4Address = IPA.ToString();
                break;
            }
        }
        return IP4Address;
    }

    private void ListenerCallback(IAsyncResult result)
	{
		var context = listener.EndGetContext(result);

		Debug.Log("=====================Request received=============================================");

        // from a GET-request: example.com/?key=value&...
        if (context.Request.QueryString.AllKeys.Length > 0)
        {
            Debug.Log("Key length passed.");

            //Check if it's an administrator's request. Administrator requests always only have one key.
            message = context.Request.QueryString;
            if (message.AllKeys.Length < 2)
            {
                if (message[0] == "reset")
                {
                    Debug.Log("Reset command detected");
                    Grapher.ResetVariable = true;


                }

                else if (message[0] == "ARETT")
                {
                    if (GazeDataFromHL2ExampleUsingARETT.IsArettWorking)
                    {
                        GazeDataFromHL2ExampleUsingARETT.DeactivateArett = true;
                    }
                    else
                    {
                        GazeDataFromHL2ExampleUsingARETT.ActivateArett = true;
                    }
                }

                else if (message[0] == "toggle_graph")
                {
                    Grapher.ToggleGraphVariable = true;
                }

                else if (message[0] == "reposition_graph")
                {
                    ResetGraph.RepositionGraphToggle = true;
                }

                else if (message[0] == "timer_think")
                {
                    Timers.ToggleTimerThink = true;
                }

                else if (message[0] == "timer_think_graph")
                {
                    Timers.ToggleTimerThinkGraph = true;
                }

                else if (message[0] == "timer_cabinet")
                {
                    Timers.ToggleTimerCabinet = true;
                }

                else if (AllNodes["x"].ContainsKey(message[0]) | AllNodes["y"].ContainsKey(message[0]) | AllNodes["z"].ContainsKey(message[0]) | AllNodes["w"].ContainsKey(message[0]))
                {

                    Timers.ToggleTimerThinkGraph = true;
                    SetGoal.NewGoalVariable = true;


                }

                else
                {
                    try
                    {
                        AOICreator.maxPing = int.Parse(message[0]);
                        Debug.Log("Max ping changed to: " + AOICreator.maxPing);
                    }
                    catch { Debug.Log("Unknown command detected or object name incorrectly inserted."); }

                }
            }



            //If not an administrator's request, then continue with YOLO functionality.
            else if (message[0] == "1") //Yolo also notifies when an object is NOT there anymore. We don't want this info, obviously, so we filter only when object is there i.e. equal to 1.
            {
                Debug.Log("Going into YOLO.");
                weirdDict = context.Request.QueryString; //Just using weirdDict to denote this object instead of having to retype everything.
                keyReplacement = weirdDict.GetKey(0);//I want to edit this key later, but changing the value in the weirdDict is strange. So I add it to this var and change here.



                int index = keyReplacement.LastIndexOf(" ");
                if (index >= 0)
                    keyReplacement = keyReplacement.Substring(0, index); // or index + 1 to keep slash




                if (true)
                {
                    if (true)
                    {
                        if (!(AllNodes["x"].ContainsKey(keyReplacement) | AllNodes["y"].ContainsKey(keyReplacement) | AllNodes["z"].ContainsKey(keyReplacement) | AllNodes["w"].ContainsKey(keyReplacement)))
                        //if (!(Unfiltered_Objects_String_Name.Contains(keyReplacement)))
                        {
                            //The very center of the bounding box.
                            xCoord = ( float.Parse(weirdDict["coordTLx"]) + float.Parse(weirdDict["coordBRx"]) ) / 2;
                            yCoord = ( float.Parse(weirdDict["coordTLy"]) + float.Parse(weirdDict["coordBRy"]) + 150 ) / 2; //was +200. Just removed that now. alternativel decrease it by about 100 pixels or so.





                            Debug.Log("Calling AOICreator function.");
                            //Calling the function does not work for whatever reason. Instead, we'll set this newYoloResult to true, which will trigger the function to start.
                            //Before we set newYoloResult to true, we'll parse over relevant data to the script, which will then run the function itself.
                            AOICreator.coordsCenter = (xCoord, yCoord);
                            AOICreator.coordsBR = (float.Parse(weirdDict["coordBRx"]), float.Parse(weirdDict["coordBRy"]));
                            AOICreator.coordsTL = (float.Parse(weirdDict["coordTLx"]), float.Parse(weirdDict["coordTLy"]));
                            AOICreator.keyName = keyReplacement;
                            AOICreator.nodeIdentifier = nodeIdentifier;
                            AOICreator.framestart = weirdDict["framestart"];
                            AOICreator.processing = true;
                            AOICreator.newYoloResult = true;
                            Debug.Log("AOICreator function successfully called.");








                        }


                    }
                }

            }

        }



       /* 
        while (AOICreator.processing)
        {
            //Debug.Log("Waiting for AOI creator to finish its process.");
        }
        */

        Debug.Log("AOI creator finished processing. Sending back a response over HTTPListener.");
        context.Response.Close();
	}



}