﻿using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable PossibleNullReferenceException
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

// ReSharper disable UseIndexFromEndExpression
// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable MergeConditionalExpression

namespace Parser
{
    /// <summary>
    /// An <see cref="Expression"/> is basically a value. It can be an <see cref="Integer"/>,
    /// a <see cref="DynamicList"/> or a <see cref="BooleanVar"/>.
    /// <para>The only exception is the function call, which is a value, but it can modify values if
    /// they are passed through by reference</para>
    /// <see cref="Expression"/>s are often represented as a literal arithmetical or logical expressions
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// The literal expresion, as a string (e.g. "$x + 5")
        /// </summary>
        public readonly string expr;

        /// <summary>
        /// Create a new <see cref="Expression"/>
        /// </summary>
        /// <param name="expr"> Expression string</param>
        public Expression(string expr)
        {
            this.expr = StringUtils.normalizeWhiteSpaces(expr);
        }

        /// <summary>
        /// Call <see cref="parse"/> on <see cref="expr"/>
        /// </summary>
        /// <returns> Arithmetic or logic value of <see cref="expr"/></returns>
        public Variable evaluate() => parse(expr);

        /// <summary>
        /// Takes an arithmetical or logical expression and returns the corresponding variable
        /// <para/>Examples:
        /// <para/>* "5 + 6" : returns Integer (11)
        /// <para/>* "$l[5 * (1 - $i)]" : returns the elements at index 5*(1-i) in the list "l"
        /// <para/>* "$l" : returns the list variable l
        /// </summary>
        /// <param name="expr_string"> expression to parse</param>
        /// <returns> Variable object containing the value of the evaluated expression value (at time t)</returns>
        public static Variable parse(string expr_string)
        {
            /* Order of operations:
             * checking expression string integrity
             * raw dynamic list
             * clean redundant symbols
             * raw integer value
             * raw boolean value (not done yet)
             * raw float value (not done yet)
             * mathematical or logical operation
             * function call
             * variable access (e.g. $name or in list by index)
             */
            
            // clean expression
            expr_string = StringUtils.normalizeWhiteSpaces(expr_string);

            Debugging.print("input expression: " + expr_string);

            // matching parentheses & brackets
            Debugging.assert(StringUtils.checkMatchingDelimiters(expr_string, '(', ')'));
            Debugging.assert(StringUtils.checkMatchingDelimiters(expr_string, '[', ']'));
            expr_string = StringUtils.removeRedundantMatchingDelimiters(expr_string, '(', ')');

            Debugging.print("dynamic list ?");
            // dynamic list
            try
            {
                return Parser.string2DynamicList(expr_string);
            }
            catch
            {
                //
            }
            
            // now that lists are over, check for redundant brackets
            expr_string = StringUtils.removeRedundantMatchingDelimiters(expr_string, '[', ']');

            if (expr_string == null) throw new AquilaExceptions.SyntaxExceptions.SyntaxError("Null Expression");

            Debugging.assert(expr_string != ""); //! NullValue here, instead of Exception

            Debugging.print("int ?");
            // try evaluating expression as an integer
            if (Int32.TryParse(expr_string, out int int_value))
            {
                return new Integer(int_value, true);
            }

            Debugging.print("bool ?");
            // try evaluating expression as a boolean
            if (expr_string == "true")
            {
                return new BooleanVar(true, true);
            }
            if (expr_string == "false")
            {
                return new BooleanVar(false, true);
            }
            
            Debugging.print("float ?");
            // try evaluating expression as float
            if (float.TryParse(expr_string, out float float_value))
            {
                Debugging.print("french/classic float");
                return new FloatVar(float_value, true);
            }
            if (float.TryParse(expr_string.Replace('.', ','), out float_value))
            {
                Debugging.print("normalized float");
                return new FloatVar(float_value, true);
            }
            if (expr_string.EndsWith("f") &&
                float.TryParse(expr_string.Substring(0, expr_string.Length - 1), out float_value))
            {
                Debugging.print("f-float");
                return new FloatVar(float_value, true);
            }
            if (expr_string.EndsWith("f") &&
                float.TryParse(expr_string.Replace('.', ',').Substring(0, expr_string.Length - 1), out float_value))
            {
                Debugging.print("f-float");
                return new FloatVar(float_value, true);
            }
            
            Debugging.print("checking for negative expression");
            // special step: check for -(expr)
            if (expr_string.StartsWith("-"))
            {
                Debugging.print("evaluating expression without \"-\" sign");
                string opposite_sign_expr = expr_string.Substring(1); // take away the "-"
                Variable opposite_sign_var = parse(opposite_sign_expr);
                Debugging.print("evaluated expression without the \"-\" symbol is of type ", opposite_sign_var.getTypeString(), " and value ", opposite_sign_var.getValue());
                switch (opposite_sign_var)
                {
                    case Integer _:
                    {
                        int signed_value = -1 * opposite_sign_var.getValue();
                        return new Integer(signed_value);
                    }
                    case FloatVar _:
                    {
                        float signed_value = -1 * opposite_sign_var.getValue();
                        return new FloatVar(signed_value);
                    }
                    default:
                        throw new AquilaExceptions.InvalidTypeError($"Cannot cast \"-\" on a {opposite_sign_var.getTypeString()} variable");
                }
            }

            Debugging.print("AL operations ?");
            // mathematical and logical operations
            foreach (char op in Global.al_operations)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (expr_string.Contains(op.ToString()))
                {
                    string simplified = StringUtils.simplifyExpr(expr_string, new []{ op }); // only look for specific delimiter
                    // more than one simplified expression ?
                    if (simplified.Split(op).Length > 1)
                    {
                        Debugging.print("operation ", expr_string, " and op: ", op);
                        List<string> splitted_str =
                            StringUtils.splitStringKeepingStructureIntegrity(expr_string, op, Global.base_delimiters);

                        // custom: logic operations laziness here (tmp) //!
                        Variable variable = parse(splitted_str[0]);
                        if (Global.getSetting("lazy logic") && variable is BooleanVar)
                        {
                            Debugging.print("lazy logic evaluation");
                            bool first = (variable as BooleanVar).getValue();
                            switch (op)
                            {
                                case '|':// when first:
                                    if (first) return new BooleanVar(true, true);
                                    break;
                                case '&':// when !first:
                                    if (!first) return new BooleanVar(false, true);
                                    break;
                            }
                        }

                        var splitted_var = new List<Variable>{ variable };
                        splitted_var.AddRange(splitted_str.GetRange(1, splitted_str.Count - 1).Select(parse));

                        // reduce the list to a list of one element
                        // e.g. expr1 + expr2 + expr3 => final_expr
                        while (splitted_var.Count > 1)
                        {
                            // merge the two first expressions
                            Variable expr1_var = splitted_var[0];
                            Variable expr2_var = splitted_var[1];
                            Variable result = applyOperator(expr1_var, expr2_var, op);
                            // merge result of 0 and 1
                            splitted_var[0] = result;
                            // remove 1 (part of it found in 0 now)
                            splitted_var.RemoveAt(1);
                        }

                        return splitted_var[0];
                    }
                }
            }
            Debugging.print("not (!) operator ?");
            // '!' operator (only one to take one variable)
            if (expr_string.StartsWith("!"))
            {
                Debugging.assert(expr_string[1] == '(');
                Debugging.assert(expr_string[expr_string.Length - 1] == ')');
                Variable expr = parse(expr_string.Substring(2, expr_string.Length - 3));
                Debugging.assert(expr is BooleanVar);
                Debugging.print("base val b4 not operator is ", expr.getValue());
                return ((BooleanVar) expr).not();
            }

