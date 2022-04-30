using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] Transform target;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (GameManager.Instance.State != GameState.freeRoam)
            gameObject.SetActive(false);
        */
        Vector3 desiredPos = target.position;
        desiredPos.z = transform.position.z;

        transform.position = desiredPos;
    }
}
