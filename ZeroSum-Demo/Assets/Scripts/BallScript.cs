using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class BallScript : MonoBehaviour
{
    ControlManager controlManager;
    public Vector3 originalPositions;
    private void Start()
    {
        DOTween.Init();
        controlManager = ControlManager.Instance;
        originalPositions = transform.localPosition;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ExplosionPoint")
        {
            Time.timeScale = 0.4f;

            //  controlManager.CurrentWall;
            controlManager.WallExplosion();
        }
        else if (other.tag == "Respawn")
        {
            controlManager.CheckBallsCount();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "ExplosionPoint")
        {
            Time.timeScale = 1f;
        }
    }

    public void ResetBallsPos()
    {
        transform.localPosition = originalPositions;
        transform.GetComponent<MeshRenderer>().enabled = true;
        transform.GetComponent<Rigidbody>().useGravity = false;
        transform.GetComponent<Rigidbody>().isKinematic = true;

    }
}
