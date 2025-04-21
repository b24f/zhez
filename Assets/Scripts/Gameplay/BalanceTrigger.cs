using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceTrigger : MonoBehaviour
{
    public float imbalanceAmount = 0.5f;
    public float correctionTorque = 5f;
    public float maxTiltAngle = 30f;


    private bool isBalancing = false;
    private Rigidbody player;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = other.attachedRigidbody;
            StartBalancing(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StopBalancing(other.attachedRigidbody);
        }
    }

    private void Update()
    {
        if (!isBalancing) return;

        float input = Input.GetAxisRaw("Horizontal");

        player.AddTorque(transform.forward * -input * correctionTorque, ForceMode.Force);

        float tilt = Vector3.Angle(Vector3.up, transform.up);

        if (tilt > maxTiltAngle)
        {
            Debug.Log("Player lost balance");
        }
    }

    private void StartBalancing(Rigidbody player)
    {
        isBalancing = true;
        float randomShift = Random.Range(-imbalanceAmount, imbalanceAmount);
        player.centerOfMass = new Vector3(randomShift, player.centerOfMass.y, player.centerOfMass.z);
    }

    private void StopBalancing(Rigidbody player)
    {
        isBalancing = false;
        player.centerOfMass = Vector3.zero;
    }
}
