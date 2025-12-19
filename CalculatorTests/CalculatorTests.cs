using CalculatorCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CalculatorTests
{
    [TestClass]
    public class ExpressionEvaluationTests
    {
        private readonly CalculatorService _calculatorService;

        public ExpressionEvaluationTests()
        {
            _calculatorService = new CalculatorService(new Calculator());
        }

        // ТЕСТЫ ДЛЯ СКОБОК
        [TestMethod]
        [DataRow("(2+3)*4", 20)]
        [DataRow("2+(3*4)", 14)]
        [DataRow("(1+2)*(3+4)", 21)]
        [DataRow("((2+3)*4)/2", 10)]
        [DataRow("-(2+3)", -5)]
        [DataRow("(-2+3)*4", 4)]
        [DataRow("2*(3+4*(5-2))", 30)]
        public void EvaluateExpression_WithParentheses_ShouldReturnCorrectResult(string expression, decimal expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Value);
        }

        // ТЕСТЫ ДЛЯ ЭКСПОНЕНТ
        [TestMethod]
        [DataRow("2e3", 2000)]
        [DataRow("1.5e2", 150)]
        [DataRow("1.5e-2", 0.015)]
        [DataRow("2.5e+3", 2500)]
        [DataRow("1e0", 1)]
        [DataRow("1.23e-4", 0.000123)]
        [DataRow("10e3*2", 20000)]
        [DataRow("1.5e2+100", 250)]
        public void EvaluateExpression_WithExponents_ShouldReturnCorrectResult(string expression, decimal expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Value, 0.0000001m); // Допуск для decimal
        }

        // ТЕСТЫ ДЛЯ МАТЕМАТИЧЕСКИХ КОНСТАНТ
        [TestMethod]
        [DataRow("e", 2.71828182845904523536)]
        [DataRow("pi", 3.14159265358979323846)]
        [DataRow("2*e", 5.43656365691809047072)]
        [DataRow("pi/2", 1.57079632679489661923)]
        [DataRow("e+pi", 5.85987448204883847382)]
        [DataRow("2*pi", 6.28318530717958647692)]
        public void EvaluateExpression_WithConstants_ShouldReturnCorrectResult(string expression, decimal expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Value, 0.0000001m);
        }

       
        // ТЕСТЫ ТРИГОНОМЕТРИЧЕСКИХ ФУНКЦИЙ
        [TestMethod]
        [DataRow("sin(30)", 0.5)]
        [DataRow("cos(60)", 0.5)]
        [DataRow("tan(45)", 1.0)]
        [DataRow("ctg(45)", 1.0)]
        public void EvaluateExpression_TrigonometricFunctions_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value, 0.0000000001m);
        }

        // ТЕСТЫ ДЛЯ СЛОЖНЫХ ВЫРАЖЕНИЙ С ТРИГОНОМЕТРИЕЙ
        [TestMethod]
        [DataRow("sin(30)+cos(60)", 1.0)]           // 0.5 + 0.5 = 1
        [DataRow("2*sin(30)", 1.0)]                 // 2 * 0.5 = 1
        [DataRow("sin(45)*cos(45)", 0.5)]           // 0.7071 * 0.7071 ≈ 0.5
        [DataRow("tan(45)*ctg(45)", 1.0)]           // 1 * 1 = 1
        [DataRow("sin(30+60)", 1.0)]                // sin(90) = 1
        [DataRow("cos(2*30)", 0.5)]                 // cos(60) = 0.5
        [DataRow("sin(0)", 0.0)]                    // sin(0) = 0
        [DataRow("cos(0)", 1.0)]                    // cos(0) = 1
        [DataRow("tan(0)", 0.0)]                    // tan(0) = 0
        [DataRow("sin(90)", 1.0)]                   // sin(90) = 1
        [DataRow("cos(90)", 0.0)]                   // cos(90) = 0
        [DataRow("sin(180)", 0.0)]                  // sin(180) = 0
        public void EvaluateExpression_ComplexTrigonometric_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsTrue(result.Success, $"Выражение '{expression}' должно быть успешным. Ошибка: {result.ErrorMessage}");
            Assert.AreEqual((decimal)expected, result.Value, 0.0001m,
                $"Выражение: {expression}. Ожидалось: {expected}, Получено: {result.Value}");
        }

        // ТЕСТЫ ДЛЯ ФАКТОРИАЛА
        [TestMethod]
        [DataRow("5!", 120)]
        [DataRow("3!", 6)]
        [DataRow("1!", 1)]
        [DataRow("0!", 1)]
        [DataRow("3!*2", 12)]
        [DataRow("(2+3)!", 120)]
        [DataRow("2!+3!", 8)]
        public void EvaluateExpression_WithFactorial_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value);
        }

        // ТЕСТЫ ДЛЯ КОМБИНИРОВАННЫХ ВЫРАЖЕНИЙ
        [TestMethod]
        [DataRow("2e3+100", 2100.0)]
        [DataRow("sin(30)*100", 50.0)]
        [DataRow("(2e3+100)*2", 4200.0)]
        [DataRow("sin(45)*cos(45)*100", 50.0)]
        [DataRow("2*pi*10", 62.8318530717958647692)]
        [DataRow("e*10", 27.1828182845904523536)]
        public void EvaluateExpression_CombinedOperations_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsTrue(result.Success, $"Выражение '{expression}' должно быть успешным. Ошибка: {result.ErrorMessage}");
            Assert.AreEqual((decimal)expected, result.Value, 0.0000000001m,
                $"Выражение: {expression}. Ожидалось: {expected}, Получено: {result.Value}");
        }

        // ТЕСТЫ НА ПРИОРИТЕТ ОПЕРАЦИЙ
        [TestMethod]
        [DataRow("2+3*4", 14.0)] // Умножение перед сложением: 2 + (3*4) = 2 + 12 = 14
        [DataRow("(2+3)*4", 20.0)] // Скобки меняют приоритет: (2+3)*4 = 5*4 = 20
        [DataRow("10/2*5", 25.0)] // Левая ассоциативность: (10/2)*5 = 5*5 = 25
        [DataRow("10*2/5", 4.0)] // Левая ассоциативность: (10*2)/5 = 20/5 = 4
        [DataRow("2+3*4-1", 13.0)] // Смешанные операции: 2 + (3*4) - 1 = 2 + 12 - 1 = 13
        [DataRow("2*3+4*5", 26.0)] // Приоритет умножения: (2*3) + (4*5) = 6 + 20 = 26
        [DataRow("2^3*4", 32.0)] // Степень перед умножением: (2^3)*4 = 8*4 = 32
        [DataRow("2*3^2", 18.0)] // Степень перед умножением: 2*(3^2) = 2*9 = 18
        [DataRow("5!+1", 121.0)] // Факториал перед сложением: (5!)+1 = 120+1 = 121
        [DataRow("sin(30)*2", 1.0)] // Функция перед умножением: sin(30)*2 = 0.5*2 = 1
        public void EvaluateExpression_OperatorPrecedence_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsTrue(result.Success, $"Выражение '{expression}' должно быть успешным. Ошибка: {result.ErrorMessage}");
            Assert.AreEqual((decimal)expected, result.Value, $"Выражение: {expression}. Ожидалось: {expected}, Получено: {result.Value}");
        }

        // ТЕСТЫ НА ОШИБКИ ФОРМАТА
        [TestMethod]
        [DataRow("1 2")] // Пробелы между числами
        [DataRow("(2+3")] // Незакрытая скобка
        [DataRow("2+3)")] // Неоткрытая скобка
        [DataRow("sin")] // Неполная функция
        [DataRow("sin(")] // Незакрытая функция
        [DataRow("2.3.4")] // Неверный формат числа
        [DataRow("1e")] // Неполная экспонента
        [DataRow("1e+")] // Неполная экспонента со знаком
        [DataRow("(-2)!")] // Факториал отрицательного числа
        [DataRow("2.5!")] // Факториал нецелого числа
        public void EvaluateExpression_InvalidFormat_ShouldReturnFailure(string expression)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsFalse(result.Success);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
        }

        // ТЕСТЫ ДЛЯ УНАРНОГО МИНУСА
        [TestMethod]
        [DataRow("-5", -5.0)]
        [DataRow("-(2+3)", -5.0)]
        [DataRow("-2*3", -6.0)]
        [DataRow("2*-3", -6.0)]
        public void EvaluateExpression_UnaryMinus_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);

            Assert.IsTrue(result.Success, $"Выражение '{expression}' должно быть успешным. Ошибка: {result.ErrorMessage}");
            Assert.AreEqual((decimal)expected, result.Value, 0.0000001m,
                $"Выражение: {expression}. Ожидалось: {expected}, Получено: {result.Value}");
        }
    }

    [TestClass]
    public class EdgeCaseTests
    {
        private readonly CalculatorService _calculatorService;

        public EdgeCaseTests()
        {
            _calculatorService = new CalculatorService(new Calculator());
        }

        // ТЕСТЫ ГРАНИЧНЫХ СЛУЧАЕВ
        [TestMethod]
        public void EvaluateExpression_EmptyString_ShouldReturnFailure()
        {
            var result = _calculatorService.EvaluateExpression("");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Пустое выражение", result.ErrorMessage);
        }

        [TestMethod]
        public void EvaluateExpression_OnlySpaces_ShouldReturnFailure()
        {
            var result = _calculatorService.EvaluateExpression("   ");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Пустое выражение", result.ErrorMessage);
        }

        [TestMethod]
        public void EvaluateExpression_SingleNumber_ShouldReturnCorrectResult()
        {
            var result = _calculatorService.EvaluateExpression("42");

            Assert.IsTrue(result.Success);
            Assert.AreEqual(42, result.Value);
        }

        [TestMethod]
        public void EvaluateExpression_VeryLargeExponent_ShouldHandleCorrectly()
        {
            var result = _calculatorService.EvaluateExpression("1e10");

            Assert.IsTrue(result.Success);
            Assert.AreEqual(10000000000, result.Value);
        }

        [TestMethod]
        public void EvaluateExpression_VerySmallExponent_ShouldHandleCorrectly()
        {
            var result = _calculatorService.EvaluateExpression("1e-10");

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0.0000000001m, result.Value);
        }
    }
    [TestClass]
    public class CalculatorTests
    {
        private readonly Calculator _calculator = new Calculator();

        // ТЕСТЫ СЛОЖЕНИЯ (ADD)
        [TestMethod]
        [DataRow(5, 3, 8)]
        [DataRow(-2, 3, 1)]
        [DataRow(0, 0, 0)]
        [DataRow(-5, -3, -8)]
        [DataRow(0, 5, 5)]
        public void Add_ShouldReturnCorrectResult(int a, int b, int expected)
        {
            var result = _calculator.Add(a, b);
            Assert.AreEqual(expected, result);
        }

        // ТЕСТЫ ВЫЧИТАНИЯ (SUBTRACT)
        [TestMethod]
        [DataRow(5, 3, 2)]
        [DataRow(3, 5, -2)]
        [DataRow(0, 0, 0)]
        [DataRow(-5, -3, -2)]
        [DataRow(10, 0, 10)]
        public void Subtract_ShouldReturnCorrectResult(int a, int b, int expected)
        {
            var result = _calculator.Subtract(a, b);
            Assert.AreEqual(expected, result);
        }

        // ТЕСТЫ УМНОЖЕНИЯ (MULTIPLY)
        [TestMethod]
        [DataRow(5, 3, 15)]
        [DataRow(-2, 3, -6)]
        [DataRow(0, 5, 0)]
        [DataRow(-3, -4, 12)]
        [DataRow(1, 1, 1)]
        public void Multiply_ShouldReturnCorrectResult(int a, int b, int expected)
        {
            var result = _calculator.Multiply(a, b);
            Assert.AreEqual(expected, result);
        }

        // ТЕСТЫ ДЕЛЕНИЯ (DIVIDE)
        [TestMethod]
        [DataRow(10, 2, 5)]
        [DataRow(9, 3, 3)]
        [DataRow(-12, 4, -3)]
        [DataRow(0, 5, 0)]
        public void Divide_ShouldReturnCorrectResult(int a, int b, int expected)
        {
            var result = _calculator.Divide(a, b);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Divide_ByZero_ShouldThrowDivideByZeroException()
        {
            _calculator.Divide(10, 0);
        }

        // ТЕСТЫ МЕТОДА TRYDIVIDE
        [TestMethod]
        [DataRow(10, 2, true, 5)]
        [DataRow(9, 3, true, 3)]
        [DataRow(0, 5, true, 0)]
        public void TryDivide_ValidDivision_ShouldReturnTrueAndCorrectResult(int a, int b, bool expectedSuccess, int expectedResult)
        {
            var success = _calculator.TryDivide(a, b, out var result);
            Assert.AreEqual(expectedSuccess, success);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [DataRow(10, 0, false, 0)]
        [DataRow(5, 0, false, 0)]
        public void TryDivide_DivisionByZero_ShouldReturnFalse(int a, int b, bool expectedSuccess, int expectedResult)
        {
            var success = _calculator.TryDivide(a, b, out var result);
            Assert.AreEqual(expectedSuccess, success);
            Assert.AreEqual(expectedResult, result);
        }
    }

    [TestClass]
    public class CalculatorServiceTests
    {
        private readonly CalculatorService _calculatorService;

        public CalculatorServiceTests()
        {
            _calculatorService = new CalculatorService(new Calculator());
        }

        // ТЕСТЫ БАЗОВЫХ ОПЕРАЦИЙ
        [TestMethod]
        [DataRow("2+3", 5)]
        [DataRow("5-3", 2)]
        [DataRow("2*3", 6)]
        [DataRow("10/2", 5)]
        [DataRow("1+2*3", 7)]
        [DataRow("(1+2)*3", 9)]
        public void EvaluateExpression_BasicOperations_ShouldReturnCorrectResult(string expression, int expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(expected, result.Value);
        }

        // ТЕСТЫ ОПЕРАЦИИ СТЕПЕНИ - используем decimal литералы
        [TestMethod]
        [DataRow("2^3", 8.0)]
        [DataRow("4^0.5", 2.0)]
        [DataRow("10^-2", 0.01)]
        [DataRow("(2+3)^2", 25.0)]
        [DataRow("2^-3", 0.125)]
        [DataRow("2^10", 1024.0)]
        [DataRow("0.5^2", 0.25)]
        [DataRow("3^0", 1.0)]
        [DataRow("1^100", 1.0)]
        public void EvaluateExpression_PowerOperator_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value, 0.0000000001m);
        }

        // ТЕСТЫ ПРАВОАССОЦИАТИВНОСТИ СТЕПЕНИ
        [TestMethod]
        [DataRow("2^3^2", 512.0)] // 2^(3^2) = 2^9 = 512
        [DataRow("2^2^3", 256.0)] // 2^(2^3) = 2^8 = 256
        public void EvaluateExpression_PowerRightAssociativity_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value);
        }

        // ТЕСТЫ КОМБИНАЦИЙ СТЕПЕНИ С ДРУГИМИ ОПЕРАЦИЯМИ
        [TestMethod]
        [DataRow("2^3+1", 9.0)] // 8 + 1 = 9
        [DataRow("1+2^3", 9.0)] // 1 + 8 = 9
        [DataRow("2^3*2", 16.0)] // 8 * 2 = 16
        [DataRow("2*3^2", 18.0)] // 2 * 9 = 18
        public void EvaluateExpression_PowerWithOtherOperations_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value);
        }

        // ТЕСТЫ ТРИГОНОМЕТРИЧЕСКИХ ФУНКЦИЙ
        [TestMethod]
        [DataRow("sin(30)", 0.5)]
        [DataRow("cos(60)", 0.5)]
        [DataRow("tan(45)", 1.0)]
        [DataRow("ctg(45)", 1.0)]
        public void EvaluateExpression_TrigonometricFunctions_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value, 0.0000000001m);
        }

        // ТЕСТЫ ФАКТОРИАЛА
        [TestMethod]
        [DataRow("5!", 120.0)]
        [DataRow("3!", 6.0)]
        [DataRow("0!", 1.0)]
        public void EvaluateExpression_Factorial_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value);
        }

        // ТЕСТЫ КОНСТАНТ
        [TestMethod]
        [DataRow("e", 2.71828182845904523536)]
        [DataRow("pi", 3.14159265358979323846)]
        [DataRow("2*e", 5.43656365691809047072)]
        [DataRow("2*pi", 6.28318530717958647692)]
        public void EvaluateExpression_Constants_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value, 0.0000000001m);
        }

        // ТЕСТЫ ЧИСЕЛ С ЭКСПОНЕНТОЙ
        [TestMethod]
        [DataRow("12e3", 12000.0)]
        [DataRow("1.23e-5", 0.0000123)]
        [DataRow("2.5e2", 250.0)]
        public void EvaluateExpression_ExponentNumbers_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value, 0.0000000001m);
        }

        // ТЕСТЫ ОШИБОК - исправленные примеры
        [TestMethod]
        [DataRow("1 2")] // Пробелы между числами
        [DataRow("2^")] // Неполное выражение
        [DataRow("^3")] // Отсутствует основание
        [DataRow("sin(")] // Незакрытая скобка
        [DataRow("5!!")] // Двойной факториал (не поддерживается)
        [DataRow("2^^3")] // Двойной оператор степени
        [DataRow("sin")] // Функция без скобок
        [DataRow("")] // Пустая строка
        public void EvaluateExpression_InvalidExpressions_ShouldReturnError(string expression)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsFalse(result.Success, $"Выражение '{expression}' должно возвращать ошибку, но вернуло: {result.Value}");
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
        }

        // ТЕСТЫ ДЕЛЕНИЯ НА НОЛЬ
        [TestMethod]
        public void EvaluateExpression_DivisionByZero_ShouldReturnError()
        {
            var result = _calculatorService.EvaluateExpression("10/0");
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("ноль") || result.ErrorMessage.Contains("zero") || result.ErrorMessage.Contains("делен"));
        }

        // ТЕСТЫ PERFORMOPERATION
        [TestMethod]
        [DataRow("+", 5, 3, 8)]
        [DataRow("-", 5, 3, 2)]
        [DataRow("*", 5, 3, 15)]
        [DataRow("/", 10, 2, 5)]
        [DataRow("^", 2, 3, 8)]  // Тест степени через PerformOperation
        public void PerformOperation_ValidOperations_ShouldReturnSuccessResult(string operation, int a, int b, int expected)
        {
            var result = _calculatorService.PerformOperation(operation, a, b);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Value);
        }

        [TestMethod]
        public void PerformOperation_DivideByZero_ShouldReturnFailureResult()
        {
            var result = _calculatorService.PerformOperation("/", 10, 0);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Division by zero", result.ErrorMessage);
        }

        [TestMethod]
        public void PerformOperation_UnknownOperation_ShouldReturnFailureResult()
        {
            var result = _calculatorService.PerformOperation("unknown", 5, 3);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Unknown operation", result.ErrorMessage);
        }
    }

    [TestClass]
    public class CalculatorResultTests
    {
        [TestMethod]
        public void CalculatorResult_Ok_ShouldCreateSuccessResult()
        {
            var result = CalculatorResult.Ok(42);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(42, result.Value);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
        }

        [TestMethod]
        public void CalculatorResult_Fail_ShouldCreateFailureResult()
        {
            var result = CalculatorResult.Fail("Test error");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Test error", result.ErrorMessage);
            Assert.AreEqual(0, result.Value);
        }
    }
}