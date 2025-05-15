using System.Collections.Generic;
using Xunit;
using DfaMinimizer;

namespace DfaMinimizerTests
{
    public class DFAMinimizerTests
    {
        [Fact]
        public void Minimize_ReducesStatesAndPreservesLanguage()
        {
            var states = new HashSet<int> { 0, 1, 2 };
            var alphabet = new HashSet<char> { 'a', 'b' };
            var transitions = new Dictionary<(int, char), int>
            {
                {(0, 'a'), 1},
                {(0, 'b'), 2},
                {(1, 'a'), 1},
                {(1, 'b'), 2},
                {(2, 'a'), 1},
                {(2, 'b'), 2}
            };
            var startState = 0;
            var acceptStates = new HashSet<int> { 1 };

            var dfa = new DFA(states, alphabet, transitions, startState, acceptStates);
            var minimizer = new DFAMinimizer();
            var minimizedDfa = minimizer.Minimize(dfa);

            Assert.True(minimizedDfa.States.Count < dfa.States.Count);
            Assert.NotEmpty(minimizedDfa.AcceptStates);
            Assert.True(IsAccepted(minimizedDfa, "a"));
            Assert.False(IsAccepted(minimizedDfa, "b"));
        }

        private bool IsAccepted(DFA dfa, string input)
        {
            var state = dfa.StartState;
            foreach (var c in input)
            {
                if (!dfa.TransitionFunction.TryGetValue((state, c), out var nextState))
                    return false;
                state = nextState;
            }
            return dfa.AcceptStates.Contains(state);
        }
    }
}