            Debugging.print("value function call ?");
            // value function call
            if (expr_string.Contains("("))
            {
                string function_name = expr_string.Split('(')[0]; // extract function name
                int func_call_length = function_name.Length;
                function_name = StringUtils.normalizeWhiteSpaces(function_name);
                Debugging.print("function name: ", function_name);
                Functions.assertFunctionExists(function_name);
                expr_string = expr_string.Substring(func_call_length); // remove function name
                expr_string = expr_string.Substring(1, expr_string.Length - 2); // remove parenthesis
                Debugging.print("expr_string for function call ", expr_string);

                var arg_list = new List<Expression>();
                foreach (string arg_string in StringUtils.splitStringKeepingStructureIntegrity(expr_string, ',', Global.base_delimiters))
                {
                    string purged_arg_string = StringUtils.normalizeWhiteSpaces(arg_string);
                    Expression arg_expr = new Expression(purged_arg_string);
                    arg_list.Add(arg_expr);
                }

                if (arg_list.Count == 1 && arg_list[0].expr == "")
                {
                    arg_list = new List<Expression>();
                }

                Debugging.print("creating value function call with ", arg_list.Count, " parameters");

                FunctionCall func_call = new FunctionCall(function_name, arg_list);
                return func_call.callFunction();
            }

            // function call without parenthesis -> no parameters either
            if (!expr_string.StartsWith("$"))
            {
                Debugging.print($"Call the function \"{expr_string}\" with no parameters");
                var func_call = new FunctionCall(expr_string, new List<Expression>());
                return func_call.callFunction();
            }

