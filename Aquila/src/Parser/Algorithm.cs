﻿using System;
using System.Collections.Generic;
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Parser
{
    /// <summary>
    /// The <see cref="Algorithm"/> class tries to simulate a function.
    /// <para/>List of attributes :
    /// <para/>* <see cref="_name"/> : string
    /// <para/>* <see cref="_instructions"/> : List(Instruction)
    /// <para/>* <see cref="_return_value"/> : Expression
    /// </summary>
    public class Algorithm
    {
        /// <summary>
        /// Algorithm/Function name
        /// </summary>
        private readonly string _name;
        
        /// <summary>
        /// List of <see cref="Instruction"/>s. This is the basic definition of an algorithm.
        /// </summary>
        private readonly List<Instruction> _instructions;
        
        /// <summary>
        /// Expression of the return value of the Algorithm/Function
        /// </summary>
        private Expression _return_value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"> name of the Algorithm/Function</param>
        /// <param name="instructions"> list of <see cref="Instruction"/>s</param>
        public Algorithm(string name, List<Instruction> instructions)
        {
            this._name = name;
            this._instructions = instructions;
        }

        /// <summary>
        /// Getter function for the name
        /// </summary>
        /// <returns> function name</returns>
        public string getName() => _name;

        /// <summary>
        /// Execute the Algorithm/Function. <see cref="Instruction"/> by <see cref="Instruction"/>,
        /// until the list of instructions is exhausted and we can return the <see cref="_return_value"/>,
        /// using <see cref="Expression.parse"/> on it (it is an <see cref="Expression"/>)
        /// </summary>
        /// <returns> The evaluated <see cref="_return_value"/> after all the <see cref="_instructions"/> have been executed</returns>
        public Variable run()
        {
            Context.setStatus(4);
            Context.setInfo(_name);
            foreach (Instruction instr in _instructions)
            {
                try
                {
                    Context.setStatus(5);
                    Context.setInfo(instr);
                    instr.execute();
                    Context.reset();
                }
                catch (System.Reflection.TargetInvocationException out_exception)
                {
                    // normal TargetInvocationException
                    if (!(out_exception.InnerException is AquilaExceptions.ReturnValueException)) throw;
                    // casted ReturnValueException
                    AquilaExceptions.ReturnValueException exception =
                        (AquilaExceptions.ReturnValueException) out_exception.InnerException;

                    if (exception == null)
                    {
                        throw Global.aquilaError(); // something went wrong
                    }

                    _return_value = new Expression(exception.getExpr());
                    Context.reset();
                    return _return_value.evaluate();
                }
            }

            Context.reset();
            return new NullVar(); // NoReturnCallWarning
        }
    }
}