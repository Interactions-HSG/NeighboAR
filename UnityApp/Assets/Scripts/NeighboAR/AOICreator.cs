using ARETT;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


public class AOICreator : MonoBehaviour
{

    public LayerMask IgnoreMe;

    public Camera HL2Camera;
    public GameObject BoundingBoxPrefab;
    public GameObject ObjectPointPrefab;
    public HTTPListener HTTPListener;
    public Grapher Grapher;

    //used only in the function
    public GameObject ObjectPointTL;
    public GameObject ObjectPointBR;
    public GameObject ObjectPointBL;
    float xLength;
    float yLength;

    public bool newYoloResult = false;
    public bool loopAllRays;
    public bool processing = false;

    public (float, float) coordsCenter;
    public (float, float) coordsBR;
    public (float, float) coordsTL;
    public string keyName;
    public string framestart;

    public string nodeIdentifier;

    public int maxPing = 600;


    Dictionary<string, int> hitboxNumber = new Dictionary<string, int>();



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (newYoloResult)
        {
            //processing = true;
            CreateAOI(keyName, coordsBR, coordsTL, coordsCenter);
            newYoloResult = false;
            processing = false;
        }
    }

    public void CreateAOI(string objectName, (float, float) coordsBR, (float, float) coordsTL, (float, float) coordsCenter)
    {
        //loopAllRays = true;
        //while (loopAllRays)
        for (int repeats = 0; repeats < 1; repeats++) //Only runs once, basically. I have this because I used to work with a system which allowed for several repeats, in case there is a miss. That legacy system has been disabled and repurposed.
        {
            
            Debug.Log("======================Calculating center 0 and creating bounding box.======================");
            Debug.Log("Flipping coords.");
            var flippedcoordsCenter = new Vector2(coordsCenter.Item1, HL2Camera.pixelHeight -  coordsCenter.Item2);
            Debug.Log("Saving ray.");
            Ray rayCenter = HL2Camera.ScreenPointToRay(flippedcoordsCenter);

            //instantiate bounding box
            Debug.Log("Raycast hit check");
            if (Physics.Raycast(rayCenter, out RaycastHit hitCenter, 2.25f, ~IgnoreMe))
            {
                Debug.Log("newPositionInCamLocalSpace being saved.");
                Vector3 newPositionInCamLocalSpaceCenter = HL2Camera.transform.InverseTransformPoint(hitCenter.point);



                //Checking how long it took to raycast. Might have to comment out due to annoying error that keeps hpapening.
                /*
                Debug.Log(framestart);
                string framestart_processing = framestart.Replace(".", "");
                Debug.Log(framestart_processing);
                framestart = framestart_processing.Remove(13);
                Debug.Log(framestart);

                long framestartMS = long.Parse(framestart);
                DateTimeOffset now = DateTimeOffset.UtcNow;
                long raystartMS = now.ToUnixTimeMilliseconds();
                Debug.Log("Raycasting took: " + (raystartMS - framestartMS) + " ms.");
               

                if (raystartMS - framestartMS > maxPing)
                {
                    Debug.Log("Raycasting took too long. Discarding...");
                    break;
                }
                Debug.Log("Raycasting was fast enough. Continuing.");
               */

                //if using direct hit
                Debug.Log("Instantiating");
                GameObject BoundingBox = Instantiate(BoundingBoxPrefab);
                Debug.DrawLine(HL2Camera.transform.position, hitCenter.point, Color.magenta, 99999999f, true);
                BoundingBox.transform.position = hitCenter.point;

                //if using camera-relative offset hit
                /*
                Debug.Log("Instantiating");
                GameObject BoundingBox = Instantiate(BoundingBoxPrefab);
                var orbitalSolver = BoundingBox.GetComponent<Orbital>();
                Debug.DrawLine(HL2Camera.transform.position, hitCenter.point, Color.magenta, 99999999f, true);
                orbitalSolver.LocalOffset = newPositionInCamLocalSpaceCenter;
                orbitalSolver.enabled = true;
                */

                Debug.Log("Naming.");
                BoundingBox.name = objectName;


                
                //We create 12 hitboxes with 12 different raycasts using 12 different coordinates from Yolo. 
                if (hitboxNumber.ContainsKey(objectName))
                {
                    hitboxNumber[objectName] += 1;


                    //check distance of this and previous hitbox. delete if distance too big. decrease hitboxNumber[objectName] by 2.
                    if (hitboxNumber[objectName] >= 2)
                    {
                        var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == objectName); //will return an IEnumerable of all gameobjects with this specific name.
                        var objects_list = objects.Cast<GameObject>().ToList(); //casting to list in order to use indexing

                        //GameObject previous_BoundingBox = objects_list[hitboxNumber[objectName] - 2]; //changed to -2, was -1.
                        GameObject previous_BoundingBox = objects_list[1]; //[]


                        float distance = Vector3.Distance(BoundingBox.transform.position, previous_BoundingBox.transform.position);
                        Debug.Log("The distance was: " + distance);

                        if (distance > 0.2f)
                        {
                            Debug.Log("Distance between last two bounding boxes was too large. Discarding both bounding boxes.");
                            Destroy(previous_BoundingBox);
                            Destroy(BoundingBox);
                            hitboxNumber[objectName] -= 2;

                        }
                    }


                    if (hitboxNumber[objectName] >= 12)  //If we've got the twelve, we run this code below to get the average X, Y and Z coordinates of the 12 hitboxes.
                    {

                        var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == objectName); //will return a list of all gameobjects with this specific name.
                        float totalX = 0f;
                        float totalY = 0f;
                        float totalZ = 0f;
                        foreach (GameObject hitbox in objects)
                        {
                            totalX += hitbox.transform.position.x;
                            totalY += hitbox.transform.position.y;
                            totalZ += hitbox.transform.position.z;
                            Destroy(hitbox); //we destroy each hitbox, as we are done processing it.
                        }
                        float averageX = totalX / 12;
                        float averageY = totalY / 12;
                        float averageZ = totalZ / 12;


                        //Finally, using the x, y and z average coords, we create one hitbox which is representative of the center.
                        BoundingBox = Instantiate(BoundingBoxPrefab);
                        BoundingBox.transform.position = new Vector3(averageX, averageY, averageZ);
                        BoundingBox.transform.localScale = new Vector3(0.2f, 0.3f, 0.3f);
                        BoundingBox.name = objectName;
                        BoundingBox.transform.LookAt(HL2Camera.transform.position);
                        //rotating to remove upward tilt
                        BoundingBox.transform.rotation = Quaternion.Euler(0, BoundingBox.transform.eulerAngles.y, BoundingBox.transform.eulerAngles.z);
                        BoundingBox.layer = 8; //change it to layer 8, which is EyeTracking. []Commented out for testing.


                        //Finally, the keyReplacement from a few lines before is used as the key for the new dictionary entry. A touple of the x and y coords is used as the value.
                        //used to be keyName. Changed to objectName. Should work better now, but should double check.
                        //HTTPListener.AllNodes[nodeIdentifier].Add(objectName, (coordsCenter.Item1, coordsCenter.Item2));
                        HTTPListener.Unfiltered_Objects.Add(BoundingBox);
                        //HTTPListener.Unfiltered_Objects_String_Name.Add(BoundingBox.name); []
                        Grapher.FilterObjects(HTTPListener.Unfiltered_Objects);
                        Grapher.CreateGraph(HTTPListener.AllNodes);
                    }
                }
                else
                {
                    hitboxNumber.Add(objectName, 1);
                }



                Debug.Log("Dictionary entry updated!");

                Debug.Log("Bounding box created. Function should close now.");

                break;






            }
            else
            {
                Debug.Log("continue " + repeats);
                continue;
            }






        }




    }
 
}


