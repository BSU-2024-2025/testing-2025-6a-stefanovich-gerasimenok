using CalculatorCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CalculatorTests
{
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