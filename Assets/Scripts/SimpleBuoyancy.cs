using System.Collections.Generic;
using UnityEngine;

public class SimpleBuoyancy : MonoBehaviour
{
    public float floatStrength = 10f; // 浮力の強さ
    private List<GameObject> TouchingPlayers;

    void Start()
    {
        TouchingPlayers = new List<GameObject>();
    }
    void FixedUpdate()
    {
        foreach (var player in TouchingPlayers)
        {
            player.GetComponent<Rigidbody>().AddForce(new Vector3(0, floatStrength, 0), ForceMode.Impulse);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.gameObject.tag);
        if (collider.gameObject.tag == "Player")
        {
            Debug.Log("collider.gameObject.tag");
            TouchingPlayers.Add(collider.gameObject);
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            TouchingPlayers.Remove(collider.gameObject);
        }
    }
}