using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public GameObject game_object;

    // Start is called before the first frame update
    void Start()
    {
        // Test the CSharp Translator
        /*
        string input_cs_file = @"C:\Users\Nicolas\Documents\EPITA\Code Vultus\Iris\csharp merge sort.cs";
        string output_cs_file = @"C:\Users\Nicolas\Documents\EPITA\Code Vultus\Iris\translated merge sort from cs.aq";
        string cs_output = Translator.CSharpTranslator.translateCSharp(input_cs_file, output_cs_file);
        Debug.Log("C# Translator output:\n" + cs_output);

        string input_py_file = @"C:\Users\Nicolas\Documents\EPITA\Code Vultus\scratch\Python2Aquila\merge sort in python.py";
        string output_py_file = @"C:\Users\Nicolas\Documents\EPITA\Code Vultus\scratch\Python2Aquila\translated merge sort from py.aq";
        string py_output = Translator.PythonTranslator.translatePython(input_py_file, output_py_file);
        Debug.Log("Python Translator output:\n" + py_output);
        */

        // Start the Animations
        StartCoroutine(testOfAll());
    }

    public IEnumerator testOfAll()
    {
        // we don't want any Console.WriteLines
        Parser.Global.setSetting("redirect debug stout & stderr", true);
        Parser.Global.setStdout(new System.IO.StreamWriter("log.log"));
        Parser.Global.setSetting("debug", true);
        Parser.Global.setSetting("trace debug", true);
        Parser.Global.setSetting("translator debug", false);
        // automatically trace all variables
        //Parser.Global.setSetting("auto trace", true);
        // Initialize Variables (& Context)
        Parser.Global.initVariables();

        // Tracing
        Parser.Global.tracer_update_handler_function = PostMortem.AddAnimation;
        Parser.Global.func_tracers.Add(new Parser.FuncTracer("swap"));
        Parser.Global.func_tracers.Add(new Parser.FuncTracer("list_at"));

        Parser.Algorithm algo = Parser.Interpreter.algorithmFromSrcCode("Assets/Code Parsing/Aquila scripts/test.aq");

        Global.game_object = game_object;

        Debug.Log("running algorithm");
        Parser.Variable result_var = algo.run();
        Debug.Log("finished running");

        Parser.Global.closeStdout();
        Debug.Log("result : " + result_var.ToString());

        yield return null;
    }
}
