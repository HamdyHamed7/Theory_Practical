using System;
using System.Collections.Generic;
using System.Linq;

namespace CFGToPDA
{
    public class CFG
    {
        public HashSet<string> Variables { get; set; }
        public HashSet<char> Terminals { get; set; }
        public Dictionary<string, List<string>> Productions { get; set; }
        public string StartVariable { get; set; }

        public CFG(HashSet<string> variables, HashSet<char> terminals, Dictionary<string, List<string>> productions, string startVariable)
        {
            Variables = variables;
            Terminals = terminals;
            Productions = productions;
            StartVariable = startVariable;
        }
    }

    public class PDA
    {
        public HashSet<string> States { get; set; }
        public HashSet<char> InputAlphabet { get; set; }
        public HashSet<string> StackAlphabet { get; set; }
        public string StartState { get; set; }
        public string StartStackSymbol { get; set; }
        public Dictionary<(string, char?, string), List<(string, string)>> TransitionFunction { get; set; }

        public PDA(HashSet<string> states, HashSet<char> inputAlphabet, HashSet<string> stackAlphabet, string startState, string startStackSymbol)
        {
            States = states;
            InputAlphabet = inputAlphabet;
            StackAlphabet = stackAlphabet;
            StartState = startState;
            StartStackSymbol = startStackSymbol;
            TransitionFunction = new Dictionary<(string, char?, string), List<(string, string)>>();
        }

        public void AddTransition(string currentState, char? input, string stackTop, string nextState, string stackPush)
        {
            var key = (currentState, input, stackTop);
            if (!TransitionFunction.ContainsKey(key))
            {
                TransitionFunction[key] = new List<(string, string)>();
            }
            TransitionFunction[key].Add((nextState, stackPush));
        }

        public bool Simulate(string input)
        {
            var stack = new Stack<string>();
            stack.Push(StartStackSymbol);

            var configurations = new Queue<(string, int, Stack<string>)>();
            configurations.Enqueue((StartState, 0, stack));

            while (configurations.Count > 0)
            {
                var (currentState, inputIndex, currentStack) = configurations.Dequeue();

                if (inputIndex == input.Length && currentStack.Count == 0)
                {
                    return true;
                }

                var stackTop = currentStack.Count > 0 ? currentStack.Peek() : null;

                foreach (var transition in TransitionFunction)
                {
                    var (state, symbol, top) = transition.Key;
                    if (state == currentState && (symbol == null || (inputIndex < input.Length && symbol == input[inputIndex])) && top == stackTop)
                    {
                        foreach (var (nextState, stackPush) in transition.Value)
                        {
                            var newStack = new Stack<string>(currentStack.Reverse());
                            newStack.Pop();
                            foreach (var pushSymbol in stackPush.Reverse())
                            {
                                if (!string.IsNullOrEmpty(pushSymbol.ToString()))
                                {
                                    newStack.Push(pushSymbol.ToString());
                                }
                            }

                            configurations.Enqueue((nextState, symbol == null ? inputIndex : inputIndex + 1, newStack));
                        }
                    }
                }
            }

            return false;
        }
    }
    public class CFGToPDAConverter
    {
        public PDA Convert(CFG cfg)
        {
            var states = new HashSet<string> { "q_start", "q_accept" };
            var stackAlphabet = new HashSet<string>(cfg.Variables) { "$" };
            var inputAlphabet = cfg.Terminals;
            var pda = new PDA(states, inputAlphabet, stackAlphabet, "q_start", "$");
            pda.AddTransition("q_start", null, "$", "q_start", cfg.StartVariable + "$");
            foreach (var production in cfg.Productions)
            {
                foreach (var rule in production.Value)
                {
                    pda.AddTransition("q_start", null, production.Key, "q_start", rule);
                }
            }

            foreach (var terminal in cfg.Terminals)
            {
                pda.AddTransition("q_start", terminal, terminal.ToString(), "q_start", "");
            }

            pda.AddTransition("q_start", null, "$", "q_accept", "$");
            return pda;
        }
    }
}