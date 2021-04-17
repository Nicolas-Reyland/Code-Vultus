using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Quasiment tout ce qui sera dessiné héritera de cette classe
Les seules exceptions sont:
 - les éléments du menu
 - les éléments de l'interface
 -
*/
public abstract class GraphicalObject : MonoBehaviour
{
    // attributes
    public Vector3 dimensions;        // ex: (0.9f, 1, 1) -> (scale.x, scale.y, scale.z)
    public Vector3 position;          // ex: (8, 0, -7) -> (position.x, position.y, position.z)
    public GameObject game_obj;
    public GameObject floating_number;
    public float floatnumb_y_constant = 1f; // Pour mettre les "floating number" a une bonne hauteur (valeur arbitraire)

    //protected string name;          // ex: "List 1" // déjà intégré dans MonoBehaviour
    protected Parser.Variable variable;    // ex: Integer

    protected int representation;

    // getters
    public string getName() => name;
    public Parser.Variable getVariable() => variable;

    // setters
    public void setVarValue(Parser.Variable new_variable) => variable = new_variable;

    // methods
    public abstract void initialize(Parser.Variable var, GameObject parent_obj, Vector3 dimensions, Vector3 position, int representation);

    public abstract void setValue(dynamic new_val);
    public abstract void drawObject();       // dessine l'objet.
                                             // Doit être implémenté pour chaque sous-classe de GraphicalObject

    public static GraphicalObject graphicalObjectFromVariable(GameObject gameObject, Parser.Variable variable, Vector3 dimensions, Vector3 position, int representation)
    {
        // be sure that the variable has a name
        System.Diagnostics.Debug.Assert(variable.getName() != null);
        // from Parser.Variable to GraphicalObject
        if (variable is Parser.Integer)
        {
            return GraphicalInteger.graphicalIntegerFromInteger(gameObject, variable as Parser.Integer, dimensions, position, representation);
        }

        if (variable is Parser.FloatVar)
        {
            return GraphicalFloat.graphicalFloatFromFloat(gameObject, variable as Parser.FloatVar, dimensions, position, representation);
        }

        if (variable is Parser.BooleanVar)
        {
            return GraphicalBoolean.graphicalBooleanFromBoolean(gameObject, variable as Parser.BooleanVar, dimensions, position, representation);
        }

        if (variable is Parser.DynamicList)
        {
            return GraphicalList.graphicalListFromDynamicList(gameObject, variable as Parser.DynamicList, dimensions, position, representation);
        }

        throw new Exception("Unrecognized Variable type: " + variable.getTypeString());
    }
}

// exemple: l'entier graphique
public class GraphicalInteger : GraphicalObject // on utilisera probablement une classe "nombre" qui fera int et flottant, voir classe abstraite
{
    public override void initialize(Parser.Variable val, GameObject parent_obj, Vector3 dimensions, Vector3 position, int representation) // representation : 0 -> dans une liste, 1 -> un index pointant sur une valeur de liste, 2 -> ...
    {
        this.variable = val as Parser.Integer;
        this.name = this.variable.getName();
        this.dimensions = dimensions;
        this.position = position;
        this.representation = representation;
        this.game_obj = Instantiate(Resources.Load<GameObject>("Prefabs/Number_prefab"), Vector3.zero, Quaternion.identity); // new Vector3(position.x, position.y + variable.getValue() / 2f, position.z);
        this.game_obj.transform.parent = parent_obj.transform;
        this.game_obj.transform.localPosition = position + new Vector3(dimensions.x / 2f, 0, 0); // car le redimensionnement d'un objet se fait sur deux directions
        this.game_obj.transform.localScale = dimensions;

        this.floating_number =
            Instantiate(Resources.Load<GameObject>("Prefabs/FloatingNumber"),
            new Vector3(this.game_obj.transform.position.x, parent_obj.transform.position.y,
            this.game_obj.transform.position.z), Quaternion.identity);

        this.floating_number.GetComponent<TextMesh>().text = this.variable.getValue().ToString();
        this.floating_number.transform.SetParent(this.game_obj.transform);
    }

