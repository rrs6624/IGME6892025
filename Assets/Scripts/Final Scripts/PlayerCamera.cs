using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerLocation = player.transform.position;
        this.transform.position = new Vector3(playerLocation.x, (float)30.37, playerLocation.z - 10);
        Debug.Log("Player Location: " + playerLocation);
        Debug.Log("Camera Location: " + this.transform.position);
    }
}
