using System;
using System.Collections.Generic;

namespace CalculatorCore
{
    // Базовый интерфейс калькулятора
    public interface ICalculator
    {
        decimal Add(decimal a, decimal b);
        decimal Subtract(decimal a, decimal b);
        decimal Multiply(decimal a, decimal b);
        decimal Divide(decimal a, decimal b);
        bool TryDivide(decimal a, decimal b, out decimal result);
    }

    // Интерфейс для расширенного калькулятора
    public interface IAdvancedCalculator : ICalculator
    {
        decimal Evaluate(string expression);
        decimal EvaluateWithReturn(string expression);
        ExecutionResult EvaluateWithIntermediateResults(string expression);
        List<Command> ParseToCommands(string expression);
        decimal ExecuteCommands();
    }

    // Класс для результата выполнения
    public class ExecutionResult
    {
        public decimal FinalResult { get; set; }
        public List<string> IntermediateResults { get; set; }

        public ExecutionResult()
        {
            IntermediateResults = new List<string>();
        }

        public ExecutionResult(decimal finalResult, List<string> intermediateResults)
        {
            FinalResult = finalResult;
            IntermediateResults = intermediateResults ?? new List<string>();
        }

        public void AddIntermediateResult(string result)
        {
            IntermediateResults.Add(result);
        }
    }

    // Перечисление типов команд
    public enum CommandType
    {
        PUSH,
        ASSIGN,
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        GT,
        LT,
        GE,
        LE,
        EQ,
        NE,
        JMP,
        JNE,
        RETURN,
        SIN,
        COS,
        TAN,
        CTG,
        EXP,
        FACT
    }

    // Класс команды
    public class Command
    {
        public CommandType Type { get; set; }
        public decimal? Value { get; set; }
        public string Variable { get; set; }
        public int? Address { get; set; }

        private string _variableName;
        private int _variableSign = 1;

        public Command(CommandType type)
        {
            Type = type;
        }

        public Command(CommandType type, decimal value) : this(type)
        {
            Value = value;
        }

        public Command(CommandType type, string variable) : this(type)
        {
            Variable = variable;
        }

        public Command(CommandType type, int address) : this(type)
        {
            Address = address;
        }

        public decimal? GetValue(Dictionary<string, decimal> variables)
        {
            if (Type == CommandType.PUSH)
            {
                if (Value.HasValue)
                {
                    return Value;
                }
                else if (_variableName != null)
                {
                    if (variables.TryGetValue(_variableName, out var variableValue))
                    {
                        return variableValue * _variableSign;
                    }
                    return 0m;
                }
            }
            return null;
        }

        public void SetVariableInfo(string name, int sign)
        {
            _variableName = name;
            _variableSign = sign;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case CommandType.PUSH:
                    if (Value.HasValue)
                        return $"PUSH {Value}";
                    else if (_variableName != null)
                        return $"PUSH {_variableName} (sign: {_variableSign})";
                    else
                        return "PUSH [unknown]";
                case CommandType.ASSIGN: return $"ASSIGN {Variable}";
                case CommandType.JMP: return $"JMP {Address}";
                case CommandType.JNE: return $"JNE {Address}";
                case CommandType.RETURN: return "RETURN";
                case CommandType.ADD: return "ADD";
                case CommandType.SUB: return "SUB";
                case CommandType.MUL: return "MUL";
                case CommandType.DIV: return "DIV";
                case CommandType.MOD: return "MOD";
                case CommandType.GT: return "GT";
                case CommandType.LT: return "LT";
                case CommandType.GE: return "GE";
                case CommandType.LE: return "LE";
                case CommandType.EQ: return "EQ";
                case CommandType.NE: return "NE";
                case CommandType.SIN: return "SIN";
                case CommandType.COS: return "COS";
                case CommandType.TAN: return "TAN";
                case CommandType.CTG: return "CTG";
                case CommandType.EXP: return "EXP";
                case CommandType.FACT: return "FACT";
                default: return Type.ToString();
            }
        }
    }
}