    public void updatePosition(Vector3 new_position) => this.position = new_position;

    public void updateDimension(Vector3 new_dimensions) => this.dimensions = new_dimensions;

    public override void drawObject()
    {
        Vector3 prefab_scale = Resources.Load<GameObject>("Prefabs/FloatingNumber").transform.localScale;
        Vector3 prefab_pos = Resources.Load<GameObject>("Prefabs/FloatingNumber").transform.position;
        
        this.game_obj.transform.localPosition = this.position + new Vector3(this.dimensions.x / 2f, 0, 0); // car le redimensionnement d'un objet se fait sur deux directions
        this.game_obj.transform.localScale = this.dimensions;

        this.floating_number.transform.SetParent(this.game_obj.transform);

        this.floating_number.transform.localPosition = Vector3.zero;
        this.floating_number.transform.position = new Vector3(this.floating_number.transform.position.x, 
            this.game_obj.transform.position.y - (this.game_obj.transform.localScale.y / 2f) + floatnumb_y_constant, 
            this.floating_number.transform.position.z);

        if (this.game_obj.transform.localScale.x != 0) // Pour eviter le cas de la division par 0
        {
            if (this.game_obj.transform.localScale.y != 0)
                if (this.game_obj.transform.localScale.z != 0)
                {
                    this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y / this.game_obj.transform.localScale.y,
                prefab_scale.z / this.game_obj.transform.localScale.z);
                }
                else
                {
                    this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y / this.game_obj.transform.localScale.y,
                prefab_scale.z);
                }
            else if (this.game_obj.transform.localScale.z != 0)
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y,
                prefab_scale.z / this.game_obj.transform.localScale.z);
            }
            else
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y,
                prefab_scale.z);
            }
        }
        else if (this.game_obj.transform.localScale.y != 0)
            if (this.game_obj.transform.localScale.z != 0)
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                prefab_scale.y / this.game_obj.transform.localScale.y,
                prefab_scale.z / this.game_obj.transform.localScale.z);
            }
            else
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                    prefab_scale.y / this.game_obj.transform.localScale.y,
                    prefab_scale.z);
            }
        else
        {
            if (this.game_obj.transform.localScale.z != 0)
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                    prefab_scale.y,
                    prefab_scale.z / this.game_obj.transform.localScale.z);
            }
            else
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                    prefab_scale.y,
                    prefab_scale.z);
            }
        }

        this.floating_number.GetComponent<TextMesh>().text = this.variable.getValue().ToString();
        /*if (clone == null)
            clone = GameObject.CreatePrimitive(PrimitiveType.Quad); //Instantiate(clone_prefab, position, Quaternion.identity);
        else
            clone.transform.position = new Vector3(position.x, position.y + variable.getValue() / 2f, position.z);*/
        /*updatePosition(position);
        updateDimension(dimensions);
        this.clone.transform.localScale = dimensions;*/ // new Vector3(dimensions.x, variable.getValue(), dimensions.z);
    }

    public override void setValue(dynamic new_val)
    {
        // Faire un try, catch pour le cast du new_int
        int new_int = (int) new_val; // Des que le probleme du type sera regle (j'imagine que "new_int" contient la nouvelle valeur a modifier)
        float seconds = 2f;

        this.variable = new Parser.Integer(new_int);

        this.floating_number.transform.parent = null;
        StartCoroutine(Animation.setValueAnimation(this, 0, new_int, this.dimensions.y, seconds));
    }

    public static GraphicalInteger graphicalIntegerFromInteger(GameObject gameObject, Parser.Integer int_var, Vector3 dimensions, Vector3 position, int representation)
    {
        GraphicalInteger graphInt = gameObject.AddComponent<GraphicalInteger>();
        graphInt.initialize(int_var, gameObject, dimensions, position, representation);
        return graphInt;
    }
}

public class GraphicalFloat : GraphicalObject // on utilisera probablement une classe "nombre" qui fera int et flottant, voir classe abstraite
{

