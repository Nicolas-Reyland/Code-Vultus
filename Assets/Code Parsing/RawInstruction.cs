﻿using System.Collections.Generic;
using System.Linq;

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable PossibleNullReferenceException
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Parser
{
    /// <summary>
    /// <see cref="RawInstruction"/>s are used to build a sketch of the given pseudo-code Algorithm.
    /// They only store the instructions as strings. One <see cref="RawInstruction"/> by base-line.
    /// A base-line is a line that is at the root of the algorithm. Lines that come
    /// after an if statement or after a for loop are not base-lines.
    /// They are stored in the corresponding <see cref="RawInstruction"/>'s <see cref="_sub_instr_list"/>, and if <see cref="_is_nested"/> is true.
    /// The <see cref="RawInstruction"/>s are the first approach to parsing a pseudo-code program
    /// into an executable <see cref="Algorithm"/> object.
    /// <para/>List of the attributes:
    /// <para/>* <see cref="_instr"/> : string
    /// <para/>* <see cref="_is_nested"/> : bool
    /// <para/>* <see cref="_sub_instr_list"/> : List(RawInstruction)
    /// </summary>
    public class RawInstruction
    {
        /// <summary>
        /// The line of pseudo-code represented by the <see cref="RawInstruction"/>
        /// </summary>
        private readonly string _instr;

        /// <summary>
        /// Line index of this RawInstruction in the Source Code
        /// </summary>
        private readonly int _line_index;
        
        /// <summary>
        /// Nested <see cref="RawInstruction"/>s are for-loops, if-statements, etc.
        /// </summary>
        private bool _is_nested;
        
        /// <summary>
        /// If nested (<seealso cref="_is_nested"/>), holds the nested instructions that follow the instruction.
        /// </summary>
        private List<RawInstruction> _sub_instr_list;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instr"> instruction</param>
        /// <param name="line_index"> index of the corresponding line in the src code</param>
        internal RawInstruction(string instr, int line_index)
        {
            _instr = instr;
            _line_index = line_index;
        }

        /// <summary>
        /// Print the instruction and its <see cref="_sub_instr_list"/> if there are any.
        /// Indents the code according to the nested-depth to increase readability
        /// </summary>
        /// <param name="depth"> indentation level</param>
        public void prettyPrint(uint depth = 0)
        {
            for (uint i = 0; i < depth; i++) { Global.stdoutWrite("\t"); }
            Global.stdoutWriteLine(this._instr);
            
            if (!_is_nested) return;
            foreach (RawInstruction sub_instr in _sub_instr_list)
            {
                sub_instr.prettyPrint(depth + 1);
            }
        }

        /// <summary>
        /// Transform a string list into a <see cref="RawInstruction"/>. Takes into account
        /// nested instructions. This method creates the base algorithm structure, using
        /// the <see cref="_is_nested"/> and <see cref="_sub_instr_list"/> attributes.
        /// </summary>
        /// <param name="lines"> list of strings</param>
        /// <returns> list of RawInstructions</returns>
        public static List<RawInstruction> code2RawInstructions(Dictionary<int, string> lines)
        {
            Context.setStatus(Context.StatusEnum.building_raw_instructions);
            Context.setInfo(lines);
            
            int line_index = 0;
            List<RawInstruction> instructions = new List<RawInstruction> ();

            while (line_index < lines.Count) // using while loop bc index will be modified
            {
                string line = lines.Values.ElementAt(line_index);
                int real_line_index = lines.Keys.ElementAt(line_index);
                Debugging.print("doing line ", line);
                if (line.StartsWith("#")) // macro preprocessor line
                {
                    line_index++;
                    continue;
                }

                RawInstruction instr = new RawInstruction (line, real_line_index);

                foreach (var pair in Global.nested_instruction_flags.Where(flag => line.StartsWith(flag.Key + " ")))
                {
                    Debugging.print("FOUND " + pair.Key);
                    instr._is_nested = true;
                    int end_index =
                        StringUtils.findCorrespondingElementIndex(lines.Select(pair_ => pair_.Value).ToList(), line_index + 1, pair.Key, pair.Value);
                    Dictionary<int, string> sub_lines = new Dictionary<int, string>();
                    List<int> picked =
                        lines.Select(pair_ => pair_.Key).ToList().GetRange(line_index + 1,
                            end_index - line_index - 1);
                    foreach (int i in picked)
                    {
                        sub_lines.Add(i, lines[i]);
                    }

                    instr._sub_instr_list = code2RawInstructions(sub_lines);
                    Debugging.print(line_index, " - ", end_index);
                    line_index = end_index;
                }

                instructions.Add(instr);
                line_index++;
            }

            Context.reset();
            return instructions;
        }

        /// <summary>
        /// Calls <see cref="RawInstruction.rawInstr2Instr"/> on itself
        /// </summary>
        /// <returns> corresponding <see cref="Instruction"/></returns>
        public Instruction toInstr()
        {
            return rawInstr2Instr(_line_index, this);
        }

        /// <summary>
        /// Transforms a <see cref="RawInstruction"/> into an <see cref="Instruction"/>.
        /// <para/>The order of operations is:
        /// <para/>* variable declaration
        /// <para/>* variable modification
        /// <para/>* for loop
        /// <para/>* while loop
        /// <para/>* if statement
        /// <para/>* void function call
        /// </summary>
        /// <param name="line_index"> index of the line in the purged source code</param>
        /// <param name="raw_instr"> a <see cref="RawInstruction"/> to convert</param>
        /// <returns> the corresponding <see cref="Instruction"/></returns>
        /// <exception cref="AquilaExceptions.SyntaxExceptions.SyntaxError"> Invalid syntax</exception>
        private static Instruction rawInstr2Instr(int line_index, RawInstruction raw_instr)
        {
            /* Order of operations:
             * tracing
             * declaration
             * variable assignment
             * function definition
             * for loop
             * while loop
             * if statement
             * void function call
             */
            
            Debugging.print("from raw instr to instr: \"", raw_instr._instr, "\"");

            // split instruction
            List<string> instr = StringUtils.splitStringKeepingStructureIntegrity(raw_instr._instr, ' ', Global.base_delimiters);

            Debugging.print("trace ?");
            // variable tracing
            if (instr[0] == "trace")
            {
                if (Global.getSetting("auto trace"))
                    Debugging.print("\"trace\" instruction, but \"trace all\" is set to true ?");
                
                List<Expression> traced_vars = new List<Expression>();
                for (int i = 1; i < instr.Count; i++)
                {
                    traced_vars.Add(new Expression(instr[i]));
                }

                return new Tracing(line_index, traced_vars);
            }

            Debugging.print("decl ?");
            // variable declaration
            if (instr.Contains("decl"))
            {
                // "decl type name" or "global safe const decl type name value"
                if (instr.Count < 3 || instr.Count > 7)
                {
                    throw new AquilaExceptions.SyntaxExceptions.SyntaxError($"Word count mismatch in \"{raw_instr._instr}\" for declaration");
                }
                
                // declaration modes
                bool safe_mode = false,
                    overwrite = false,
                    constant = false,
                    global = false;
                while (instr[0] != "decl")
                {
                    switch (instr[0])
                    {
                        case "safe":
                            safe_mode = true;
                            Debugging.print("safe mode !");
                            break;
                        case "overwrite":
                            overwrite = true;
                            Debugging.print("overwrite !");
                            break;
                        case "const":
                            constant = true;
                            Debugging.print("const !");
                            break;
                        case "global":
                            global = true;
                            Debugging.print("global !");
                            break;
                        default:
                            throw new AquilaExceptions.UnknownKeywordError($"Unknown keyword in declaration: \"{instr[0]}\"");
                    }

                    instr.RemoveAt(0);
                }
                
                // all types
                string[] type_list = {"int", "float", "bool", "list"};

                // decl type name value
                if (instr.Count == 4) Debugging.assert(type_list.Contains(instr[1]), new AquilaExceptions.UnknownTypeError($"The type \"{instr[1]}\" is not recognized"));
                else if (instr[1] == "auto") throw new AquilaExceptions.InvalidTypeError("You cannot declare an empty variable using type \"auto\""); // cannot "decl auto var_name"

                // if instr[1] is type
                if (type_list.Contains(instr[1]))
                {
                    Expression default_value = Global.default_values_by_var_type[instr[1]];
                    return new Declaration(line_index,
                            instr[2],
                            instr.Count < 4 ? default_value : new Expression(instr[3]),
                            instr[1],
                            instr.Count > 3,
                            safe_mode,
                            overwrite,
                            constant,
                            global);
                }

                // case is: "decl var_name value"
                string var_name = instr[1];
                string var_value = instr[2];

                return new Declaration(line_index, var_name, new Expression(var_value), "auto", true, safe_mode, overwrite, constant, global);
            }

            Debugging.print("assignment ?");
            // variable assignment
            if (instr.Count > 1 && instr[1].EndsWith("=") && (instr[0][0] == '$' || instr[0].Contains("(")))
            {
                Debugging.assert(instr.Count > 2); // syntax ?unfinished line?
                string var_designation = instr[0];
                string equal_sign = instr[1];
                instr.RemoveAt(0); // remove "$name"
                instr.RemoveAt(0); // remove "{op?}="
                // reunite all on the right side of the "=" sign
                string assignment_string = StringUtils.reuniteBySymbol(instr);
                // custom operator in assignment
                if (equal_sign.Length != 1)
                {
                    Debugging.print("Custom operator detected: ", equal_sign);
                    Debugging.assert(equal_sign.Length == 2);
                    assignment_string = $"{var_designation} {equal_sign[0]} ({assignment_string})";
                }
                // get the Expresion
                Expression assignment = new Expression(assignment_string);
                return new Assignment(line_index, var_designation, assignment);
            }
            // increment || decrement
            if (instr.Count == 2 && (instr[1] == "++" || instr[1] == "--"))
            {
                Debugging.print("Increment or Decrement detected");
                return new Assignment(line_index, instr[0], new Expression($"{instr[0]} {instr[1][0]} 1"));
            }

            Debugging.print("function definition ?");
            if (instr[0] == "function") {
                Debugging.assert(raw_instr._is_nested); // syntax???
                Debugging.assert(instr.Count == 3 || instr.Count == 4); // "function" "type" ("keyword"?) "name(args)"

                Function func = Functions.readFunction(raw_instr._instr, raw_instr._sub_instr_list);
                return new FunctionDef(line_index, func);
            }
            
            Debugging.print("for loop ?");
            // for loop
            if (instr[0] == "for")
            {
                Debugging.assert(raw_instr._is_nested); // syntax???
                Debugging.assert(instr[1].StartsWith("(") && instr[1].EndsWith(")")); // syntax
                List<string> sub_instr =
                    StringUtils.splitStringKeepingStructureIntegrity(instr[1].Substring(1, instr[1].Length - 2), ';', Global.base_delimiters);
                sub_instr = StringUtils.purgeLines(sub_instr);
                Debugging.print(sub_instr);
                Debugging.assert(sub_instr.Count == 3); // syntax

                // start
                Instruction start = new RawInstruction(sub_instr[0], raw_instr._line_index).toInstr();

                // stop
                Expression condition = new Expression(sub_instr[1]);
                
                // step
                Instruction step = new RawInstruction(sub_instr[2], raw_instr._line_index).toInstr();

                // instr
                List<Instruction> loop_instructions = new List<Instruction>();
                int add_index = 0;
                foreach (RawInstruction loop_instr in raw_instr._sub_instr_list)
                {
                    loop_instructions.Add(rawInstr2Instr(line_index + ++add_index, loop_instr));
                }

                return new ForLoop(line_index, start, condition, step, loop_instructions);

            }

            Debugging.print("while loop ?");
            // while loop
            if (instr[0] == "while")
            {
                // syntax check
                Debugging.assert(instr.Count == 2); // syntax

                // condition expression
                Expression condition = new Expression(instr[1]);
                
                // instr
                List<Instruction> loop_instructions = new List<Instruction>();
                int add_index = 0;
                foreach (RawInstruction loop_instr in raw_instr._sub_instr_list)
                {
                    loop_instructions.Add(rawInstr2Instr(line_index + ++add_index, loop_instr));
                }

                return new WhileLoop(line_index, condition, loop_instructions);
            }
            
            Debugging.print("if statement ?");
            // if statement
            if (instr[0] == "if")
            {
                // syntax check
                Debugging.assert(instr.Count == 2); // syntax
                
                // condition expression
                Expression condition = new Expression(instr[1]);
                
                // instr
                List<Instruction> if_instructions = new List<Instruction>();
                List<Instruction> else_instructions = new List<Instruction>();
                bool if_section = true;
                int add_index = 0;
                foreach (RawInstruction loop_instr in raw_instr._sub_instr_list)
                {
                    add_index++;
                    if (if_section)
                    {
                        if (loop_instr._instr == "else")
                        {
                            if_section = false;
                            continue;
                        }
                        if_instructions.Add(rawInstr2Instr(line_index + add_index, loop_instr));
                    }
                    else
                    {
                        else_instructions.Add(rawInstr2Instr(line_index + add_index, loop_instr));
                    }
                }

                return new IfCondition(line_index, condition, if_instructions, else_instructions);
            }

            Debugging.print("function call ?");
            // function call with spaces between function name and parenthesis
            if (instr.Count == 2 && instr[1].StartsWith("("))
            {
                Debugging.print("function call with space between name and first parenthesis. Merging ", instr[0], " and ", instr[1]);
                instr[0] += instr[1];
                instr.RemoveAt(1);
            }
            // void function call (no return value, or return value not used)
            if (instr[0].Contains('('))
            {
                // syntax checks
                Debugging.assert(instr.Count == 1); // syntax
                Debugging.assert(instr[0][instr[0].Length - 1] == ')'); // syntax
                
                // function name
                string function_name = instr[0].Split('(')[0]; // extract function name
                if (Global.getSetting("check function existence before runtime")) Functions.assertFunctionExists(function_name); // assert function exists
                Debugging.print("expr_string for function call ", instr[0]);
                // extract args
                string exprs = instr[0].Substring(function_name.Length + 1); // + 1 : '('
                exprs = exprs.Substring(0, exprs.Length - 1); // ')'
                List<string> arg_expr_str = StringUtils.splitStringKeepingStructureIntegrity(exprs, ',', Global.base_delimiters);

                // no args ?
                if (arg_expr_str.Count == 1 && StringUtils.normalizeWhiteSpaces(arg_expr_str[0]) == "")
                {
                    return new VoidFunctionCall(line_index, function_name);
                }

                // ReSharper disable once SuggestVarOrType_Elsewhere
                object[] args = arg_expr_str.Select(x => (object) new Expression(x)).ToArray();

                return new VoidFunctionCall(line_index, function_name, args);
            }
            
            // try using this as a function ?
            if (instr.Count == 1 && Functions.functionExists(instr[0]))
            {
                Debugging.print($"Call the function \"{instr[0]}\" with no parameters");
                return new VoidFunctionCall(line_index, instr[0]);
            }

            Debugging.print("unrecognized line: \"", raw_instr._instr, "\"");
            throw new AquilaExceptions.SyntaxExceptions.SyntaxError($"Unknown syntax \"{raw_instr._instr}\"");
        }
    }
}