using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Vector3 starPos;
    private void Awake()
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