    public override void initialize(Parser.Variable val, GameObject parent_obj, Vector3 dimensions, Vector3 position, int representation) // representation : 0 -> dans une liste, 1 -> un index pointant sur une valeur de liste, 2 -> ...
    {
        this.variable = val as Parser.FloatVar;
        this.name = this.variable.getName();
        this.dimensions = dimensions;
        this.position = position;
        this.representation = representation;
        this.game_obj = Instantiate(Resources.Load<GameObject>("Prefabs/Number_prefab"), Vector3.zero, Quaternion.identity); // new Vector3(position.x, position.y + variable.getValue() / 2f, position.z);
        this.game_obj.transform.parent = parent_obj.transform;
        this.game_obj.transform.localPosition = position + new Vector3(dimensions.x / 2f, 0, 0); // car le redimensionnement d'un objet se fait sur deux directions
        this.game_obj.transform.localScale = dimensions;

        this.floating_number =
            Instantiate(Resources.Load<GameObject>("Prefabs/FloatingNumber"),
            new Vector3(this.game_obj.transform.position.x, parent_obj.transform.position.y,
            this.game_obj.transform.position.z), Quaternion.identity);

        this.floating_number.GetComponent<TextMesh>().text = this.variable.getValue().ToString();
        this.floating_number.transform.SetParent(this.game_obj.transform);
    }

    public override void setValue(dynamic new_val)
    {
        // Faire un try, catch pour le cast du new_float
        float new_float = (float)new_val; // Des que le probleme du type sera regle (j'imagine que "new_float" contient la nouvelle valeur a modifier).
        float seconds = 2f;

        this.variable = new Parser.FloatVar(new_float);

        this.floating_number.transform.parent = null;
        StartCoroutine(Animation.setValueAnimation(this, 1, new_float, this.dimensions.y, seconds));
    }

    public override void drawObject()
    {
        Vector3 prefab_scale = Resources.Load<GameObject>("Prefabs/FloatingNumber").transform.localScale;
        Vector3 prefab_pos = Resources.Load<GameObject>("Prefabs/FloatingNumber").transform.position;

        this.game_obj.transform.localPosition = this.position + new Vector3(this.dimensions.x / 2f, 0, 0); // car le redimensionnement d'un objet se fait sur deux directions
        this.game_obj.transform.localScale = this.dimensions;

        this.floating_number.transform.SetParent(this.game_obj.transform);

        this.floating_number.transform.localPosition = Vector3.zero;
        this.floating_number.transform.position = new Vector3(this.floating_number.transform.position.x,
            this.game_obj.transform.position.y - (this.game_obj.transform.localScale.y / 2f) + floatnumb_y_constant,
            this.floating_number.transform.position.z);

        if (this.game_obj.transform.localScale.x != 0) // Pour eviter le cas de la division par 0
        {
            if (this.game_obj.transform.localScale.y != 0)
                if (this.game_obj.transform.localScale.z != 0)
                {
                    this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y / this.game_obj.transform.localScale.y,
                prefab_scale.z / this.game_obj.transform.localScale.z);
                }
                else 
                {
                    this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y / this.game_obj.transform.localScale.y,
                prefab_scale.z);
                }
            else if (this.game_obj.transform.localScale.z != 0)
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y,
                prefab_scale.z / this.game_obj.transform.localScale.z);
            }
            else
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
                prefab_scale.y,
                prefab_scale.z);
            }
        }
        else if (this.game_obj.transform.localScale.y != 0)
            if (this.game_obj.transform.localScale.z != 0)
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                prefab_scale.y / this.game_obj.transform.localScale.y,
                prefab_scale.z / this.game_obj.transform.localScale.z);
            }
            else
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                    prefab_scale.y / this.game_obj.transform.localScale.y,
                    prefab_scale.z);
            }
        else
        {
            if (this.game_obj.transform.localScale.z != 0)
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                    prefab_scale.y,
                    prefab_scale.z / this.game_obj.transform.localScale.z);
            }
            else
            {
                this.floating_number.transform.localScale = new Vector3(prefab_scale.x,
                    prefab_scale.y,
                    prefab_scale.z);
            }
        }

        this.floating_number.GetComponent<TextMesh>().text = this.variable.getValue().ToString();
    }

    public static GraphicalFloat graphicalFloatFromFloat(GameObject gameObject, Parser.FloatVar float_var, Vector3 dimensions, Vector3 position, int representation)
    {
        
        GraphicalFloat graphFloat = gameObject.AddComponent<GraphicalFloat>();
        graphFloat.initialize(float_var, gameObject, dimensions, position, representation);
        return graphFloat;
    }
}