            Debugging.print("variable ?");
            // variable access

            // since it is the last possibility for the parse call to return something, assert it is a variable
            Debugging.assert(expr_string.StartsWith("$")); // SyntaxError
            Debugging.print("list access ?");
            // ReSharper disable once PossibleNullReferenceException
            if (expr_string.Contains("["))
            {
                // brackets
                Debugging.assert(expr_string.EndsWith("]")); // cannot be "$l[0] + 5" bc AL_operations have already been processed
                int bracket_start_index = expr_string.IndexOf('[');
                Debugging.assert(bracket_start_index > 1); // "$[$i - 4]" is not valid
                // variable
                Expression var_name_expr = new Expression(expr_string.Substring(0, bracket_start_index));
                Debugging.print("list name: " + var_name_expr.expr);
                // index list
                IEnumerable<string> index_list = StringUtils.getBracketsContent(expr_string.Substring(bracket_start_index));
                string index_list_expr_string = index_list.Aggregate("", (current, s) => current + s + ", ");
                index_list_expr_string = "[" + index_list_expr_string.Substring(0, index_list_expr_string.Length - 2) + "]";
                var index_list_expr = new Expression(index_list_expr_string);
                
                Debugging.print("index: " + index_list_expr.expr);

                // create a value function call (list_at call)
                object[] args = { var_name_expr, index_list_expr };
                return Functions.callFunctionByName("list_at", args);
            }

