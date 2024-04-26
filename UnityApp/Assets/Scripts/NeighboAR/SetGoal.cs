using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class SetGoal : MonoBehaviour
{
    public bool NewGoalVariable;
    public HTTPListener HTTPListener;
    public GazeDataFromHL2ExampleUsingARETT GazeDataFromHL2ExampleUsingARETT;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (NewGoalVariable)
        {
            NewGoalVariable = false;
            NewGoal(HTTPListener.message);
        }

    }







    public void NewGoal(NameValueCollection message)
    {
        Debug.Log("Object goal command detected.");
        //PlayerScript.RebuildLoopVariable = false;
        //Debug.Log("Coroutine stopped.");
        Debug.Log("TextBox_" + message[0]);
        GameObject goalObject = GameObject.Find("TextBox_" + message[0]);
        //GameObject goalObject = GameObject.Find("KnowledgeGraph");
        Debug.Log(goalObject);
        Debug.Log("Goal Object found.");

        List<string> list_of_objects = new List<string>();
       
        //the child in this case is every single slot, i.e. NW, NE, N etc.
        foreach (Transform child in goalObject.transform.parent.parent)
        {
            try //possible that a child is accessed which further has no children of its own, so this code can't be executed.
            {
                list_of_objects.Add(child.GetChild(0).name.Remove(0,8));
            }
            catch { }
            
        }

        list_of_objects.Remove(goalObject.name);
        List<long> list_of_times = new List<long>();
        foreach (string object_name in list_of_objects.ToList())
        {
            if (GazeDataFromHL2ExampleUsingARETT.ObjectAttention.ContainsKey(object_name))
            {
                list_of_times.Add(GazeDataFromHL2ExampleUsingARETT.ObjectAttention[object_name]);
            }
            else
            {
                list_of_objects.Remove(object_name);
            }   
        }

        var maxIndex = list_of_times.IndexOf(list_of_times.Max());
        string max_attention_object = "bawfggqrqwqtgwqtqwtfqw";
        foreach (string object_name in list_of_objects)
        {
            if (list_of_objects.IndexOf(object_name) == maxIndex)
            {
                max_attention_object = object_name;
                break;
            }
        }





        foreach (Transform child in goalObject.transform.parent.parent)
        {
            
            
            try //possible that a child is accessed which further has no children of its own, so this code can't be executed.
            {
                if ((child.GetChild(0).name.Remove(0, 8) != max_attention_object) & (child.GetChild(0).name != goalObject.name))
                {
                    //Applies to objects in proximity. But NOT the landmark.    
                    child.GetChild(0).GetComponent<TextMeshPro>().color = new Color32(255, 255, 255, 84);
                    child.GetChild(0).GetChild(0).gameObject.SetActive(true); //this is the picture
                    child.GetChild(0).GetChild(0).transform.localScale = new Vector3(0.18f, 0.33f, 0.18f);
                    child.GetChild(0).GetChild(0).transform.localPosition = new Vector3(-0.123f, 1f, 0f);
                    child.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 3); //remove background highlight
                    child.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
                }

            }
            catch { }

        }



        //Applies to goal object.
        goalObject.GetComponent<TextMeshPro>().color = new Color32(255, 255, 255, 255);
        goalObject.transform.GetChild(0).gameObject.SetActive(true); //this is the picture
        
        
        
        //Decreasing size of all objects which are not in the same node as the goal object.
        string goalNode = goalObject.transform.parent.parent.name;
        foreach (Transform node in GameObject.Find("KnowledgeGraph").transform)
        {
            if (node.name != goalNode)
            {
                foreach (Transform slot in node)
                {
                    try //possible that a child is accessed which further has no children of its own, so this code can't be executed.
                    {
                        //Applies to all objects which are NOT in the goal node.
                        slot.GetChild(0).GetComponent<TextMeshPro>().color = new Color32(255, 255, 255, 10);
                        slot.GetChild(0).GetChild(0).gameObject.SetActive(true);
                        slot.GetChild(0).GetChild(0).transform.localScale = new Vector3(0.13f, 0.33f, 0.13f);
                        slot.GetChild(0).GetChild(0).transform.localPosition = new Vector3(-0.123f, 0.8f, 0f);
                        slot.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 2); // remove background highlight
                        slot.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
                    }
                    catch { }
                }




            }
        }





    }



}
