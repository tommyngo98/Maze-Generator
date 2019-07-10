using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    float inputFactor = 5;

    public Text infoDisplay;
    public Text oldTimeDisplay;
    public Text newTimeDisplay;
    float startTime; //Start time of the round
    bool startRound = false;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        float oldTime = 0;
        if (PlayerPrefs.HasKey("oldTime"))
        {
            oldTime = PlayerPrefs.GetFloat("oldTime");
        }
        oldTimeDisplay.text = string.Format("Old Time: {0,6:0.0} sec", oldTime);
    }

    // Update is called once per frame
    void Update()
    {
        //The arrow keys get active and the time starts running when the enter key was pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            startRound = true;
            startTime = Time.time;
        }

        if (startRound)
        {
            //The difference between the start time and the current time will be displayed
            newTimeDisplay.text = string.Format("New Time: {0,6:0.0} sec.", Time.time - startTime);


            if (Input.GetKey(KeyCode.RightArrow))
            {
                float xAxis = Input.GetAxis("Horizontal");
                Vector3 posChange = new Vector3(xAxis, 0.0f, 0.0f);
                rb.AddForce(posChange * inputFactor);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                float xAxis = Input.GetAxis("Horizontal");
                Vector3 posChange = new Vector3(xAxis, 0.0f, 0.0f);
                rb.AddForce(posChange * inputFactor);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                float zAxis = Input.GetAxis("Vertical");
                Vector3 posChange = new Vector3(0.0f, 0.0f, zAxis);
                rb.AddForce(posChange * inputFactor);
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                float zAxis = Input.GetAxis("Vertical");
                Vector3 posChange = new Vector3(0.0f, 0.0f, zAxis);
                rb.AddForce(posChange * inputFactor);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //End the round if the top right walls get hit
        if (collision.gameObject.tag == "Target")
        {
            Invoke("EndOfGame", 1.2f);
            startRound = false;
            PlayerPrefs.SetFloat("oldTime", Time.time - startTime);
            PlayerPrefs.Save();
        }
    }

    //Make the player inactive
    void EndOfGame()
    {
        gameObject.SetActive(false);
        infoDisplay.text = "You made it";
    }
}
