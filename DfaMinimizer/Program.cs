using System;
using System.Collections.Generic;
using System.Linq;

namespace DfaMinimizer
{
    public class DFA
    {
        public HashSet<int> States { get; set; }
        public HashSet<char> Alphabet { get; set; }
        public Dictionary<(int, char), int> TransitionFunction { get; set; }
        public int StartState { get; set; }
        public HashSet<int> AcceptStates { get; set; }

        public DFA(HashSet<int> states, HashSet<char> alphabet, Dictionary<(int, char), int> transitionFunction, int startState, HashSet<int> acceptStates)
        {
            States = states;
            Alphabet = alphabet;
            TransitionFunction = transitionFunction;
            StartState = startState;
            AcceptStates = acceptStates;
        }
    }

    public class DFAMinimizer
    {
        public DFA Minimize(DFA dfa)
        {
            var partitions = new List<HashSet<int>> { dfa.AcceptStates, dfa.States.Except(dfa.AcceptStates).ToHashSet() };
            var workList = new Queue<HashSet<int>>(partitions);

            while (workList.Count > 0)
            {
                var currentSet = workList.Dequeue();

                foreach (var symbol in dfa.Alphabet)
                {
                    var reachableStates = dfa.TransitionFunction
                        .Where(t => currentSet.Contains(t.Value) && t.Key.Item2 == symbol)
                        .Select(t => t.Key.Item1)
                        .ToHashSet();

                    var updatedPartitions = new List<HashSet<int>>();

                    foreach (var partition in partitions)
                    {
                        var intersection = partition.Intersect(reachableStates).ToHashSet();
                        var difference = partition.Except(reachableStates).ToHashSet();

                        if (intersection.Count > 0 && difference.Count > 0)
                        {
                            updatedPartitions.Add(intersection);
                            updatedPartitions.Add(difference);

                            if (workList.Contains(partition))
                            {
                                workList.Enqueue(intersection);
                                workList.Enqueue(difference);
                                workList = new Queue<HashSet<int>>(workList.Where(x => !x.SetEquals(partition)));
                            }
                            else
                            {
                                workList.Enqueue(intersection.Count <= difference.Count ? intersection : difference);
                            }
                        }
                        else
                        {
                            updatedPartitions.Add(partition);
                        }
                    }

                    partitions = updatedPartitions;
                }
            }

            var stateMapping = partitions.SelectMany((group, index) => group.Select(state => new { state, index }))
                                         .ToDictionary(x => x.state, x => x.index);

            var newStates = partitions.Select((_, index) => index).ToHashSet();
            var newStartState = stateMapping[dfa.StartState];
            var newAcceptStates = dfa.AcceptStates.Select(state => stateMapping[state]).ToHashSet();
            var newTransitionFunction = new Dictionary<(int, char), int>();
            foreach (var transition in dfa.TransitionFunction)
            {
                var state = transition.Key.Item1;
                var symbol = transition.Key.Item2;
                var target = transition.Value;

                if (stateMapping.ContainsKey(state) && stateMapping.ContainsKey(target))
                {
                    var newSource = stateMapping[state];
                    var newTarget = stateMapping[target];
                    newTransitionFunction[(newSource, symbol)] = newTarget;
                }
            }

            return new DFA(newStates, dfa.Alphabet, newTransitionFunction, newStartState, newAcceptStates);
        }
    }
}