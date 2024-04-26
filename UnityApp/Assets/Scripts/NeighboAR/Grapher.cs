//using System;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SocialPlatforms;
//using UnityEngine.UI;
//using System.Linq;
using TMPro;
using UnityEngine.UIElements;
using Microsoft.MixedReality.Toolkit.UI;
using System.Linq;
//using UnityEditor.Experimental.GraphView;

public class Grapher : MonoBehaviour
{

    public GameObject TextBox;
    public GameObject NodeBox;
    public GameObject KnowledgeGraph;
    public GameObject KnowledgeGraphWhole;
    public HTTPListener HTTPListener;
    public List<Transform> nodeList = new List<Transform>();

    private int nodeCounter;
    private int slotCounter;

    public bool ResetVariable;
    public bool ToggleGraphVariable;

    void Update()
    {
        if (ResetVariable)
        {
            ResetVariable = false;
            CreateGraph(HTTPListener.AllNodes);
        }

        if (ToggleGraphVariable)
        {
            if (KnowledgeGraphWhole.activeSelf)
            {
                KnowledgeGraphWhole.SetActive(false);
            }
            else
            {
                KnowledgeGraphWhole.SetActive(true);
            }

            ToggleGraphVariable = false;
        }
    }

    public void CreateGraph(Dictionary<string, Dictionary<string, (float, float)>> dic)
    {

        foreach (Transform node in KnowledgeGraph.transform)
        {
            nodeList.Add(node);

            try
            {
                GameObject.Destroy(node.GetChild(8).gameObject); //This is added to prevent nodes having duplicate NodeBoxes assigned to them.
            }
            catch { }

            foreach (Transform slot in node)
            {
                foreach (Transform child in slot)
                {
                    GameObject.Destroy(child.gameObject); //clears a text box from every slot.
                }

            }
        }


        nodeCounter = 0;
        foreach (KeyValuePair<string, Dictionary<string, (float, float)>> node in dic)
        {


            GameObject NewNode = Instantiate(NodeBox, nodeList[nodeCounter], false);
            //GameObject NewBox = Instantiate(TextBox, slotList[slotCounter], false); /////Leftover code, used below for slots, albeit a bit modified to use GetChild.]
            NewNode.name = "TextBox_" + node.Key.ToString();
            //NewNode.GetComponent<TextMeshPro>().text = node.Key.ToString();
            NewNode.GetComponent<TextMeshPro>().text = ""; //current implementation makes textbox of node just an empty string. Remove this and uncomment line above to bring back visible node titles.

            //GameObject Plane = NewNode.transform.GetChild(0).gameObject;
            //Plane.GetComponent<MeshRenderer>().material = "";


            slotCounter = 0;
            foreach (KeyValuePair<string, (float, float)> detectedThing in node.Value)
            {

                GameObject NewBox = Instantiate(TextBox, NewNode.transform.parent.GetChild(slotCounter), false); //have to use parent to select Node 1, not the NodeBox text.
                NewBox.name = "TextBox_" + detectedThing.Key.ToString(); //Give it a unique name with the prefix Textbox, so it's easier to find and edit later on.
                NewBox.GetComponent<TextMeshPro>().text = detectedThing.Key.ToString();

                var foodstuff_material = Resources.Load<Material>("Materials/Foodstuff/Materials/" + detectedThing.Key.ToString());
                GameObject Plane = NewBox.transform.GetChild(0).gameObject;
                Plane.GetComponent<MeshRenderer>().material = foodstuff_material;



                slotCounter += 1;
            }
            nodeCounter += 1;
        }

    }


