using System;
using System.Collections.Generic;

namespace TuringMachineSimulator
{
    public class TuringMachine
    {
        private readonly Dictionary<(string, char), (string, char, int)> _transitions;
        private string _currentState;
        private readonly List<char> _tape;
        private int _headPosition;

        public TuringMachine(Dictionary<(string, char), (string, char, int)> transitions, string startState, List<char> inputTape)
        {
            _transitions = transitions;
            _currentState = startState;
            _tape = inputTape;
            _headPosition = 0;
        }

        public string Run()
        {
            while (_currentState != "HALT")
            {
                var currentSymbol = _tape[_headPosition];
                if (_transitions.TryGetValue((_currentState, currentSymbol), out var action))
                {
                    _currentState = action.Item1;
                    _tape[_headPosition] = action.Item2;
                    _headPosition += action.Item3;

                    if (_headPosition < 0)
                    {
                        _tape.Insert(0, '_');
                        _headPosition = 0;
                    }
                    else if (_headPosition >= _tape.Count)
                    {
                        _tape.Add('_');
                    }
                }
                else
                {
                    throw new InvalidOperationException($"No transition defined for state {_currentState} and symbol {currentSymbol}");
                }
            }

            return string.Join("", _tape).TrimEnd('_');
        }
    }

    public static class BinaryIncrementer
    {
        public static string IncrementBinary(string binaryInput)
        {
            var tape = new List<char>(binaryInput.ToCharArray());
            tape.Add('_');

            var transitions = new Dictionary<(string, char), (string, char, int)>
            {
                { ("q0", '1'), ("q0", '1', 1) },
                { ("q0", '0'), ("q0", '0', 1) },
                { ("q0", '_'), ("q1", '_', -1) },
                { ("q1", '1'), ("q1", '0', -1) },
                { ("q1", '0'), ("q2", '1', 0) },
                { ("q1", '_'), ("q2", '1', 0) },
                { ("q2", '_'), ("HALT", '_', 0) }
            };

            var turingMachine = new TuringMachine(transitions, "q0", tape);
            return turingMachine.Run();
        }
    }
}