public class GraphicalBoolean : GraphicalObject // on utilisera probablement une classe "nombre" qui fera int et flottant, voir classe abstraite
{
    public override void initialize(Parser.Variable val, GameObject parent_obj,Vector3 dimensions, Vector3 position, int representation) // representation : 0 -> dans une liste, 1 -> un index pointant sur une valeur de liste, 2 -> ...
    {
        this.variable = val as Parser.BooleanVar;
        this.name = this.variable.getName();
        this.dimensions = dimensions;
        this.position = position;
        this.representation = representation;
        this.game_obj = Instantiate(Resources.Load<GameObject>("Prefabs/BooleanPrefab"), Vector3.zero, Quaternion.identity); // new Vector3(position.x, position.y + variable.getValue() / 2f, position.z
        this.game_obj.transform.parent = parent_obj.transform;
        this.game_obj.transform.localPosition = position + new Vector3(dimensions.x / 2f, 0, 0); // car le redimensionnement d'un objet se fait sur deux directions
        this.game_obj.transform.localScale = dimensions;

        this.floating_number =
            Instantiate(Resources.Load<GameObject>("Prefabs/FloatingNumber"),
            new Vector3(this.game_obj.transform.position.x, parent_obj.transform.position.y,
            this.game_obj.transform.position.z), Quaternion.identity);

        this.floating_number.GetComponent<TextMesh>().text = this.variable.getValue().ToString();
        this.floating_number.transform.SetParent(this.game_obj.transform);

        if (variable.getValue())
            this.game_obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/true_material");
        else
            this.game_obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/false_material");
    }

    public override void setValue(dynamic new_val)
    {
        // Faire un try, catch pour le cast du new_bool
        bool new_bool = (bool)new_val; // Des que le probleme du type sera regle (j'imagine que "new_bool" contient la nouvelle valeur a modifier).
        float seconds = 2f;

        bool act_val = this.variable.getValue();
        this.variable = new Parser.BooleanVar(new_bool);

        this.floating_number.transform.parent = null;
        StartCoroutine(Animation.setValueAnimation(this, 2, new_bool, act_val, seconds));
    }

    public override void drawObject()
    {
        Vector3 prefab_scale = Resources.Load<GameObject>("Prefabs/FloatingNumber").transform.localScale;
        Vector3 prefab_pos = Resources.Load<GameObject>("Prefabs/FloatingNumber").transform.position;

        this.game_obj.transform.localPosition = this.position + new Vector3(this.dimensions.x / 2f, 0, 0); // car le redimensionnement d'un objet se fait sur deux directions
        this.game_obj.transform.localScale = this.dimensions;

        if (variable.getValue())
            this.game_obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/true_material");
        else
            this.game_obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/false_material");

        this.floating_number.transform.SetParent(this.game_obj.transform);

        this.floating_number.transform.localPosition = Vector3.zero;
        this.floating_number.transform.position = new Vector3(this.floating_number.transform.position.x,
            this.game_obj.transform.position.y - (this.game_obj.transform.localScale.y / 2f) + floatnumb_y_constant,
            this.floating_number.transform.position.z);

        this.floating_number.transform.localScale = new Vector3(prefab_scale.x / this.game_obj.transform.localScale.x,
            prefab_scale.y / this.game_obj.transform.localScale.y,
            prefab_scale.z / this.game_obj.transform.localScale.z);
        this.floating_number.GetComponent<TextMesh>().text = this.variable.getValue().ToString();

    }

    public static GraphicalBoolean graphicalBooleanFromBoolean(GameObject gameObject, Parser.BooleanVar boolean_var, Vector3 dimensions, Vector3 position, int representation)
    {
        GraphicalBoolean graphBoolean = gameObject.AddComponent<GraphicalBoolean>();
        graphBoolean.initialize(boolean_var, gameObject, dimensions, position, representation);
        return graphBoolean;
    }
}

