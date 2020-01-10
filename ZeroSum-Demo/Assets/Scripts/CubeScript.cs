using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    Vector3 starPos;
    void Start()
    {
        starPos = transform.localPosition;
    }

    public void ResetCubes()
    {
        transform.GetComponent<Rigidbody>().isKinematic = true;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = starPos;
        gameObject.SetActive(true);

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Respawn")
        {
            gameObject.SetActive(false);
        }
    }


}
