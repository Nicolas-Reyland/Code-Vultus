using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostMortem : MonoBehaviour
{
    public static Queue<Parser.Alteration> animation_queue = new Queue<Parser.Alteration>();

    // Update is called once per frame
    void Update()
    {
        if (animation_queue.Count != 0 && Time.realtimeSinceStartup > Global.current_animation_endtime)
        {
            Debug.Log("Next Animation");
            Parser.Alteration next_anim_alter = animation_queue.Dequeue();
            Global.graphicalFunctions(next_anim_alter);
        }
    }

    public static float AddAnimation(Parser.Alteration alteration)
    {
        Debug.Log("Adding Alteration to Queue: " + alteration);
        animation_queue.Enqueue(alteration);
        return 0.0f;
    }
}
