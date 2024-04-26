using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetGraph : MonoBehaviour
{
    public GameObject Camera;
    public GameObject KnowledgeGraph;
    public GameObject RetrievalButton;

    public bool RepositionGraphToggle;

    // Start is called before the first frame update
    private void Start()
    {
        TaskOnClick();
    }

    private void Update()
    {
        if (RepositionGraphToggle)
        {
            RepositionGraphToggle = false;
            TaskOnClick();
        }
    }


    public void TaskOnClick()
    {
        KnowledgeGraph.GetComponent<Transform>().position = Camera.GetComponent<Transform>().position + Camera.GetComponent<Transform>().forward * 1.15f;
        KnowledgeGraph.GetComponent<Transform>().rotation = Camera.GetComponent<Transform>().rotation;

        KnowledgeGraph.GetComponent<Transform>().rotation = Quaternion.Euler(0, KnowledgeGraph.GetComponent<Transform>().eulerAngles.y, KnowledgeGraph.GetComponent<Transform>().eulerAngles.z);

        KnowledgeGraph.GetComponent<Transform>().localScale = new Vector3(0.03f, 0.03f, 0.03f);



        RetrievalButton.GetComponent<Transform>().position = Camera.GetComponent<Transform>().position + Camera.GetComponent<Transform>().forward * 1.10f;
        //RetrievalButton.GetComponent<Transform>().position = Camera.GetComponent<Transform>().position + Camera.GetComponent<Transform>().right * (-0.05f);
        //RetrievalButton.GetComponent<Transform>().position = Camera.GetComponent<Transform>().position + Camera.GetComponent<Transform>().up * (-1f);
        RetrievalButton.GetComponent<Transform>().rotation = Camera.GetComponent<Transform>().rotation;

        RetrievalButton.GetComponent<Transform>().rotation = Quaternion.Euler(0, KnowledgeGraph.GetComponent<Transform>().eulerAngles.y, KnowledgeGraph.GetComponent<Transform>().eulerAngles.z);
    }
}