public class GraphicalList : GraphicalObject
{
    public Parser.DynamicList list;
    public List<GraphicalObject> graph_list;
    public float max_value;

    public override void initialize(Parser.Variable list, GameObject parent_obj, Vector3 dimensions, Vector3 position, int representation)
    {
        this.list = list as Parser.DynamicList;
        this.game_obj = new GameObject(list.getName());
        this.game_obj.transform.parent = parent_obj.transform;
        this.game_obj.transform.localPosition = position;
        //this.game_obj.transform.localScale = dimensions;
        this.name = list.getName();
        this.dimensions = dimensions;
        this.position = position;
        this.representation = representation;
        calkGraphList();
    }

    public void calkGraphList()
    {
        this.graph_list = new List<GraphicalObject>();
        int N = list.length().getValue(); // == list.getValue().Count
        float element_width = (dimensions.x / N) - Global.space_between_elements;

        foreach (Parser.Variable variable in this.list.getValue())
        {
            Vector3 elem_dim = new Vector3(element_width, 10f, this.dimensions.z);
            Vector3 elem_pos = new Vector3(0, 0, 0);

            GraphicalObject obj_inst = GraphicalObject.graphicalObjectFromVariable(this.game_obj, variable, elem_dim, elem_pos, 0);
            this.graph_list.Add(obj_inst);
        }

        this.drawObject();
    }

    /*
        public List<GameObject> calkObjList()
        {
            List<GameObject> game_obj_list = new List<GameObject>();
            for (int i = 0; i < this.graph_list.Count; i++)
            {
                GameObject clone_prefab = Resources.Load<GameObject>("Prefabs/Quad_prefab");
                GameObject clone = Instantiate(clone_prefab, position + new Vector3(i, graph_list[i].position.y / 2f, graph_list[i].position.z), Quaternion.identity);
                //clone.transform.position = position + new Vector3(i, obj_value / 2f, 0);
                clone.transform.localScale = new Vector3(graph_list[i].dimensions.x, graph_list[i].dimensions.y, 1);
                game_obj_list.Add(clone);
            }
            return game_obj_list;
        } */

    public void insert(Parser.Integer index, Parser.Variable value)
    {
        float time_animation = 3f;

        //
        //this.list.insertValue(value, index);
        //

        int N = this.list.length().getValue(); // == list.getValue().Count
        float element_width = (this.dimensions.x / N) - Global.space_between_elements;

        Vector3 elem_dim = new Vector3(element_width, 10f, this.dimensions.z);
        Vector3 elem_pos = new Vector3(0, 0, 0);

        GraphicalObject obj_inst = GraphicalObject.graphicalObjectFromVariable(this.game_obj, value, elem_dim, elem_pos, 0);
        this.graph_list.Add(obj_inst);

        this.drawObject();
        obj_inst.game_obj.SetActive(false);

        GameObject[] obj_list = new GameObject[list.length().getValue() - index.getValue() - 1];
        for (int i = index.getValue(), u = 0; i < (list.length().getValue() - 1); i++, u++)
        {
            obj_list[u] = graph_list[i].game_obj; // game_obj_list[i];
        }

        int element_index = index.getValue(); // La coroutine n'accepte pas le fait de mettre directement "index.getValue()"
        // La composante x de "translation_vect" represente la place qu'occupe un element dans la nouvelle liste (c-a-d le nouvel element est ajoute)
        Vector3 translation_vect = new Vector3(this.dimensions.x / this.list.length().getValue(), 0, 0);

        StartCoroutine(Animation.insertAnimation(obj_list, element_index, translation_vect, time_animation, 1, this));
    }