    public void FilterObjects(List<GameObject> unfiltered_objects)
    {

        //We create a copy of unfiltered_objects, as we are going to be editing that copy. 
        List<GameObject> unfiltered_objects_copy = new List<GameObject>();
        foreach (GameObject Hitbox in unfiltered_objects)
        {
            unfiltered_objects_copy.Add(Hitbox);
        }

        Dictionary<int, List<GameObject>> filtered_objects_in_clusters = new Dictionary<int, List<GameObject>>(); //This is where objects are stored once they are filtered into the 4 clusters/nodes.

        int numbered_cluster = -1; //We need to track which cluster we are iterating into. Start with -1 because we immediately increment by 1. Thus, we start with numbered_cluster 0.
        while (unfiltered_objects_copy.Count > 0)
        {
            numbered_cluster += 1;

            List<GameObject> cluster_of_objects = new List<GameObject>(); //This is ONE cluster/node.


            //the first object available is taken as the main comparison and is immediately removed from the unfiltered_objects_copy.
            cluster_of_objects.Add(unfiltered_objects_copy[0]); 
            unfiltered_objects_copy.RemoveAt(0);


            //Create a list of all hitboxes which are close enough to the previous iteration.
            foreach (GameObject HitBox in unfiltered_objects_copy)
            {

                float distance_between_current_and_previous_hitbox = Mathf.Abs(HitBox.transform.position.x - cluster_of_objects.First().transform.position.x); //[] Replaced Last() with First()
                //this if check means that if we are in the 4th iteration (numbered cluster 3), we add all the remaining objects together without checking distance.
                /* []
                if (numbered_cluster == 3)
                {
                    cluster_of_objects.Add(HitBox);
                }
                */


                if (distance_between_current_and_previous_hitbox < 0.3) //must be elseif [] if command above is uncommented
                {
                    cluster_of_objects.Add(HitBox);
                }



            }

            //Every object which was added to the ONE cluster should now be removed from the unfiltered_objects_copy, to prepare for the next iteration.
            foreach (GameObject Hitbox in cluster_of_objects)
            {
                if (unfiltered_objects_copy.Contains(Hitbox))
                {
                    unfiltered_objects_copy.Remove(Hitbox);
                }
            }


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Repeat the failsafe loop again. [][][][][][]
            foreach (GameObject HitBox in unfiltered_objects_copy)
            {
                if (!(cluster_of_objects.Contains(HitBox))) //if hitbox gets added in the meantime, we skip so as not to add several of the same hitbox.
                {
                    foreach (GameObject Hitbox_accepted in cluster_of_objects)
                    {
                        if (Mathf.Abs(HitBox.transform.position.x - Hitbox_accepted.transform.position.x) < 0.3)
                        {
                            cluster_of_objects.Add(HitBox);
                            break; //break out to prevent duplicate hitboxes.
                        }

                    } 

                }
            }

            //Removing additional objects from unfiltered_objects copy which were since added to the ONE cluster. [][][]
            foreach (GameObject Hitbox in cluster_of_objects)
            {
                if (unfiltered_objects_copy.Contains(Hitbox))
                {
                    unfiltered_objects_copy.Remove(Hitbox);
                }
            }


           
            //Repeat the failsafe loop again. [][][][][][]
            foreach (GameObject HitBox in unfiltered_objects_copy)
            {
                if (!(cluster_of_objects.Contains(HitBox))) //if hitbox gets added in the meantime, we skip so as not to add several of the same hitbox.
                {
                    foreach (GameObject Hitbox_accepted in cluster_of_objects)
                    {
                        if (Mathf.Abs(HitBox.transform.position.x - Hitbox_accepted.transform.position.x) < 0.3)
                        {
                            cluster_of_objects.Add(HitBox);
                            break; //break out to prevent duplicate hitboxes.
                        }

                    }

                }
            }

            //Removing additional objects from unfiltered_objects copy which were since added to the ONE cluster. [][][]
            foreach (GameObject Hitbox in cluster_of_objects)
            {
                if (unfiltered_objects_copy.Contains(Hitbox))
                {
                    unfiltered_objects_copy.Remove(Hitbox);
                }
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////





            //This cluster of objects is added to the dictionary, with its key being the numbered_cluster. This is just an index value from 0 to 3, inclusive.
            filtered_objects_in_clusters.Add(numbered_cluster, cluster_of_objects);
            
        }

///////////////////////////////////////////////////////////////////////////////////////////while loop exit. After this while loop is complete, all objects are separated by proximity.

        List<float> samples_from_each_cluster_baseline = new List<float>();
        foreach (KeyValuePair<int, List<GameObject>> cluster in filtered_objects_in_clusters)
        {
            samples_from_each_cluster_baseline.Add(cluster.Value[0].transform.position.x);
        }





        //Clear dictionaries from previous entries before we begin applying anything to dictionaries.
        HTTPListener.AllNodes['x'.ToString()].Clear();
        HTTPListener.AllNodes['y'.ToString()].Clear();
        HTTPListener.AllNodes['z'.ToString()].Clear();
        HTTPListener.AllNodes['w'.ToString()].Clear();





        int cluster_id = -1;
        while (filtered_objects_in_clusters.Count > 0) //keep doing this till we run out of clusters. So we should do this four times.
        {
            cluster_id += 1;
            List<float> samples_from_each_cluster = new List<float>();
            //iterate over each cluster, each cluster being saved in the filtered_objects_in_clusters
            foreach (KeyValuePair<int, List<GameObject>> cluster in filtered_objects_in_clusters)
            {
                samples_from_each_cluster.Add(cluster.Value[0].transform.position.x);
            }

            //We find which cluster the max vaue belongs to. I.e. cluster 0, 1, 2 or 3.
            float minimum_value = samples_from_each_cluster.Max();
            int minimum_value_cluster_id = samples_from_each_cluster.IndexOf(minimum_value);
           
            /*
            int cluster_id = 516161; //can't leave it undefined, gotta use super improable number.
            int loop_counter = -1;
            foreach (float value in samples_from_each_cluster_baseline)
            {
                loop_counter += 1;
                if (value == maximum_value)
                {
                    cluster_id = loop_counter;
                }

            }
            */

            Dictionary<string, (float, float)> temporaryNodeDict = new Dictionary<string, (float, float)>();
            foreach (GameObject HitBox in filtered_objects_in_clusters.ElementAt(minimum_value_cluster_id).Value) //used to be just [cluster_id]
            {
                temporaryNodeDict.Add(HitBox.name, (0, 0));
            }

            string node_to_be_transcribed_to = "";

            if (cluster_id == 0)
            {
                node_to_be_transcribed_to = "x";
            }
            else if (cluster_id == 1)
            {
                node_to_be_transcribed_to = "y";
            }
            else if (cluster_id == 2)
            {
                node_to_be_transcribed_to = "z";
            }
            else if (cluster_id == 3)
            {
                node_to_be_transcribed_to = "w";
            }



            HTTPListener.AllNodes[node_to_be_transcribed_to] = temporaryNodeDict;
            filtered_objects_in_clusters.Remove(filtered_objects_in_clusters.ElementAt(minimum_value_cluster_id).Key); //used to be cluster_idy
            samples_from_each_cluster.Remove(minimum_value_cluster_id); //used to be cluster_idy

            //temporaryNodeDict.Clear();

        }
        




    }
  
         

}