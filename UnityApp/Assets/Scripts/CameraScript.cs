using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("Main Camera").GetComponent<Transform>().position.y != 6.28f)
        {
            GameObject.Find("Main Camera").GetComponent<Transform>().position = new Vector3(GameObject.Find("Main Camera").GetComponent<Transform>().position.x, 6.28f, GameObject.Find("Main Camera").GetComponent<Transform>().position.z);
        }
    }
}
