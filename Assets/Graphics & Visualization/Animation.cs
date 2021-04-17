using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation : MonoBehaviour
{
    public static void animationStart(float seconds) => Global.current_animation_endtime = Time.realtimeSinceStartup + seconds + 1f;
    // Il faut ajouter une certaine pause (ici 0.4f) pour que l'animation finit totalement

    /* representation_type :
     * 0 -> setValueAnimation of a GraphicalInteger
     * 1 -> setValueAnimation of a GraphicalFloat
     * 2 -> setValueAnimation of a GraphicalBool
     * 3 -> setValueAnimation of a GraphicalList
     */

    public static IEnumerator setValueAnimation(GraphicalObject graphObj, int representation_type, object val_to_set, object act_val, float seconds)
    {
        animationStart(seconds);

        if (representation_type == 0)
        {
            float difference = ((int) val_to_set) - ((float) act_val); // Corriger avec un try, catch

            for(float i = 0; i < seconds; i += Time.deltaTime)
            {
                float added_height = difference * Time.deltaTime / seconds;
                graphObj.game_obj.transform.localScale += (new Vector3 (0, added_height, 0));
                graphObj.game_obj.transform.position += new Vector3 (0, added_height / 2f, 0);
                graphObj.floating_number.GetComponent<TextMesh>().text = graphObj.game_obj.transform.localScale.y.ToString(".##"); // A voir si c'est couteux

                yield return null;
            }

            Vector3 new_pos = new Vector3(graphObj.position.x, (graphObj.position.y - (graphObj.dimensions.y / 2f)) + ((int)val_to_set / 2f), graphObj.position.z);
            Vector3 new_scale = new Vector3(graphObj.dimensions.x, (int)val_to_set, graphObj.dimensions.z);

            graphObj.position = new_pos;
            graphObj.dimensions = new_scale;

            graphObj.drawObject();
        }
        else if (representation_type == 1)
        {
            float difference = ((float)val_to_set) - ((float)act_val); // Corriger avec un try, catch

            for (float i = 0; i < seconds; i += Time.deltaTime)
            {
                float added_height = difference * Time.deltaTime / seconds;
                graphObj.game_obj.transform.localScale += (new Vector3(0, added_height, 0));
                graphObj.game_obj.transform.position += new Vector3(0, added_height / 2f, 0);
                graphObj.floating_number.GetComponent<TextMesh>().text = graphObj.game_obj.transform.localScale.y.ToString(".##"); // A voir si c'est couteux

                yield return null;
            }

            Vector3 new_pos = new Vector3(graphObj.position.x, (graphObj.position.y - (graphObj.dimensions.y / 2f)) + ((float)val_to_set / 2f), graphObj.position.z);
            Vector3 new_scale = new Vector3(graphObj.dimensions.x, (float)val_to_set, graphObj.dimensions.z);

            graphObj.position = new_pos;
            graphObj.dimensions = new_scale;

            graphObj.drawObject();
        }
        else if (representation_type == 2)
        {
            seconds /= 2f;
            // fade out animation of the old val
            GameObject obj_to_fade = graphObj.game_obj;
            MeshRenderer render = obj_to_fade.GetComponent<MeshRenderer>();
            if ((bool)act_val)
                render.material = Resources.Load<Material>("Materials/true_transp_material");
            else
                render.material = Resources.Load<Material>("Materials/false_transp_material");

            Color set_not_transparent = new Color(render.material.color.r, render.material.color.g, render.material.color.b, 1f);
            render.material.color = set_not_transparent;
            float stable_alpha_color = render.material.color.a;

            graphObj.floating_number.GetComponent<TextMesh>().text = ""; // pour enlever le texte avant de passer a la prochaine valeur

            while (render.material.color.a > 0)
            {
                Color new_color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, render.material.color.a - (stable_alpha_color * Time.deltaTime / seconds));
                render.material.color = new_color;
                yield return null;
            }

            if ((bool)val_to_set)
                render.material = Resources.Load<Material>("Materials/true_transp_material");
            else
                render.material = Resources.Load<Material>("Materials/false_transp_material");

            Color set_to_transparent = new Color(render.material.color.r, render.material.color.g, render.material.color.b, 0f);
            render.material.color = set_to_transparent;
            stable_alpha_color = 1 - render.material.color.a;

            while (render.material.color.a < 1)
            {
                Color new_color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, render.material.color.a + (stable_alpha_color * Time.deltaTime / seconds));
                render.material.color = new_color;
                yield return null;
            }

            if ((bool)val_to_set)
                render.material = Resources.Load<Material>("Materials/true_material");
            else
                render.material = Resources.Load<Material>("Materials/false_material");
            
            graphObj.drawObject();
        }
    }
    
    public static IEnumerator insertAnimation(GameObject[] obj_list, int index, Vector3 vector, float seconds, int target_alpha, GraphicalList list)
    {
        animationStart(seconds + 0.8f); // 0.5f + 0.3f (WaitForSeconds)
        seconds /= 2f;

        for (float i = 0; i < seconds; i += Time.deltaTime)
        {
            foreach (GameObject obj in obj_list)
            {
                obj.transform.position += (vector * Time.deltaTime / seconds);
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        int graph_list_length = list.graph_list.Count;

        GraphicalObject obj_inst = list.graph_list[graph_list_length - 1];

        list.graph_list.RemoveAt(graph_list_length - 1);
        list.graph_list.Insert(index, obj_inst);

        list.drawObject();

        GameObject obj_to_fade = list.graph_list[index].game_obj;
        obj_to_fade.SetActive(true);
        MeshRenderer render = obj_to_fade.GetComponent<MeshRenderer>();

        render.material = Resources.Load<Material>("Materials/obj_transparent_mat");
        Color set_to_transparent = new Color(render.material.color.r, render.material.color.g, render.material.color.b, 0f);
        render.material.color = set_to_transparent;
        float stable_alpha_color = target_alpha - render.material.color.a;

        while (render.material.color.a < target_alpha)
        {
            Color new_color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, render.material.color.a + (stable_alpha_color * Time.deltaTime / seconds));
            render.material.color = new_color;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        render.material = Resources.Load<Material>("Materials/obj_ordinary_mat");
        list.drawObject();
    }

    public static IEnumerator removeAnimation(GameObject[] obj_list, int index, Vector3 vector, float seconds, int target_alpha, GraphicalList list)
    {
        animationStart(seconds + 0.8f); // 0.2f + 0.6f + 0.2f (WaitForSeconds)
        seconds /= 2f;

        GameObject obj_to_fade = list.graph_list[index].game_obj;
        MeshRenderer render = obj_to_fade.GetComponent<MeshRenderer>();

        render.material = Resources.Load<Material>("Materials/obj_transparent_mat");
        Color set_to_transparent = new Color(render.material.color.r, render.material.color.g, render.material.color.b, 1f);
        render.material.color = set_to_transparent;
        float stable_alpha_color = render.material.color.a - target_alpha;

        while (render.material.color.a > target_alpha)
        {
            Color new_color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, render.material.color.a - (stable_alpha_color * Time.deltaTime / seconds));
            render.material.color = new_color;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        GameObject obj_to_remove = list.graph_list[index].game_obj;

        list.graph_list.RemoveAt(index);
        Destroy(obj_to_remove);

        yield return new WaitForSeconds(0.6f); // Ajout de 0.6 seconde pour la transition

        for (float i = 0; i < seconds; i += Time.deltaTime)
        {
            foreach (GameObject obj in obj_list)
            {
                obj.transform.position += (vector * Time.deltaTime / seconds);
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        list.drawObject();
    }

    public static IEnumerator swapAnimation(GameObject obj1, GameObject obj2, float seconds, GraphicalList list)
    {
        animationStart(seconds);
        Vector3 vector_obj1 = new Vector3(obj2.transform.position.x - obj1.transform.position.x, 0, 0);
        Vector3 vector_obj2 = new Vector3(obj1.transform.position.x - obj2.transform.position.x, 0, 0);

        for (float i = 0; i < seconds; i += Time.deltaTime)
        {
            obj1.transform.position += vector_obj1 * Time.deltaTime / seconds;
            obj2.transform.position += vector_obj2 * Time.deltaTime / seconds;

            yield return null;
        }

        list.drawObject();
    }

}
