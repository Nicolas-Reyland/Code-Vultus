using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
	[SerializeField] MenuButtonController menuButtonController;
	[SerializeField] Animator animator;
	[SerializeField] AnimatorFunctions animatorFunctions;
	[SerializeField] int thisIndex;
	[SerializeField] GameObject other_menu;

	// Update is called once per frame
	void Update()
	{
		if (menuButtonController.index == thisIndex)
		{
			animator.SetBool("selected", true);
			if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) && other_menu.name == "OptionsMenu")
			{
				animator.SetBool("pressed", true);
				if (thisIndex == 0)
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
				else if (thisIndex == 2)
				{
					transform.parent.gameObject.SetActive(false);
					other_menu.SetActive(true);
					other_menu.transform.parent.gameObject.GetComponent<MenuButtonController>().maxIndex = 1; // A definir
					other_menu.transform.parent.gameObject.GetComponent<MenuButtonController>().index = 0;
				}
				else if (thisIndex == 3)
					Application.Quit();
			}
			else if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) && other_menu.name == "MainMenu")
			{
				if (thisIndex == 0)
                {
					transform.parent.gameObject.SetActive(false);
					other_menu.SetActive(true);
					other_menu.transform.parent.gameObject.GetComponent<MenuButtonController>().maxIndex = 3;
					other_menu.transform.parent.gameObject.GetComponent<MenuButtonController>().index = 0;
				}
            }
			else if (animator.GetBool("pressed"))
			{
				animator.SetBool("pressed", false);
				animatorFunctions.disableOnce = true;
			}
		}
		else
		{
			animator.SetBool("selected", false);
		}
	}
}
