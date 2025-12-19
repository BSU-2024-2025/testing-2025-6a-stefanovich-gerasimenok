using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CalculatorCore
{
    public class AdvancedCalculator : IAdvancedCalculator
    {
        private static readonly HashSet<string> Functions = new HashSet<string>
        {
            "sin", "cos", "tan", "ctg", "exp"
        };

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "if", "else", "while", "return"
        };

        private readonly Dictionary<string, decimal> _variables = new Dictionary<string, decimal>();
        private List<Command> _program = new List<Command>();
        private readonly Stack<decimal> _stack = new Stack<decimal>();
        private int _pc = 0;

        private string _input;
        private int _pos;
        private int _length;

        private bool _returnFlag = false;
        private decimal _returnValue = 0m;

        // Константы
        private const decimal E = 2.71828182845904523536028747135266249775724709369995m;
        private const decimal PI = 3.14159265358979323846264338327950288419716939937510m;

        #region IAdvancedCalculator Implementation

        public decimal Evaluate(string expression)
        {
            _program = ParseToCommands(expression);
            return ExecuteCommands();
        }

        public decimal EvaluateWithReturn(string expression)
        {
            return Evaluate(expression);
        }

        public ExecutionResult EvaluateWithIntermediateResults(string expression)
        {
            _program = ParseToCommands(expression);

            var result = new ExecutionResult();

            try
            {
                _stack.Clear();
                _pc = 0;
                _returnFlag = false;
                _returnValue = 0m;

                result.AddIntermediateResult("Начало выполнения");

                int loopCount = 0;
                while (_pc < _program.Count && !_returnFlag)
                {
                    Command currentCommand = _program[_pc];

                    ExecuteCommand(currentCommand);

                    if (currentCommand.Type == CommandType.JMP)
                    {
                        loopCount++;
                        if (_variables.Any())
                        {
                            var state = string.Join(", ", _variables
                                .Select(kv => $"{kv.Key} = {(kv.Value == Math.Floor(kv.Value) ? ((int)kv.Value).ToString() : kv.Value.ToString())}"));
                            result.AddIntermediateResult($"Цикл {loopCount}: {state}");
                        }
                    }

                    _pc++;
                }

                decimal finalResult = _returnFlag ? _returnValue : (_stack.Count == 0 ? 0m : _stack.Peek());
                result.FinalResult = finalResult;
                result.AddIntermediateResult($"Финальный результат: {finalResult}");

                return result;
            }
            catch (Exception e)
            {
                result.AddIntermediateResult($"Ошибка: {e.Message}");
                throw new InvalidOperationException($"Ошибка выполнения: {e.Message}", e);
            }
        }

        public List<Command> ParseToCommands(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Ввод не может быть пустым.");

            string processedExpression = expression
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\r", "\r")
                .Replace("e^-", "e-");

            processedExpression = processedExpression.TrimEnd();

            string oldInput = _input;
            int oldPos = _pos;
            int oldLength = _length;
            List<Command> oldProgram = _program;

            try
            {
                _input = processedExpression;
                _pos = 0;
                _length = _input.Length;
                _program = new List<Command>();
                _variables.Clear();

                ParseAllStatements();

                ValidateParentheses(expression);

                return new List<Command>(_program);
            }
            finally
            {
                _input = oldInput;
                _pos = oldPos;
                _length = oldLength;
                _program = oldProgram;
            }
        }

        public decimal ExecuteCommands()
        {
            _stack.Clear();
            _pc = 0;
            _returnFlag = false;
            _returnValue = 0m;

            while (_pc < _program.Count && !_returnFlag)
            {
                ExecuteCommand(_program[_pc]);
                _pc++;
            }

            return _returnFlag ? _returnValue : (_stack.Count == 0 ? 0m : _stack.Peek());
        }

        #endregion

        #region ICalculator Implementation

        public decimal Add(decimal a, decimal b) => a + b;

        public decimal Subtract(decimal a, decimal b) => a - b;

        public decimal Multiply(decimal a, decimal b) => a * b;

        public decimal Divide(decimal a, decimal b)
        {
            if (b == 0)
                throw new DivideByZeroException("Cannot divide by zero");
            return a / b;
        }

        public bool TryDivide(decimal a, decimal b, out decimal result)
        {
            result = 0;
            if (b == 0) return false;
            result = a / b;
            return true;
        }

        #endregion

        #region Private Methods

        private void ExecuteCommand(Command cmd)
        {
            switch (cmd.Type)
            {
                case CommandType.PUSH:
                    var pushValue = cmd.GetValue(_variables);
                    if (pushValue.HasValue)
                        _stack.Push(pushValue.Value);
                    else
                        throw new InvalidOperationException("Invalid PUSH command");
                    break;

                case CommandType.ASSIGN:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for ASSIGN");
                    _variables[cmd.Variable] = _stack.Pop();
                    break;

                case CommandType.ADD:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for ADD");
                    _stack.Push(_stack.Pop() + _stack.Pop());
                    break;

                case CommandType.SUB:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for SUB");
                    decimal b = _stack.Pop();
                    decimal a = _stack.Pop();
                    _stack.Push(a - b);
                    break;

                case CommandType.MUL:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for MUL");
                    _stack.Push(_stack.Pop() * _stack.Pop());
                    break;

                case CommandType.DIV:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for DIV");
                    decimal divisor = _stack.Pop();
                    if (Math.Abs(divisor) < 0.000000000001m)
                        throw new DivideByZeroException("Деление на ноль невозможно.");
                    _stack.Push(_stack.Pop() / divisor);
                    break;

                case CommandType.MOD:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for MOD");
                    decimal modDivisor = _stack.Pop();
                    if (Math.Abs(modDivisor) < 0.000000000001m)
                        throw new DivideByZeroException("Деление на ноль невозможно.");
                    _stack.Push(_stack.Pop() % modDivisor);
                    break;

                case CommandType.GT:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for GT");
                    decimal rightGT = _stack.Pop();
                    decimal leftGT = _stack.Pop();
                    _stack.Push(leftGT > rightGT ? 1m : 0m);
                    break;

                case CommandType.LT:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for LT");
                    decimal rightLT = _stack.Pop();
                    decimal leftLT = _stack.Pop();
                    _stack.Push(leftLT < rightLT ? 1m : 0m);
                    break;

                case CommandType.GE:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for GE");
                    decimal rightGE = _stack.Pop();
                    decimal leftGE = _stack.Pop();
                    _stack.Push(leftGE >= rightGE ? 1m : 0m);
                    break;

                case CommandType.LE:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for LE");
                    decimal rightLE = _stack.Pop();
                    decimal leftLE = _stack.Pop();
                    _stack.Push(leftLE <= rightLE ? 1m : 0m);
                    break;

                case CommandType.EQ:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for EQ");
                    decimal rightEQ = _stack.Pop();
                    decimal leftEQ = _stack.Pop();
                    _stack.Push(Math.Abs(leftEQ - rightEQ) < 0.000000000001m ? 1m : 0m);
                    break;

                case CommandType.NE:
                    if (_stack.Count < 2) throw new InvalidOperationException("Stack underflow for NE");
                    decimal rightNE = _stack.Pop();
                    decimal leftNE = _stack.Pop();
                    _stack.Push(Math.Abs(leftNE - rightNE) > 0.000000000001m ? 1m : 0m);
                    break;

                case CommandType.JMP:
                    _pc = cmd.Address.Value - 1;
                    break;

                case CommandType.JNE:
                    if (_stack.Count == 0)
                        throw new InvalidOperationException("Stack underflow for JNE - no condition value");
                    decimal condition = _stack.Pop();
                    if (Math.Abs(condition) < 0.000000000001m)
                        _pc = cmd.Address.Value - 1;
                    break;

                case CommandType.RETURN:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for RETURN");
                    _returnValue = _stack.Pop();
                    _returnFlag = true;
                    break;

                case CommandType.SIN:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for SIN");
                    decimal sinArg = _stack.Pop();
                    _stack.Push((decimal)Math.Sin((double)(sinArg * PI / 180m)));
                    break;

                case CommandType.COS:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for COS");
                    decimal cosArg = _stack.Pop();
                    _stack.Push((decimal)Math.Cos((double)(cosArg * PI / 180m)));
                    break;

                case CommandType.TAN:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for TAN");
                    decimal tanArg = _stack.Pop();
                    _stack.Push((decimal)Math.Tan((double)(tanArg * PI / 180m)));
                    break;

                case CommandType.CTG:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for CTG");
                    decimal ctgArg = _stack.Pop();
                    decimal tanValue = (decimal)Math.Tan((double)(ctgArg * PI / 180m));
                    if (Math.Abs(tanValue) < 0.000000000001m)
                        throw new DivideByZeroException("Котангенс не определен для данного угла.");
                    _stack.Push(1m / tanValue);
                    break;

                case CommandType.EXP:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for EXP");
                    decimal expArg = _stack.Pop();
                    _stack.Push((decimal)Math.Exp((double)expArg));
                    break;

                case CommandType.FACT:
                    if (_stack.Count == 0) throw new InvalidOperationException("Stack underflow for FACT");
                    decimal factArg = _stack.Pop();
                    if (factArg < 0 || factArg != Math.Floor(factArg))
                        throw new ArgumentException("Факториал определен только для целых неотрицательных чисел");
                    decimal factResult = 1m;
                    for (decimal i = 2; i <= factArg; i++)
                        factResult *= i;
                    _stack.Push(factResult);
                    break;
            }
        }

        private void ParseAllStatements()
        {
            while (_pos < _length)
            {
                SkipBlank();
                if (_pos >= _length) break;

                ParseStatementToCommands();

                SkipBlank();
                if (_pos < _length && _input[_pos] == ';')
                {
                    _pos++;
                }
            }
        }

        private void ValidateParentheses(string expression)
        {
            int parentheses = 0;
            int braces = 0;

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (c == '(') parentheses++;
                else if (c == ')') parentheses--;
                else if (c == '{') braces++;
                else if (c == '}') braces--;

                if (parentheses < 0 || braces < 0)
                {
                    throw new ArgumentException("Несбалансированные скобки в выражении: " + expression);
                }
            }

            if (parentheses != 0)
                throw new ArgumentException("Несбалансированные круглые скобки в выражении: " + expression);

            if (braces != 0)
                throw new ArgumentException("Несбалансированные фигурные скобки в выражении: " + expression);
        }

        private void SkipBlank()
        {
            while (_pos < _length)
            {
                char currentChar = _input[_pos];

                if (char.IsWhiteSpace(currentChar))
                {
                    _pos++;
                    continue;
                }

                if (currentChar == '/' && _pos + 1 < _length && _input[_pos + 1] == '/')
                {
                    _pos += 2;
                    while (_pos < _length && _input[_pos] != '\n')
                    {
                        _pos++;
                    }
                    continue;
                }

                break;
            }
        }

        private void ParseStatementToCommands()
        {
            SkipBlank();

            if (ParseKeyword("return"))
            {
                SkipBlank();
                ParseComparisonToCommands();
                _program.Add(new Command(CommandType.RETURN));
                return;
            }

            if (ParseKeyword("if"))
            {
                ParseIfStatementToCommands();
                return;
            }

            if (ParseKeyword("while"))
            {
                ParseWhileLoopToCommands();
                return;
            }

            if (CheckAndSkip('{'))
            {
                while (_pos < _length && !Check('}'))
                {
                    ParseStatementToCommands();
                    SkipBlank();
                    if (Check(';'))
                    {
                        CheckAndSkip(';');
                    }
                }
                if (!CheckAndSkip('}'))
                {
                    throw new ArgumentException("Ожидается '}'");
                }
                return;
            }

            ParseAssignmentToCommands();

            SkipBlank();
            if (Check(';'))
            {
                CheckAndSkip(';');
            }
        }

        private void ParseAssignmentToCommands()
        {
            int startPos = _pos;
            string maybeId = ParseIdentifierForAssignment();

            SkipBlank();
            if (!string.IsNullOrEmpty(maybeId) && CheckAndSkip('='))
            {
                if (!IsValidVariableName(maybeId))
                {
                    throw new ArgumentException("Неверное имя переменной: " + maybeId);
                }
                ParseComparisonToCommands();
                _program.Add(new Command(CommandType.ASSIGN, maybeId));
            }
            else
            {
                _pos = startPos;
                ParseComparisonToCommands();
            }
        }

        private void ParseIfStatementToCommands()
        {
            SkipBlank();

            if (!CheckAndSkip('('))
            {
                throw new ArgumentException("Ожидается '(' после if");
            }
            ParseComparisonToCommands();
            if (!CheckAndSkip(')'))
            {
                throw new ArgumentException("Ожидается ')' после условия if");
            }

            _program.Add(new Command(CommandType.JNE, 0));
            int jnePosition = _program.Count - 1;

            ParseStatementToCommands();

            _program.Add(new Command(CommandType.JMP, 0));
            int jmpPosition = _program.Count - 1;

            _program[jnePosition] = new Command(CommandType.JNE, _program.Count);

            SkipBlank();
            if (ParseKeyword("else"))
            {
                ParseStatementToCommands();
            }

            _program[jmpPosition] = new Command(CommandType.JMP, _program.Count);
        }

        private void ParseWhileLoopToCommands()
        {
            int loopStart = _program.Count;

            string condStr = ExtractParenthesesContent();
            List<Command> condCommands = ParseSubExpressionToCommands(condStr);
            _program.AddRange(condCommands);

            int jnePosition = _program.Count;
            _program.Add(new Command(CommandType.JNE, 0));

            string bodyStr = ExtractSingleStatementString();
            if (string.IsNullOrWhiteSpace(bodyStr))
            {
                throw new ArgumentException("Пустое тело while");
            }
            List<Command> bodyCommands = ParseSubExpressionToCommands(bodyStr);
            _program.AddRange(bodyCommands);

            _program.Add(new Command(CommandType.JMP, loopStart));

            _program[jnePosition] = new Command(CommandType.JNE, _program.Count);
        }

        private List<Command> ParseSubExpressionToCommands(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return new List<Command>();
            }

            string oldInput = _input;
            int oldPos = _pos;
            int oldLength = _length;
            List<Command> oldProgram = _program;

            try
            {
                _input = expression;
                _pos = 0;
                _length = expression.Length;
                _program = new List<Command>();

                ParseAllStatements();

                return new List<Command>(_program);
            }
            finally
            {
                _input = oldInput;
                _pos = oldPos;
                _length = oldLength;
                _program = oldProgram;
            }
        }

        private void ParseComparisonToCommands()
        {
            ParseExpressionToCommands();
            SkipBlank();

            while (_pos < _length)
            {
                string op = ParseComparisonOperator();
                if (op != null)
                {
                    ParseExpressionToCommands();

                    switch (op)
                    {
                        case ">":
                            _program.Add(new Command(CommandType.GT));
                            break;
                        case "<":
                            _program.Add(new Command(CommandType.LT));
                            break;
                        case ">=":
                            _program.Add(new Command(CommandType.GE));
                            break;
                        case "<=":
                            _program.Add(new Command(CommandType.LE));
                            break;
                        case "==":
                            _program.Add(new Command(CommandType.EQ));
                            break;
                        case "!=":
                            _program.Add(new Command(CommandType.NE));
                            break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseExpressionToCommands()
        {
            ParseTermToCommands();
            SkipBlank();

            while (_pos < _length)
            {
                if (CheckAndSkip('+'))
                {
                    ParseTermToCommands();
                    _program.Add(new Command(CommandType.ADD));
                }
                else if (CheckAndSkip('-'))
                {
                    ParseTermToCommands();
                    _program.Add(new Command(CommandType.SUB));
                }
                else
                {
                    break;
                }
                SkipBlank();
            }
        }

        private void ParseTermToCommands()
        {
            ParseFactorToCommands();
            SkipBlank();

            while (_pos < _length)
            {
                if (CheckAndSkip('*'))
                {
                    ParseFactorToCommands();
                    _program.Add(new Command(CommandType.MUL));
                }
                else if (CheckAndSkip('/'))
                {
                    ParseFactorToCommands();
                    _program.Add(new Command(CommandType.DIV));
                }
                else if (CheckAndSkip('%'))
                {
                    ParseFactorToCommands();
                    _program.Add(new Command(CommandType.MOD));
                }
                else
                {
                    break;
                }
                SkipBlank();
            }
        }

        private void ParseFactorToCommands()
        {
            SkipBlank();

            if (_pos >= _length)
            {
                throw new ArgumentException("Ожидается число, функция или переменная на позиции " + (_pos + 1));
            }

            int sign;
            if (CheckAndSkip('+'))
            {
                sign = 1;
            }
            else if (CheckAndSkip('-'))
            {
                sign = -1;
            }
            else
            {
                sign = 1;
            }
            SkipBlank();

            if (_pos >= _length)
            {
                throw new ArgumentException("Ожидается число, функция или переменная на позиции " + (_pos + 1));
            }

            if (CheckAndSkip('('))
            {
                ParseExpressionToCommands();
                SkipBlank();
                if (!CheckAndSkip(')'))
                {
                    throw new ArgumentException("Ожидается закрывающая скобка на позиции " + (_pos + 1));
                }
                if (sign == -1)
                {
                    _program.Add(new Command(CommandType.PUSH, -1.0m));
                    _program.Add(new Command(CommandType.MUL));
                }
            }
            else if (char.IsDigit(GetCurrentChar()) || GetCurrentChar() == '.')
            {
                decimal number = ParseNumber();
                _program.Add(new Command(CommandType.PUSH, sign * number));
            }
            else if (char.IsLetter(GetCurrentChar()) || GetCurrentChar() == '_')
            {
                string identifier = ParseName();
                SkipBlank();
                if (Check('('))
                {
                    _pos -= identifier.Length;
                    ParseFunctionCallToCommands();
                    if (sign == -1)
                    {
                        _program.Add(new Command(CommandType.PUSH, -1.0m));
                        _program.Add(new Command(CommandType.MUL));
                    }
                }
                else
                {
                    Command pushCmd = new Command(CommandType.PUSH);
                    pushCmd.SetVariableInfo(identifier, sign);
                    _program.Add(pushCmd);
                }
            }
            else if (CheckKeyword("e") || CheckKeyword("pi"))
            {
                if (CheckKeyword("e"))
                {
                    _pos += 1;
                    _program.Add(new Command(CommandType.PUSH, sign * E));
                }
                else if (CheckKeyword("pi"))
                {
                    _pos += 2;
                    _program.Add(new Command(CommandType.PUSH, sign * PI));
                }
            }
            else
            {
                char unexpected = GetCurrentChar();
                throw new ArgumentException("Неожиданный символ '" + unexpected + "' на позиции " + (_pos + 1));
            }
        }

        private string ExtractSingleStatementString()
        {
            SkipBlank();
            int start = _pos;

            if (CheckAndSkip('{'))
            {
                int openBraces = 1;
                int blockStart = _pos;

                while (_pos < _length && openBraces > 0)
                {
                    char c = _input[_pos];
                    if (c == '{')
                    {
                        openBraces++;
                    }
                    else if (c == '}')
                    {
                        openBraces--;
                    }
                    _pos++;
                }

                if (openBraces != 0)
                {
                    throw new ArgumentException("Непарные фигурные скобки начиная с позиции " + (start + 1));
                }

                return _input.Substring(blockStart, _pos - blockStart - 1).Trim();
            }
            else
            {
                StringBuilder stmt = new StringBuilder();
                int braceDepth = 0;

                while (_pos < _length)
                {
                    char c = _input[_pos];

                    if (c == '{')
                    {
                        braceDepth++;
                    }
                    else if (c == '}')
                    {
                        if (braceDepth == 0)
                        {
                            break;
                        }
                        braceDepth--;
                    }
                    else if (braceDepth == 0 && (c == ';' || (c == 'e' && _pos + 3 < _length && _input.Substring(_pos, 4) == "else")))
                    {
                        if (c == 'e')
                        {
                            int savedPos = _pos;
                            ParseKeyword("else");
                            if (_pos > savedPos)
                            {
                                _pos = savedPos;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    stmt.Append(c);
                    _pos++;
                }

                string result = stmt.ToString().Trim();
                if (string.IsNullOrEmpty(result))
                {
                    throw new ArgumentException("Пустое выражение начиная с позиции " + (start + 1));
                }

                return result;
            }
        }

        private void ParseFunctionCallToCommands()
        {
            string funcName = ParseName();
            if (!Functions.Contains(funcName))
            {
                throw new ArgumentException("Неизвестная функция '" + funcName + "'");
            }
            SkipBlank();
            if (!CheckAndSkip('('))
            {
                throw new ArgumentException("Ожидается открывающая скобка после функции '" + funcName + "' на позиции " + (_pos + 1));
            }
            ParseExpressionToCommands();
            SkipBlank();
            if (!CheckAndSkip(')'))
            {
                throw new ArgumentException("Ожидается закрывающая скобка после функции '" + funcName + "' на позиции " + (_pos + 1));
            }

            switch (funcName)
            {
                case "sin": _program.Add(new Command(CommandType.SIN)); break;
                case "cos": _program.Add(new Command(CommandType.COS)); break;
                case "tan": _program.Add(new Command(CommandType.TAN)); break;
                case "ctg": _program.Add(new Command(CommandType.CTG)); break;
                case "exp": _program.Add(new Command(CommandType.EXP)); break;
            }
        }

        private string ExtractParenthesesContent()
        {
            SkipBlank();
            if (!CheckAndSkip('('))
            {
                throw new ArgumentException("Ожидается '(' на позиции " + (_pos + 1));
            }
            int start = _pos;
            int depth = 1;
            while (_pos < _length && depth > 0)
            {
                if (CheckAndSkip('('))
                {
                    depth++;
                }
                else if (CheckAndSkip(')'))
                {
                    depth--;
                }
                else
                {
                    _pos++;
                }
            }
            if (depth != 0)
            {
                throw new ArgumentException("Ожидается закрывающая скобка на позиции " + (_pos + 1));
            }
            return _input.Substring(start, _pos - start - 1);
        }

        private string ParseComparisonOperator()
        {
            SkipBlank();
            if (_pos + 1 < _length)
            {
                string twoCharOp = _input.Substring(_pos, 2);
                if (new HashSet<string> { "==", "!=", ">=", "<=" }.Contains(twoCharOp))
                {
                    _pos += 2;
                    return twoCharOp;
                }
            }
            if (_pos < _length)
            {
                char opChar = GetCurrentChar();
                if (opChar == '>' || opChar == '<')
                {
                    _pos++;
                    return opChar.ToString();
                }
            }
            return null;
        }

        private char GetCurrentChar()
        {
            return _pos < _length ? _input[_pos] : '\0';
        }

        private decimal ParseNumber()
        {
            int start = _pos;
            while (_pos < _length && char.IsDigit(GetCurrentChar())) _pos++;
            if (_pos < _length && GetCurrentChar() == '.')
            {
                do _pos++;
                while (_pos < _length && char.IsDigit(GetCurrentChar()));
            }
            if (_pos < _length && (GetCurrentChar() == 'e' || GetCurrentChar() == 'E'))
            {
                _pos++;
                if (_pos < _length && (GetCurrentChar() == '+' || GetCurrentChar() == '-')) _pos++;
                int exponentStart = _pos;
                while (_pos < _length && char.IsDigit(GetCurrentChar())) _pos++;
                if (_pos == exponentStart)
                {
                    throw new ArgumentException("Неверный числовой формат '" + _input.Substring(start, _pos - start) + "' на позиции " + (start + 1));
                }
            }
            try
            {
                return decimal.Parse(_input.Substring(start, _pos - start), CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new ArgumentException("Неверный числовой формат '" + _input.Substring(start, _pos - start) + "' на позиции " + (start + 1), e);
            }
        }

        private string ParseName()
        {
            int start = _pos;
            if (_pos < _length && (char.IsLetter(GetCurrentChar()) || GetCurrentChar() == '_'))
            {
                do
                {
                    _pos++;
                } while (_pos < _length && (char.IsLetterOrDigit(GetCurrentChar()) || GetCurrentChar() == '_'));
            }
            string name = _input.Substring(start, _pos - start);
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Ожидается идентификатор на позиции " + (_pos + 1));
            }
            return name;
        }

        private string ParseIdentifierForAssignment()
        {
            SkipBlank();
            int start = _pos;
            while (_pos < _length && !char.IsWhiteSpace(_input[_pos]) && _input[_pos] != '=')
            {
                _pos++;
            }
            string identifier = _input.Substring(start, _pos - start);

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("Неверный синтаксис присваивания: ожидается имя переменной");
            }

            return identifier;
        }

        private bool ParseKeyword(string keyword)
        {
            SkipBlank();
            if (_pos + keyword.Length <= _length && _input.Substring(_pos, keyword.Length) == keyword)
            {
                int after = _pos + keyword.Length;
                if (after >= _length || !(char.IsLetterOrDigit(_input[after]) || _input[after] == '_'))
                {
                    _pos += keyword.Length;
                    return true;
                }
            }
            return false;
        }

        private bool CheckKeyword(string keyword)
        {
            if (_pos + keyword.Length <= _length && _input.Substring(_pos, keyword.Length) == keyword)
            {
                int after = _pos + keyword.Length;
                return after >= _length || !(char.IsLetterOrDigit(_input[after]) || _input[after] == '_');
            }
            return false;
        }

        private bool Check(char expected)
        {
            return _pos < _length && _input[_pos] == expected;
        }

        private bool CheckAndSkip(char expected)
        {
            if (_pos < _length && _input[_pos] == expected)
            {
                _pos++;
                return true;
            }
            return false;
        }

        private bool IsValidVariableName(string name)
        {
            if (string.IsNullOrEmpty(name) || Functions.Contains(name)) return false;
            if (Keywords.Contains(name)) return false;

            char firstChar = name[0];
            if (!(char.IsLetter(firstChar) || firstChar == '_')) return false;

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (!char.IsLetterOrDigit(c) && c != '_') return false;
            }
            return true;
        }

        #endregion
    }
}