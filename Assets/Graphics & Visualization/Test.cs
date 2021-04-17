using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Test : MonoBehaviour
{
    GraphicalList graphList;

    public void Start()
    {
        /*graphList = gameObject.AddComponent<GraphicalList>();

        List<int> testList = new List<int>() { 5, 4, 3, 2, 1 };

        Parser.DynamicList list_test = Parser.VariableUtils.createDynamicList(testList);

        (list_test.atIndex(new Parser.Integer(0))).setName("int1");
        (list_test.atIndex(new Parser.Integer(1))).setName("int2");
        (list_test.atIndex(new Parser.Integer(2))).setName("int3");
        (list_test.atIndex(new Parser.Integer(3))).setName("int4");
        (list_test.atIndex(new Parser.Integer(4))).setName("int5");

        Dictionary<string, GameObject> list_dict_test = new Dictionary<string, GameObject>();

        graphList.list_dict = list_dict_test;
        graphList.list = list_test;

        graphList.drawObject(); */// To update the dictionary

        /* INSERT DEMO
        Parser.Integer insert_numb = new Parser.Integer(5);
        insert_numb.setName("int6");

        graphList.insert(new Parser.Integer(2), insert_numb); // tester avec les index qui se trouvent a l'extremite
        */

        /* REMOVE DEMO
        graphList.remove(new Parser.Integer(1));
        */

        ///* SWAP DEMO
        //graphList.swap(new Parser.Integer(1), new Parser.Integer(4));
        //StartCoroutine(graphList.swap(new Parser.Integer(2), new Parser.Integer(3)));
        //*/

        //Debug.Log("Animation finished!");
    }
    /*public void Update()
    {


    }*/



    /*
    public int testFunction(Parser.DynamicList list, Parser.Integer index1, Parser.Integer index2)
    {
        graph
        return 0;
    }
    */
}