    public void remove(Parser.Integer index)
    {
        int i = index.getValue();
        float time_animation = 3f;

        GameObject[] obj_list = new GameObject[list.length().getValue() - index.getValue()];
        for (int j = index.getValue() + 1, u = 0; j < (list.length().getValue() + 1); j++, u++)
        {
            obj_list[u] = graph_list[j].game_obj;
        }

        Vector3 translation_vect = new Vector3(graph_list[i].dimensions.x + Global.space_between_elements, 0, 0);

        StartCoroutine(Animation.removeAnimation(obj_list, i, (-1) * translation_vect, time_animation, 0, this));
    }

    public void swap(Parser.Integer index1, Parser.Integer index2)
    {
        GraphicalObject graph_obj1 = graph_list[index1.getValue()];
        GameObject game_obj1 = graph_obj1.game_obj;

        GraphicalObject graph_obj2 = graph_list[index2.getValue()];
        GameObject game_obj2 = graph_obj2.game_obj;
        
        graph_list[index1.getValue()] = graph_obj2;
        graph_list[index2.getValue()] = graph_obj1;

        float time_before_animation = Time.realtimeSinceStartup;

        StartCoroutine(Animation.swapAnimation(game_obj1, game_obj2, 3f, this));
    }

    public override void setValue(dynamic new_val)
    {
        throw new NotImplementedException();
        /*
        // Faire un try, catch pour le cast du new_list
        List<dynamic> new_list = (List<dynamic>)new_val; // Des que le probleme du type sera regle (j'imagine que "new_list" contient la nouvelle valeur a modifier).
        float seconds = 2f;

        bool act_val = this.variable.getValue();
        this.variable = new Parser.BooleanVar(new_bool);

        this.floating_number.transform.parent = null;
        StartCoroutine(Animation.setValueAnimation(this, 2, new_bool, act_val, seconds));
        // "list" et "graph_list" devront etre update
        */
    }

    public void listAt(List<int> indexes)
    {
        int indexes_length = indexes.Count;
        if (indexes_length > 0)
        {
            Parser.Variable replaced_val = this.list.atIndex(new Parser.Integer(indexes[0]));
            GraphicalObject graph_obj_parent = this;
            GraphicalObject graph_obj_to_replace = this.graph_list[indexes[0]];
            int i = 1;

            while (i < indexes_length && replaced_val is Parser.DynamicList)
            {
                replaced_val = (replaced_val as Parser.DynamicList).atIndex(new Parser.Integer(indexes[i]));
                graph_obj_parent = (graph_obj_parent as GraphicalList).graph_list[indexes[i - 1]];
                graph_obj_to_replace = (graph_obj_to_replace as GraphicalList).graph_list[indexes[i]];
                i += 1;
            }

            graph_obj_to_replace.setValue(replaced_val.getValue());
            // probleme pour la dimension en y des "GraphicalObject" qui n'est pas la meme que sa variable
            // graph_obj_to_replace.setValue(new Tuple<object, GraphicalList>(replaced_val.getValue(), (GraphicalList)graph_obj_parent));
        }
        //else
        //Debug.Log("THERE IS NO INDEXES");
    }

    public void listMaxValue()
    {
        int length = list.length().getValue();

        for (int i = 0; i < length; i++)
        {
            Parser.Variable val = list.atIndex(new Parser.Integer(i));

            if (val is Parser.Integer)
            {
                int int_val = val.getValue();
                if (int_val > max_value)
                    max_value = int_val;
            }

            else if (val is Parser.FloatVar)
            {
                float float_val = val.getValue();
                if (float_val > max_value)
                    max_value = float_val;
            }

        }
    }

