using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonController : MonoBehaviour {

	// Use this for initialization
	public int index;
	public int maxIndex;
	public AudioSource audioSource;

	void Start () 
	{
		audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
			if (index > 0)
			{
				index = 0;
			}
			else
			{
				index = 1;
			}

		}

		else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
			if (index > 0)
			{
				index = 0;
			}
			else
			{
				index = 1;
			}

        }

		else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
			if (index > 0)
			{
				index -= 1;
			}
			else
			{
				index = maxIndex;
			}
		}

		else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
			if (index < maxIndex)
			{
				index += 1;
			}
			else
			{
				index = 0;
			}
		}

	}

}
