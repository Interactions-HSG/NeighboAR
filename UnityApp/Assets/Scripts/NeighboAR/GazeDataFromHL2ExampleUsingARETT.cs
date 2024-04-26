using ARETT;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using TMPro;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class GazeDataFromHL2ExampleUsingARETT : MonoBehaviour
{

    public Dictionary<string, long> ObjectAttention = new Dictionary<string, long>();

    bool NewEntry = false;

    string AOI_NameUNPROCESSED;
    long TimestampUNPROCESSED;
    bool firstRun = true;

    long Timestamp_first;
    long Timestamp_last;
    string AOI_Name_first;
    long Search_Begin_Timestamp;

    public bool IsArettWorking = false;
    public bool ActivateArett = false;
    public bool DeactivateArett = false;

    public GameObject MainCamera;
    public GameObject UserSelectedObject;


    public bool UserHasSelectedAnObject;
    public bool UserHasBegunSearch;



    // connect the DataProvider-Prefab from ARETT in the Unity Editor
    public DataProvider DataProvider;
    private ConcurrentQueue<Action> _mainThreadWorkQueue = new ConcurrentQueue<Action>();

    // Start is called before the first frame update
    void Start()
    {
        //StartArettData(); <- no idea wht this is. Can probably be deleted.
    }

    // Update is called once per frame
    void Update()
    {
        //This should always be uncommented in order to actually have and gandle gaze data. []
        HandleDataFromARETT();
    }



    public void TaskOnClick()
    {
        UserHasSelectedAnObject = true;
    }




    /// jfk

    /// <summary>
    /// Handles gaze data from ARETT and allows you to do something with it
    /// </summary>
    /// <param name="gd"></param>
    /// <returns></returns>
    public void HandleDataFromARETT()
    {
        // Some exemplary values from ARETT.
        // for a full list of available data see:
        // https://github.com/AR-Eye-Tracking-Toolkit/ARETT/wiki/Log-Format#gaze-data

        /*
         string t = "received GazeData\n";
         t += "EyeDataRelativeTimestamp:" + gd.EyeDataRelativeTimestamp;
         t += "\nGazeDirection: " + gd.GazeDirection;
         t += "\nGazePointWebcam: " + gd.GazePointWebcam;
         t += "\nGazeHasValue: " + gd.GazeHasValue;
         t += "\nGazePoint: " + gd.GazePoint;
         t += "\nGazePointMonoDisplay: " + gd.GazePointMonoDisplay;
         t += "\nGazePointAOIName: " + gd.GazePointAOIName;
         Debug.Log(t);*/




        var eyesdata = CoreServices.InputSystem.EyeGazeProvider;


        //Testing. Again.
        Debug.Log("Gaze target: " + eyesdata.GazeTarget);
        Debug.Log("Gaze direction: " + eyesdata.GazeDirection);
        
        DateTimeOffset dto = new DateTimeOffset(eyesdata.Timestamp);
        long unixTimeMilliSeconds = dto.ToUnixTimeMilliseconds();
        Debug.Log("Unix MS timestamp: " + unixTimeMilliSeconds);
        



        // DO SOMETHING with the gaze data


        if (true)  //if check added so this part of the code can be disabled easily for testing purposes.
        {
            if (eyesdata.GazeTarget == null) //changing it to NA instead of null, because my code doesn't support null, but it works fine with string NA.
            {
                AOI_NameUNPROCESSED = "NA";
            }
            else if (eyesdata.GazeTarget.layer == 8)
            {
                AOI_NameUNPROCESSED = eyesdata.GazeTarget.name;
            }
            else
            {
                AOI_NameUNPROCESSED = "NA";
            }
            TimestampUNPROCESSED = unixTimeMilliSeconds;
            NewEntry = true;


            //Before further processing, we log the data of the current timestamp to our little CSV file.
            /*
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath + "/eyetracking.csv", true))
            {
                file.WriteLine(unixTimeMilliSeconds.ToString() + "," + eyesdata.IsEyeTrackingEnabledAndValid + "," + eyesdata.GazeOrigin + "," + eyesdata.GazeDirection + "," + AOI_NameUNPROCESSED);
            }
            */


            /////comment here
            if (NewEntry) //new entry retrieved from ARETT.
            {
                NewEntry = false;

                if (firstRun)
                {
                    Timestamp_first = TimestampUNPROCESSED;
                    Timestamp_last = TimestampUNPROCESSED;
                    AOI_Name_first = AOI_NameUNPROCESSED;
                    firstRun = false;


                }
                else
                {
                    if (AOI_Name_first == AOI_NameUNPROCESSED) //if still looking at the same object, just save the new timestamp.
                    {
                        Timestamp_last = TimestampUNPROCESSED;

                        
                        //used to be 2499. PRactically disabled it for now.
                        /*
                        if (((Timestamp_last - Timestamp_first) > 99999) & AOI_Name_first != "NA") //this only runs if we are looking at a hitbox for 5 seconds. Must be hitbox, not NA.
                        {
                            MainCamera.GetComponent<AudioSource>().Play();
                            UserSelectedObject.SetActive(true);
                            UserSelectedObject.GetComponent<TextMeshPro>().text = "User selected object: " + AOI_Name_first + "\nFrom " + Timestamp_first + " to " + Timestamp_last;
                            firstRun = true; //currently, it will update every 5 sec. This means that a user must immediately look away after hearing the first ding, otherwise the data will wil be overwritten by new data. I prefer to keep it this way, but can be patched out.
                            

                            //Write results to CSV file
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath + "/selectedobject.csv", true))
                            {
                                file.WriteLine(unixTimeMilliSeconds.ToString() + "," + eyesdata.IsEyeTrackingEnabledAndValid + "," + eyesdata.GazeOrigin + "," + eyesdata.GazeDirection + "," + AOI_NameUNPROCESSED + "," + "Selected by user at initial timestamp: " + "," + Timestamp_first);
                            }

                            Debug.Log("Object " + AOI_Name_first + " selected by user! \nFirst timestmap: " + Timestamp_first + "\nLast timestamp: " + Timestamp_last); //temporary error to just make it easier to see.
                        }
                        */
                    }
                    else //if looking at new object now
                    {
                        if (ObjectAttention.ContainsKey(AOI_Name_first))
                        {
                            ObjectAttention[AOI_Name_first] += (Timestamp_last - Timestamp_first);
                        }
                        else
                        {
                            if (AOI_Name_first != "NA")
                            {
                                ObjectAttention[AOI_Name_first] = (Timestamp_last - Timestamp_first);
                            }

                        }
                        firstRun = true; //Enable first run again, as this will collect the first and last data.
                    }
                }




                //Calculations for selecting an object.
                if (UserHasBegunSearch)
                {
                    UserHasBegunSearch = false;
                    Search_Begin_Timestamp = TimestampUNPROCESSED;
                }

                if (UserHasSelectedAnObject)
                {
                    UserHasSelectedAnObject = false;
                    long duration = TimestampUNPROCESSED - Search_Begin_Timestamp;
                    float durationdivided = duration / 1000f;


                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath + "/selectedobject.csv", true))
                    {
                        file.WriteLine(unixTimeMilliSeconds.ToString() + "," + "User has selected an object. First timestamp/last timestamp/difference/difference in seconds:" + "," + Search_Begin_Timestamp + "," + TimestampUNPROCESSED + "," + duration + "," + durationdivided); 
                    }
                }





                Debug.Log("ObjectAttention looks like this:");
                foreach (string key in ObjectAttention.Keys)
                {
                    Debug.Log(key + " : " + ObjectAttention[key]);
                }
            }
        }




    }
}