    public override void drawObject()
    {
        listMaxValue(); // Updates max_value (only for integers and floats)
        int length = list.length().getValue(); // longueur de la "graph_list" normalement (pas de la Parser.Dynamic list)
        float element_width = dimensions.x / length;
        this.game_obj.transform.localPosition = this.position;

        for (int i = 0; i < length; i++)
        {
            GraphicalObject graph_obj = graph_list[i];

            if (graph_obj is GraphicalInteger || graph_obj is GraphicalFloat)
            {
                float val = graph_obj.getVariable().getValue();
                if (val == 0)
                    graph_obj.dimensions.y = 0.000000000001f / max_value * 10; // Trouver une solution pour le cas de la valeur 0 (le floatnumber disparait si dim = vecteur nul)
                else
                    graph_obj.dimensions.y = graph_obj.getVariable().getValue() / max_value * 10;
            }
            else if (graph_obj is GraphicalBoolean)
            {
                graph_obj.dimensions.y = 10f;
            }

            if (graph_obj is GraphicalList)
                graph_obj.position.y = 0;
            else
            {
                if ((graph_obj is GraphicalInteger || graph_obj is GraphicalFloat) && graph_obj.getVariable().getValue() == 0)
                    graph_obj.position.y = 1;
                else
                    graph_obj.position.y = graph_obj.dimensions.y / 2f; // Cette ligne est a revoir (child)
            }

            graph_obj.dimensions = new Vector3(element_width - Global.space_between_elements, graph_obj.dimensions.y, graph_obj.dimensions.z);
            graph_obj.position = new Vector3(element_width * i, graph_obj.position.y, 0);
            graph_list[i].drawObject();
            /*
                string name = list.atIndex(new Parser.Integer(i)).getName();
                int obj_value = list.atIndex(new Parser.Integer(i)).getValue();
                if (list_dict.ContainsKey(name))
                {
                    GameObject clone = list_dict[name];
                    clone.transform.position = position + new Vector3(i, obj_value / 2f, 0);
                    clone.transform.localScale = new Vector3(0.9f, obj_value, 1);
                }
                else
                {
                    GameObject clone_prefab = Resources.Load<GameObject>("Prefabs/Quad_prefab");
                    GameObject clone = Instantiate(clone_prefab, position + new Vector3(i, obj_value / 2f, 0), Quaternion.identity);
                    //clone.transform.position = position + new Vector3(i, obj_value / 2f, 0);
                    clone.transform.localScale = new Vector3(0.9f, obj_value, 1);

                    GameObject floating_numb_prefab = Resources.Load<GameObject>("Prefabs/FloatingNumber");
                    GameObject floating_number = Instantiate(floating_numb_prefab, new Vector3(clone.transform.position.x, position.y + 0.9f, clone.transform.position.z), Quaternion.identity);
                    floating_number.GetComponent<TextMesh>().text = obj_value.ToString();
                    floating_number.transform.SetParent(clone.transform);

                    list_dict.Add(name, clone);
                }
            */
        }
    }

    public static GraphicalList graphicalListFromDynamicList(GameObject gameObject, Parser.DynamicList list_var, Vector3 dimensions, Vector3 position, int representation)
    {
        GraphicalList graphList = gameObject.AddComponent<GraphicalList>();
        graphList.initialize(list_var, gameObject, dimensions, position, representation);
        return graphList;
    }

    /*
    public override void drawObject()
    {
        int length = list.length().getValue();

        for (int i = 0; i < length; i++)
        {
            string name = list.atIndex(new Parser.Integer(i)).getName();
            Parser.Variable obj_value = list.atIndex(new Parser.Integer(i)).getValue();
            if (list_dictionary.ContainsKey(name))
            {
                GraphicalObject clone = list_dictionary[name];

                clone.position = position + new Vector3(i, 0, 0);
                clone.dimensions = new Vector3(0.9f, 0, 1);
                clone.setValue(obj_value);
            }
            else
            {
                GraphicalObject

                GameObject clone_prefab = Resources.Load<GameObject>("Prefabs/Quad_prefab");
                GameObject clone = Instantiate(clone_prefab, position + new Vector3(i, obj_value / 2f, 0), Quaternion.identity);
                //clone.transform.position = position + new Vector3(i, obj_value / 2f, 0);
                clone.transform.localScale = new Vector3(0.9f, obj_value, 1);

                GameObject floating_numb_prefab = Resources.Load<GameObject>("Prefabs/FloatingNumber");
                GameObject floating_number = Instantiate(floating_numb_prefab, new Vector3(clone.transform.position.x, position.y + 0.9f, clone.transform.position.z), Quaternion.identity);
                floating_number.GetComponent<TextMesh>().text = obj_value.ToString();
                floating_number.transform.SetParent(clone.transform);

                list_dict.Add(name, clone);
            }
        }
    }
    */

}
