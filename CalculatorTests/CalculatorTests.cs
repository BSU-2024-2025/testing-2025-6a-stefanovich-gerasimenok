using CalculatorCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

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

        // ТЕСТЫ ОПЕРАЦИИ СТЕПЕНИ
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
            Assert.AreEqual((decimal)expected, result.Value, 0.0001m);
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
            Assert.AreEqual((decimal)expected, result.Value, 0.0001m);
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
        [DataRow("e", 2.718281828)]
        [DataRow("pi", 3.141592654)]
        [DataRow("2*e", 5.436563656)]
        [DataRow("2*pi", 6.283185307)]
        public void EvaluateExpression_Constants_ShouldReturnCorrectResult(string expression, double expected)
        {
            var result = _calculatorService.EvaluateExpression(expression);
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual((decimal)expected, result.Value, 0.0001m);
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
            Assert.AreEqual((decimal)expected, result.Value, 0.00000001m);
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

    [TestClass]
    public class AdvancedCalculatorTests
    {
        private readonly AdvancedCalculator _calculator = new AdvancedCalculator();
        private readonly CalculatorService _calculatorService;

        public AdvancedCalculatorTests()
        {
            _calculatorService = new CalculatorService(_calculator);
        }

        // ТЕСТЫ ПЕРЕМЕННЫХ
        [TestMethod]
        public void Evaluate_VariableAssignment_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=5; x";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(5m, result);
        }

        [TestMethod]
        public void Evaluate_MultipleVariables_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=2; y=3; x+y";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(5m, result);
        }

        [TestMethod]
        public void Evaluate_ReassignVariable_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=5; x=10; x";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(10m, result);
        }

        // ТЕСТЫ IF-ELSE
        [TestMethod]
        public void Evaluate_IfTrue_ReturnsThenBranch()
        {
            // Arrange
            string expression = "if (5>3) { 10; } else { 20; }";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(10m, result);
        }

        [TestMethod]
        public void Evaluate_IfFalse_ReturnsElseBranch()
        {
            // Arrange
            string expression = "if (5<3) { 10; } else { 20; }";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(20m, result);
        }

        [TestMethod]
        public void Evaluate_IfWithVariables_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=5; if (x>3) { y=10; } else { y=20; } y";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(10m, result);
        }

        // ТЕСТЫ WHILE
        [TestMethod]
        public void Evaluate_WhileLoop_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=0; while (x < 3) x=x+1; x";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(3m, result);
        }

        [TestMethod]
        public void Evaluate_WhileWithBody_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=1; y=1; while (x < 5) { y=y*x; x=x+1; } y";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(24m, result); // 1*2*3*4 = 24
        }

        // ТЕСТЫ RETURN
        [TestMethod]
        public void Evaluate_ReturnStatement_ReturnsValue()
        {
            // Arrange
            string expression = "return 5+3";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(8m, result);
        }

        [TestMethod]
        public void Evaluate_ReturnWithVariables_ReturnsValue()
        {
            // Arrange
            string expression = "x=5; y=3; return x*y";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(15m, result);
        }

        // ТЕСТЫ ОПЕРАТОРОВ СРАВНЕНИЯ - упрощенные
      
        [TestMethod]
        public void Evaluate_ComparisonNotEqual_ReturnsCorrectResult()
        {
            Assert.AreEqual(1m, _calculator.Evaluate("5 != 3"));
            Assert.AreEqual(0m, _calculator.Evaluate("5 != 5"));
        }

        [TestMethod]
        public void Evaluate_ComparisonGreater_ReturnsCorrectResult()
        {
            Assert.AreEqual(1m, _calculator.Evaluate("5 > 3"));
            Assert.AreEqual(0m, _calculator.Evaluate("3 > 5"));
        }

        [TestMethod]
        public void Evaluate_ComparisonLess_ReturnsCorrectResult()
        {
            Assert.AreEqual(1m, _calculator.Evaluate("3 < 5"));
            Assert.AreEqual(0m, _calculator.Evaluate("5 < 3"));
        }

        [TestMethod]
        public void Evaluate_ComparisonGreaterOrEqual_ReturnsCorrectResult()
        {
            Assert.AreEqual(1m, _calculator.Evaluate("5 >= 5"));
            Assert.AreEqual(1m, _calculator.Evaluate("5 >= 3"));
            Assert.AreEqual(0m, _calculator.Evaluate("3 >= 5"));
        }

        [TestMethod]
        public void Evaluate_ComparisonLessOrEqual_ReturnsCorrectResult()
        {
            Assert.AreEqual(1m, _calculator.Evaluate("5 <= 5"));
            Assert.AreEqual(1m, _calculator.Evaluate("3 <= 5"));
            Assert.AreEqual(0m, _calculator.Evaluate("5 <= 3"));
        }

        // ТЕСТЫ ОПЕРАТОРА MODULO
        [TestMethod]
        public void Evaluate_ModuloOperator_ReturnsCorrectResult()
        {
            Assert.AreEqual(1m, _calculator.Evaluate("10 % 3"));
            Assert.AreEqual(3m, _calculator.Evaluate("15 % 4"));
            Assert.AreEqual(0m, _calculator.Evaluate("7 % 7"));
        }

        // ТЕСТЫ EXP ФУНКЦИИ
        [TestMethod]
        public void Evaluate_ExpFunction_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "exp(1)";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(2.718281828m, result, 0.0000001m);
        }

        // ТЕСТЫ КОММЕНТАРИЕВ
        [TestMethod]
        public void Evaluate_WithComments_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "2 + 3 // это комментарий";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(5m, result);
        }

        // ТЕСТЫ ПРОМЕЖУТОЧНЫХ РЕЗУЛЬТАТОВ
        [TestMethod]
        public void EvaluateWithIntermediateResults_SimpleExpression_ReturnsResult()
        {
            // Arrange
            string expression = "x=0; while (x < 2) x=x+1; x";

            // Act
            var result = _calculator.EvaluateWithIntermediateResults(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2m, result.FinalResult);
            Assert.IsTrue(result.IntermediateResults.Count > 0);
        }

        // ТЕСТЫ ПЕРЕНОСОВ СТРОК
        [TestMethod]
        public void Evaluate_WithNewLines_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "2 + \n 3";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(5m, result);
        }

        // ТЕСТЫ ОШИБОК
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Evaluate_InvalidVariableName_ThrowsException()
        {
            // Arrange
            string expression = "1var=5";

            // Act
            _calculator.Evaluate(expression);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Evaluate_UnknownFunction_ThrowsException()
        {
            // Arrange
            string expression = "unknown(5)";

            // Act
            _calculator.Evaluate(expression);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Evaluate_UnbalancedParentheses_ThrowsException()
        {
            // Arrange
            string expression = "(2+3";

            // Act
            _calculator.Evaluate(expression);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Evaluate_DivisionByZero_ThrowsException()
        {
            // Arrange
            string expression = "5/0";

            // Act
            _calculator.Evaluate(expression);
        }

        // ТЕСТЫ СЛОЖНЫХ ВЫРАЖЕНИЙ
        [TestMethod]
        public void Evaluate_ComplexExpression1_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=7; y=x+5; if (x>1) {x=5;y=1;} else {y=7;} return y";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(1m, result);
        }

        [TestMethod]
        public void Evaluate_ComplexExpression2_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=2; if (x>1) {y=10;} else {y=0;} return y";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(10m, result);
        }

        [TestMethod]
        public void Evaluate_ComplexExpression3_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=5; if (x>10) {y=1;} else {y=9;} return y";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(9m, result);
        }

        // ТЕСТЫ ПРИОРИТЕТОВ ОПЕРАЦИЙ
        [TestMethod]
        public void Evaluate_OperatorPrecedence_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "2+3*4";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(14m, result);
        }

        // ТЕСТЫ С ПРОБЕЛАМИ
        [TestMethod]
        public void Evaluate_WithSpaces_ReturnsCorrectResult()
        {
            // Arrange
            string expression = " 2 + 3 * 4 ";

            // Act
            decimal result = _calculator.Evaluate(expression);

            // Assert
            Assert.AreEqual(14m, result);
        }

        // ТЕСТЫ CalculatorService С ADVANCED CALCULATOR
        [TestMethod]
        public void CalculatorService_EvaluateExpression_WithVariables_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "x=5; y=3; x*y";

            // Act
            var result = _calculatorService.EvaluateExpression(expression);

            // Assert
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(15m, result.Value);
        }

        [TestMethod]
        public void CalculatorService_EvaluateExpression_WithIf_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "if (5>3) { 10; } else { 20; }";

            // Act
            var result = _calculatorService.EvaluateExpression(expression);

            // Assert
            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(10m, result.Value);
        }
    }
}