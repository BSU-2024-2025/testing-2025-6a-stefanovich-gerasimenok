using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CalculatorCore
{
    public class CalculatorService
    {
        private readonly ICalculator _calculator;
        private readonly IAdvancedCalculator _advancedCalculator;
        private const decimal E = 2.71828182845904523536028747135266249775724709369995m;
        private const decimal PI = 3.14159265358979323846264338327950288419716939937510m;

        public CalculatorService(ICalculator calculator)
        {
            _calculator = calculator;
            _advancedCalculator = calculator as IAdvancedCalculator;
        }

        public CalculatorResult PerformOperation(string operation, decimal a, decimal b)
        {
            try
            {
                return operation.ToLower() switch
                {
                    "add" or "+" => CalculatorResult.Ok(_calculator.Add(a, b)),
                    "subtract" or "-" => CalculatorResult.Ok(_calculator.Subtract(a, b)),
                    "multiply" or "*" => CalculatorResult.Ok(_calculator.Multiply(a, b)),
                    "divide" or "/" => _calculator.TryDivide(a, b, out var result)
                        ? CalculatorResult.Ok(result)
                        : CalculatorResult.Fail("Division by zero"),
                    "^" or "**" => CalculatorResult.Ok(Power(a, b)),
                    _ => CalculatorResult.Fail("Unknown operation")
                };
            }
            catch (Exception ex)
            {
                return CalculatorResult.Fail($"Error: {ex.Message}");
            }
        }

        public CalculatorResult EvaluateExpression(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                    return CalculatorResult.Fail("Пустое выражение");

                // Если доступен расширенный калькулятор, используем его
                if (_advancedCalculator != null && ContainsAdvancedFeatures(expression))
                {
                    return EvaluateAdvancedExpression(expression);
                }

                // Проверяем пробелы между числами
                if (HasSpaceBetweenNumbers(expression))
                    return CalculatorResult.Fail("Неверный формат: пробелы между числами не допускаются");

                // Удаляем все пробелы
                expression = expression.Replace(" ", "");

                if (HasInvalidFormat(expression))
                    return CalculatorResult.Fail("Неверный формат выражения");

                // Вычисляем выражение
                decimal result = Evaluate(expression);
                return CalculatorResult.Ok(result);
            }
            catch (Exception ex)
            {
                return CalculatorResult.Fail($"Ошибка: {ex.Message}");
            }
        }

        private CalculatorResult EvaluateAdvancedExpression(string expression)
        {
            try
            {
                if (_advancedCalculator == null)
                    return CalculatorResult.Fail("Расширенный калькулятор не доступен");

                var result = _advancedCalculator.Evaluate(expression);
                return CalculatorResult.Ok(result);
            }
            catch (Exception ex)
            {
                return CalculatorResult.Fail($"Ошибка: {ex.Message}");
            }
        }

        private bool ContainsAdvancedFeatures(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            // Проверяем наличие ключевых слов
            var lowerExpression = expression.ToLower();
            if (Keywords.Any(k => lowerExpression.Contains(k + " ")))
                return true;

            // Проверяем наличие операторов сравнения
            if (ComparisonOperators.Any(o => expression.Contains(o)))
                return true;

            // Проверяем наличие точки с запятой
            if (expression.Contains(';'))
                return true;

            // Проверяем наличие оператора присваивания
            if (expression.Contains('=') && !expression.Contains("=="))
                return true;

            return false;
        }

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "if", "else", "while", "return"
        };

        private static readonly HashSet<string> ComparisonOperators = new HashSet<string>
        {
            "==", "!=", ">=", "<=", ">", "<"
        };

        private bool HasSpaceBetweenNumbers(string expression)
        {
            return Regex.IsMatch(expression, @"\d\s+\d");
        }

        private bool HasInvalidFormat(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return true;

            // Проверяем сбалансированность скобок
            int balance = 0;
            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0) return true;
            }
            if (balance != 0) return true;

            return false;
        }

        private decimal Evaluate(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return 0;

            int i = 0;
            return EvaluateExpressionRecursive(expression, ref i);
        }

        private decimal EvaluateExpressionRecursive(string expression, ref int index)
        {
            decimal result = EvaluateTerm(expression, ref index);

            while (index < expression.Length)
            {
                char op = expression[index];
                if (op == '+' || op == '-')
                {
                    index++;
                    decimal nextTerm = EvaluateTerm(expression, ref index);

                    if (op == '+')
                        result = _calculator.Add(result, nextTerm);
                    else
                        result = _calculator.Subtract(result, nextTerm);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private decimal EvaluateTerm(string expression, ref int index)
        {
            decimal result = EvaluateFactor(expression, ref index);

            while (index < expression.Length)
            {
                char op = expression[index];
                if (op == '*' || op == '/')
                {
                    index++;
                    decimal nextFactor = EvaluateFactor(expression, ref index);

                    if (op == '*')
                        result = _calculator.Multiply(result, nextFactor);
                    else
                        result = _calculator.Divide(result, nextFactor);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private decimal EvaluateFactor(string expression, ref int index)
        {
            decimal result = EvaluatePower(expression, ref index);

            // Факториал имеет высший приоритет после степени
            while (index < expression.Length && expression[index] == '!')
            {
                index++;
                result = Factorial(result);
            }

            return result;
        }

        private decimal EvaluatePower(string expression, ref int index)
        {
            decimal result = EvaluateBaseFactor(expression, ref index);

            // Обрабатываем возведение в степень (правоассоциативная операция)
            while (index < expression.Length && expression[index] == '^')
            {
                index++;
                decimal exponent = EvaluatePower(expression, ref index); // Рекурсивно для правоассоциативности
                result = Power(result, exponent);
            }

            return result;
        }

        private decimal EvaluateBaseFactor(string expression, ref int index)
        {
            if (index >= expression.Length)
                throw new ArgumentException("Неожиданный конец выражения");

            // Пропускаем пробелы (хотя мы их удалили, но на всякий случай)
            while (index < expression.Length && char.IsWhiteSpace(expression[index]))
                index++;

            // Проверяем функции
            if (index + 2 < expression.Length)
            {
                string function = expression.Substring(index, 3).ToLower();
                if (function == "sin" || function == "cos" || function == "tan" || function == "ctg")
                {
                    index += 3;
                    if (index >= expression.Length || expression[index] != '(')
                        throw new ArgumentException($"После функции {function} ожидалась открывающая скобка");

                    index++; // Пропускаем '('
                    decimal argument = EvaluateExpressionRecursive(expression, ref index);

                    if (index >= expression.Length || expression[index] != ')')
                        throw new ArgumentException($"После аргумента функции {function} ожидалась закрывающая скобка");

                    index++; // Пропускаем ')'

                    // Конвертируем градусы в радианы для тригонометрических функций
                    decimal radians = argument * PI / 180.0m;

                    return function switch
                    {
                        "sin" => (decimal)Math.Sin((double)radians),
                        "cos" => (decimal)Math.Cos((double)radians),
                        "tan" => (decimal)Math.Tan((double)radians),
                        "ctg" => 1.0m / (decimal)Math.Tan((double)radians),
                        _ => throw new ArgumentException($"Неизвестная функция: {function}")
                    };
                }
            }

            if (char.IsDigit(expression[index]) || expression[index] == '.' ||
                expression[index] == '(' || expression[index] == '-')
            {
                return EvaluateSimpleFactor(expression, ref index);
            }
            else if (expression[index] == 'e' || expression[index] == 'E')
            {
                // Проверяем, является ли это частью числа с экспонентой или константой e
                if (index > 0 && char.IsDigit(expression[index - 1]))
                {
                    // Это часть числа с экспонентой, обрабатываем в ReadNumber
                    return ReadNumber(expression, ref index);
                }
                else
                {
                    // Это математическая константа e
                    index++;
                    return E;
                }
            }
            else if (expression[index] == 'p' && index + 1 < expression.Length && expression[index + 1] == 'i')
            {
                // Математическая константа pi
                index += 2;
                return PI;
            }
            else
            {
                throw new ArgumentException($"Неожиданный символ: {expression[index]}");
            }
        }

        private decimal EvaluateSimpleFactor(string expression, ref int index)
        {
            if (index >= expression.Length)
                throw new ArgumentException("Неожиданный конец выражения");

            if (expression[index] == '(')
            {
                // Выражение в скобках
                index++; // Пропускаем '('
                decimal result = EvaluateExpressionRecursive(expression, ref index);

                if (index >= expression.Length || expression[index] != ')')
                    throw new ArgumentException("Ожидалась закрывающая скобка");

                index++; // Пропускаем ')'
                return result;
            }
            else if (expression[index] == '-')
            {
                // Унарный минус
                index++;
                decimal factor = EvaluateSimpleFactor(expression, ref index);
                return _calculator.Multiply(-1, factor);
            }
            else if (char.IsDigit(expression[index]) || expression[index] == '.')
            {
                // Число (десятичное, возможно с экспонентой)
                return ReadNumber(expression, ref index);
            }
            else
            {
                throw new ArgumentException($"Неожиданный символ: {expression[index]}");
            }
        }

        private decimal Power(decimal baseValue, decimal exponent)
        {
            // Проверяем особые случаи
            if (exponent == 0) return 1;
            if (exponent == 1) return baseValue;
            if (baseValue == 0) return 0;
            if (baseValue == 1) return 1;

            // Для целых показателей степени используем итеративное умножение
            if (exponent == Math.Floor(exponent) && exponent > 0 && exponent <= 100)
            {
                decimal result = 1;
                for (int i = 0; i < exponent; i++)
                {
                    result *= baseValue;
                }
                return result;
            }

            // Для дробных и отрицательных показателей используем Math.Pow
            try
            {
                return (decimal)Math.Pow((double)baseValue, (double)exponent);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка при возведении в степень: {ex.Message}");
            }
        }

        private decimal Factorial(decimal n)
        {
            if (n < 0 || n != Math.Floor(n))
                throw new ArgumentException("Факториал определен только для целых неотрицательных чисел");

            if (n == 0 || n == 1) return 1;

            decimal result = 1;
            for (decimal i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private decimal ReadNumber(string expression, ref int index)
        {
            int start = index;
            bool hasExponent = false;
            bool hasExponentSign = false;
            bool hasExponentDigits = false;

            // Читаем число с поддержкой экспоненты
            while (index < expression.Length)
            {
                char current = expression[index];

                if (char.IsDigit(current) || current == '.')
                {
                    // Цифры или точка в основной части числа
                    index++;
                    if (hasExponent) hasExponentDigits = true;
                }
                else if ((current == 'e' || current == 'E') && !hasExponent)
                {
                    // Нашли экспоненту
                    hasExponent = true;
                    index++;

                    // Проверяем знак экспоненты если есть
                    if (index < expression.Length && (expression[index] == '+' || expression[index] == '-'))
                    {
                        hasExponentSign = true;
                        index++;
                    }
                }
                else if (char.IsDigit(current) && hasExponent)
                {
                    // Цифры в экспоненте
                    hasExponentDigits = true;
                    index++;
                }
                else
                {
                    break;
                }
            }

            string numberStr = expression.Substring(start, index - start);

            // Если есть экспонента, но нет цифр после нее - это ошибка
            if (hasExponent && !hasExponentDigits)
            {
                throw new ArgumentException($"Неверный формат числа с экспонентой: {numberStr}");
            }

            // Заменяем запятые на точки для корректного парсинга
            numberStr = numberStr.Replace(',', '.');

            // Парсим число с учетом экспоненты
            if (TryParseNumberWithExponent(numberStr, out decimal result))
                return result;
            else
                throw new ArgumentException($"Неверный формат числа: {numberStr}");
        }

        private bool TryParseNumberWithExponent(string numberStr, out decimal result)
        {
            result = 0;

            try
            {
                // Если есть экспонента
                int eIndex = numberStr.IndexOfAny(new[] { 'e', 'E' });
                if (eIndex != -1)
                {
                    string basePart = numberStr.Substring(0, eIndex);
                    string exponentPart = numberStr.Substring(eIndex + 1);

                    if (decimal.TryParse(basePart, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal baseValue) &&
                        int.TryParse(exponentPart, out int exponent))
                    {
                        // Вычисляем число с экспонентой
                        if (exponent >= 0)
                        {
                            result = baseValue * (decimal)Math.Pow(10, exponent);
                        }
                        else
                        {
                            result = baseValue / (decimal)Math.Pow(10, -exponent);
                        }
                        return true;
                    }
                }
                else
                {
                    // Обычное число без экспоненты
                    return decimal.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '^';
        }
    }
}