            // only variable name, no brackets
            Debugging.print("var by name: ", expr_string);
            return variableFromName(expr_string);
        }

        /// <summary>
        /// Get the <see cref="Variable"/> from the current variable Dictionary.
        /// You an give the variable name with or without the "$" prefix
        /// </summary>
        /// <param name="var_name"> The variable name (with or without the "$" as a prefix)</param>
        /// <returns> the corresponding <see cref="Variable"/></returns>
        private static Variable variableFromName(string var_name)
        {
            if (var_name.StartsWith("$")) var_name = var_name.Substring(1);
            Debugging.print("Variable access: ", var_name);
            //Interpreter.processInterpreterInput("vars");
            Debugging.assert(Global.variableExistsInCurrentScope(var_name),
                new AquilaExceptions.NameError($"Variable name \"{var_name}\" does not exist in the current Context"));
            return Global.variableFromName(var_name);
        }

        /// <summary>
        /// Applies an arithmetical or logical operation on two <see cref="Variable"/>s
        /// <para/>result = (variable1) op (variable2)
        /// </summary>
        /// <param name="v1"> var 1</param>
        /// <param name="v2"> var 2</param>
        /// <param name="op"> operator (e.g. '+', '-', '&')</param>
        /// <returns> result <see cref="Variable"/></returns>
        /// <exception cref="AquilaExceptions.InvalidTypeError"> Invalid type with this operator</exception>
        /// <exception cref="AquilaExceptions.SyntaxExceptions.SyntaxError"> Unknown operator char</exception>
        private static Variable applyOperator(Variable v1, Variable v2, char op)
        {
            int comparison;
            Debugging.print("applyOperator: ", v1.ToString(), " ", op, " ", v2.ToString(), " (", v1.getTypeString(), " ", op, " ", v2.getTypeString(), ")");
            // Debugging.assert(v1.hasSameParent(v2)); // operations between same classes/subclasses
            if (!v1.hasSameParent(v2))
            {
                if (v2.isConst())
                {
                    if (v2 is Integer)
                    {
                        Debugging.print("Converting int to float because of const status: ", v1.ToString());
                        Debugging.assert(v1 is FloatVar, new AquilaExceptions.InvalidTypeError($"The type \"{v1.getTypeString()}\" was not expected. \"{v1.getTypeString()}\" expected")); // if this is not a float, operation is not permitted !
                        v2 = new FloatVar((float) v2.getValue());
                    }
                }
                else
                {
                    throw new AquilaExceptions.InvalidTypeError($"The type \"{v1.getTypeString()}\" was not expected");
                }
            }
            switch (op)
            {
                // arithmetic
                case '+':
                    if (v1 is Integer integer)
                    {
                        return integer.addition(v2 as Integer);
                    }
                    else if (v1 is FloatVar)
                    {
                        return ((FloatVar) v1).addition(v2 as FloatVar);
                    }
                    else
                    {
                        throw new AquilaExceptions.InvalidTypeError($"Invalid type \"{v1.getTypeString()}\" with operator \"{op}\"");
                    }   
                case '-':
                    if (v1 is Integer)
                    {
                        return ((Integer) v1).subtraction(v2 as Integer);
                    }
                    else if (v1 is FloatVar)
                    {
                        return ((FloatVar) v1).subtraction(v2 as FloatVar);
                    }
                    else
                    {
                        throw new AquilaExceptions.InvalidTypeError($"Invalid type \"{v1.getTypeString()}\" with operator \"{op}\"");
                    }
                case '/':
                    if (v1 is Integer)
                    {
                        return ((Integer) v1).division(v2 as Integer);
                    }
                    else if (v1 is FloatVar)
                    {
                        return ((FloatVar) v1).division(v2 as FloatVar);
                    }
                    else
                    {
                        throw new AquilaExceptions.InvalidTypeError($"Invalid type \"{v1.getTypeString()}\" with operator \"{op}\"");
                    }
                case '*':
                    if (v1 is Integer)
                    {
                        return ((Integer) v1).mult(v2 as Integer);
                    }
                    else if (v1 is FloatVar)
                    {
                        return ((FloatVar) v1).mult(v2 as FloatVar);
                    }
                    else
                    {
                        throw new AquilaExceptions.InvalidTypeError($"Invalid type \"{v1.getTypeString()}\" with operator \"{op}\"");
                    }
                case '%':
                    if (v1 is Integer)
                    {
                        return ((Integer) v1).modulo(v2 as Integer);
                    }
                    else
                    {
                        throw new AquilaExceptions.InvalidTypeError($"Invalid type \"{v1.getTypeString()}\" with operator \"{op}\"");
                    }
                // logic
                case '<':
                    Debugging.assert(v1 is Integer || v1 is FloatVar);
                    comparison = v1 is Integer
                        ? ((Integer) v1).compare(v2 as Integer)
                        : ((FloatVar) v1).compare(v2 as FloatVar);
                    return new BooleanVar(comparison == -1);
                case '>':
                    Debugging.assert(v1 is Integer || v1 is FloatVar);
                    comparison = v1 is Integer
                        ? ((Integer) v1).compare(v2 as Integer)
                        : ((FloatVar) v1).compare(v2 as FloatVar);
                    return new BooleanVar(comparison == 1);
                case '{':
                    Debugging.assert(v1 is Integer || v1 is FloatVar);
                    comparison = v1 is Integer
                        ? ((Integer) v1).compare(v2 as Integer)
                        : ((FloatVar) v1).compare(v2 as FloatVar);
                    return new BooleanVar(comparison != 1);
                case '}':
                    Debugging.assert(v1 is Integer || v1 is FloatVar);
                    comparison = v1 is Integer
                        ? ((Integer) v1).compare(v2 as Integer)
                        : ((FloatVar) v1).compare(v2 as FloatVar);
                    return new BooleanVar(comparison != -1);
                case '~':
                    Debugging.assert(v1 is Integer || v1 is FloatVar);
                    comparison = v1 is Integer
                        ? ((Integer) v1).compare(v2 as Integer)
                        : ((FloatVar) v1).compare(v2 as FloatVar);
                    return new BooleanVar(comparison == 0);
                case ':':
                    Debugging.assert(v1 is Integer || v1 is FloatVar);
                    comparison = v1 is Integer
                        ? ((Integer) v1).compare(v2 as Integer)
                        : ((FloatVar) v1).compare(v2 as FloatVar);
                    return new BooleanVar(comparison != 0);
                case '|':
                    Debugging.assert(v1 is BooleanVar);
                    return ((BooleanVar) v1).or((BooleanVar) v2);
                case '^':
                    Debugging.assert(v1 is BooleanVar);
                    return ((BooleanVar) v1).xor((BooleanVar) v2);
                case '&':
                    Debugging.assert(v1 is BooleanVar);
                    return ((BooleanVar) v1).and((BooleanVar) v2);
                default:
                    throw new AquilaExceptions.SyntaxExceptions.SyntaxError("Unknown operand " + op);
            }
        }
    }
}