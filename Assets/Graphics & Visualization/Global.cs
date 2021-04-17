using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{

    public static GameObject game_object;
    //
    public static Dictionary<string, GraphicalObject> object_dict = new Dictionary<string, GraphicalObject>();
    //
    public static float space_between_elements = 0.1f;
    //
    public static float current_animation_endtime = 0f;

    //
    public static bool graphicalFunctions(Parser.Alteration alteration)
    {
        Debug.Log("Appel");
        string name_function = alteration.name;
        string var_name = alteration.affected.getName();
        dynamic obj = alteration.main_value; // != Parser.Variable (e.g. int, float, List<Variable>, bool)

        Debug.Log("Alteration: " + alteration.ToString());
        Debug.Log("Affected: " + alteration.affected.getName());

        // Debug.Log("THE FUNCTION NAME IS : " + name_function);

        //
        if (!object_dict.ContainsKey(var_name))
        {
            // ajout de la variable
            if (obj is List<dynamic>)
            {
                Parser.DynamicList new_list = new Parser.DynamicList(Parser.DynamicList.valueFromRawList(obj));
                new_list.setName(var_name);
                addVariableToDict(game_object, new_list, new Vector3(20, 0, 1), new Vector3(0, 0, 0), 0);
                object_dict[var_name].drawObject();
                return true;
            }
            else if (obj is int)
            {
                Parser.Integer new_int = new Parser.Integer((int)obj);
                new_int.setName(var_name);
                addVariableToDict(game_object, new_int, new Vector3(1, (int)obj, 1), new Vector3(0, (int)obj / 2f, 0), 0);
                object_dict[var_name].drawObject();
                return true;
            }
            else if (obj is float)
            {
                Parser.FloatVar new_float = new Parser.FloatVar((float)obj);
                new_float.setName(var_name);
                addVariableToDict(game_object, new_float, new Vector3(1, (float)obj, 1), new Vector3(0, (float)obj / 2f, 0), 0);
                object_dict[var_name].drawObject();
                return true;
            }
            else if (obj is bool)
            {
                Parser.BooleanVar new_bool = new Parser.BooleanVar((bool)obj);
                new_bool.setName(var_name);
                addVariableToDict(game_object, new_bool, new Vector3(1, 10, 1), new Vector3(0, 5, 0), 0);
                object_dict[var_name].drawObject();
                return true;
            }
            else
                return false;
        }
        else
        {
            switch (name_function)
            {
                case "swap":
                    // assert that (object_dict[var_name] is GraphicalList)
                    Debug.Assert(object_dict[var_name] is GraphicalList);
                    Parser.Integer index1 = new Parser.Integer(alteration.minor_values[0]);
                    Parser.Integer index2 = new Parser.Integer(alteration.minor_values[1]);
                    //Parser.Variable.fromRawValue(alteration.main_value);
                    (object_dict[var_name] as GraphicalList).list.forceSetValue(obj);
                    (object_dict[var_name] as GraphicalList).swap(index1, index2);
                    return true;
                case "insertValue":
                    // assert that (object_dict[var_name] is GraphicalList)
                    Debug.Assert(object_dict[var_name] is GraphicalList);
                    (object_dict[var_name] as GraphicalList).list.forceSetValue(obj);
                    // alteration.minor_values[0] represents the value
                    // alteration.minor_values[1] represents the index
                    (object_dict[var_name] as GraphicalList).insert(new Parser.Integer(alteration.minor_values[0]),
                                                                    Parser.Variable.fromRawValue(alteration.minor_values[1]));
                    return true;
                case "removeValue":
                    // assert that (object_dict[var_name] is GraphicalList)
                    Debug.Assert(object_dict[var_name] is GraphicalList);
                    (object_dict[var_name] as GraphicalList).list.forceSetValue(obj);
                    (object_dict[var_name] as GraphicalList).remove(new Parser.Integer(alteration.minor_values[0]));
                    return true;
                case "setValue":
                    // update "list"
                    if (object_dict[var_name] is GraphicalList)
                        (object_dict[var_name] as GraphicalList).list.forceSetValue(obj);
                    object_dict[var_name].setValue(alteration.minor_values[0]);
                    Debug.Log("setValue");
                    return true;
                case "list_at": // "list_at"
                    Debug.Assert(object_dict[var_name] is GraphicalList);
                    List<int> indexes = new List<int>();
                    foreach (dynamic index in alteration.minor_values[0])
                        indexes.Add((index as Parser.Integer).getValue());
                    (object_dict[var_name] as GraphicalList).list.forceSetValue(obj); // c'est mieux comme ça, au lieu de recréer une nouvelle liste à chaque fois 
                    (object_dict[var_name] as GraphicalList).listAt(indexes);
                    return true;
                default:
                    Debug.Log("Unknown call : " + name_function);
                    return false;
            }
        }
    }

    //
    public static void addVariableToDict(GameObject gameObject, Parser.Variable variable, Vector3 dimensions, Vector3 position, int representation)
    {
        string name = variable.getName();
        if (object_dict.ContainsKey(name)) throw new Exception("Already in object_dict !");
        object_dict.Add(name, GraphicalObject.graphicalObjectFromVariable(gameObject, variable, dimensions, position, representation));
    }

    /*
    public static bool graphicalFunctions(Parser.Alteration alteration)
    {
        GraphicalList clone_list;

        string name_function = alteration.name;
        dynamic main_value = alteration.main_value;
        dynamic[] min_values = alteration.minor_values;

        //Parser.Debugging.assert();

        string obj_name = main_value.getName();

        if (!list_dict.ContainsKey(obj_name))
        {
            clone_list = new GraphicalList(obj_name, main_value, new Vector3 (0, 0, 0), new Vector3(0, 0, 0), 0);
            list_dict.Add(obj_name, clone_list);
            clone_list.drawObject();
        }
        else
        {
            clone_list = list_dict[name_function];
        }

        switch (name_function)
        {
            case "swap":
                clone_list.swap();
                return true;
            case "insert":
                clone_list.insert();
                return true;
            case "remove":
                clone_list.remove();
                return true;
            case "setValue":
                clone_list.//animation a faire qui enleve toutes les valeurs d'une liste (fade) et en met de nouveaux
                return true;
            default:
                return false;
        }
    }
    */
}
