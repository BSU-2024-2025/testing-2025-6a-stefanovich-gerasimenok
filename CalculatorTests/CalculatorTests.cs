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
        [DataRow(1, 4, 0.25)]
        public void Divide_ShouldReturnCorrectResult(int a, int b, double expected)
        {
            var result = _calculator.Divide(a, b);

            Assert.AreEqual((decimal)expected, result);
        }

        // ТЕСТ ИСКЛЮЧЕНИЯ ПРИ ДЕЛЕНИИ НА НОЛЬ
        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Divide_ByZero_ShouldThrowDivideByZeroException()
        {
            decimal a = 10;
            decimal b = 0;

            _calculator.Divide(a, b);
        }

        // ТЕСТЫ МЕТОДА TRYDIVIDE (УСПЕШНЫЕ СЦЕНАРИИ)
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

        // ТЕСТЫ МЕТОДА TRYDIVIDE (ДЕЛЕНИЕ НА НОЛЬ)
        [TestMethod]
        [DataRow(10, 0, false, 0)]
        [DataRow(5, 0, false, 0)]
        [DataRow(-3, 0, false, 0)]
        public void TryDivide_DivisionByZero_ShouldReturnFalse(int a, int b, bool expectedSuccess, int expectedResult)
        {
            var success = _calculator.TryDivide(a, b, out var result);

            Assert.AreEqual(expectedSuccess, success);
            Assert.AreEqual(expectedResult, result);
        }

        // Отдельные тесты для decimal значений
        [TestMethod]
        public void Add_DecimalNumbers_ShouldReturnCorrectResult()
        {
            decimal a = 1.5m;
            decimal b = 2.5m;
            decimal expected = 4.0m;

            var result = _calculator.Add(a, b);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Subtract_DecimalNumbers_ShouldReturnCorrectResult()
        {
            // Тест вычитания decimal чисел
            decimal a = 5.5m;
            decimal b = 2.5m;
            decimal expected = 3.0m;

            var result = _calculator.Subtract(a, b);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Multiply_DecimalNumbers_ShouldReturnCorrectResult()
        {
            // Тест умножения decimal чисел
            decimal a = 2.5m;
            decimal b = 4.0m;
            decimal expected = 10.0m;

            var result = _calculator.Multiply(a, b);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Divide_DecimalNumbers_ShouldReturnCorrectResult()
        {
            // Тест деления decimal чисел
            decimal a = 7.0m;
            decimal b = 2.0m;
            decimal expected = 3.5m;

            var result = _calculator.Divide(a, b);

            Assert.AreEqual(expected, result);
        }
    }

    [TestClass]
    public class CalculatorServiceTests
    {
        // Тестовый калькулятор для сервиса
        private class TestCalculator : ICalculator
        {
            public decimal Add(decimal a, decimal b) => a + b;
            public decimal Subtract(decimal a, decimal b) => a - b;
            public decimal Multiply(decimal a, decimal b) => a * b;
            public decimal Divide(decimal a, decimal b) => a / b;

            public bool TryDivide(decimal a, decimal b, out decimal result)
            {
                if (b == 0)
                {
                    result = 0;
                    return false;
                }
                result = a / b;
                return true;
            }
        }

        private readonly CalculatorService _calculatorService;

        public CalculatorServiceTests()
        {
            _calculatorService = new CalculatorService(new TestCalculator());
        }

        // ТЕСТЫ ВЫПОЛНЕНИЯ ОПЕРАЦИЙ ЧЕРЕЗ СЕРВИС
        [TestMethod]
        [DataRow("add", 5, 3, 8)]
        [DataRow("ADD", 5, 3, 8)]
        [DataRow("+", 5, 3, 8)]
        [DataRow("subtract", 5, 3, 2)]
        [DataRow("SUBTRACT", 5, 3, 2)]
        [DataRow("-", 5, 3, 2)]
        [DataRow("multiply", 5, 3, 15)]
        [DataRow("MULTIPLY", 5, 3, 15)]
        [DataRow("*", 5, 3, 15)]
        [DataRow("divide", 10, 2, 5)]
        [DataRow("DIVIDE", 10, 2, 5)]
        [DataRow("/", 10, 2, 5)]
        public void PerformOperation_ValidOperations_ShouldReturnSuccessResult(string operation, int a, int b, int expected)
        {
            var result = _calculatorService.PerformOperation(operation, a, b);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Value);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
        }

        // ТЕСТЫ ДЕЛЕНИЯ НА НОЛЬ ЧЕРЕЗ СЕРВИС
        [TestMethod]
        [DataRow("divide", 10, 0)]
        [DataRow("DIVIDE", 10, 0)]
        [DataRow("/", 10, 0)]
        public void PerformOperation_DivideByZero_ShouldReturnFailureResult(string operation, int a, int b)
        {
            var result = _calculatorService.PerformOperation(operation, a, b);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Division by zero", result.ErrorMessage);
            Assert.AreEqual(0, result.Value);
        }

        // ТЕСТЫ НЕИЗВЕСТНЫХ ОПЕРАЦИЙ
        [TestMethod]
        [DataRow("unknown", 5, 3)]
        [DataRow("power", 2, 3)]
        [DataRow("mod", 10, 3)]
        public void PerformOperation_UnknownOperation_ShouldReturnFailureResult(string operation, int a, int b)
        {
            var result = _calculatorService.PerformOperation(operation, a, b);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Unknown operation", result.ErrorMessage);
            Assert.AreEqual(0, result.Value);
        }

        // ТЕСТ DECIMAL ЧИСЕЛ ЧЕРЕЗ СЕРВИС
        [TestMethod]
        public void PerformOperation_DecimalNumbers_ShouldReturnCorrectResult()
        {
            // Тест сложения decimal чисел через сервис
            string operation = "add";
            decimal a = 1.5m;
            decimal b = 2.5m;
            decimal expected = 4.0m;

            var result = _calculatorService.PerformOperation(operation, a, b);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Value);
        }

        // ТЕСТ ПУСТОЙ ОПЕРАЦИИ
        [TestMethod]
        public void PerformOperation_EmptyOperation_ShouldReturnFailureResult()
        {
            var result = _calculatorService.PerformOperation("", 1, 2);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Unknown operation", result.ErrorMessage);
        }
    }

    [TestClass]
    public class CalculatorResultTests
    {
        // ТЕСТЫ ДЛЯ КЛАССА CalculatorResult
        [TestMethod]
        public void CalculatorResult_Ok_ShouldCreateSuccessResult()
        {
            // Тест создания успешного результата
            decimal expectedValue = 42;

            var result = CalculatorResult.Ok(expectedValue);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(expectedValue, result.Value);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
        }

        [TestMethod]
        public void CalculatorResult_Fail_ShouldCreateFailureResult()
        {
            // Тест создания неуспешного результата с сообщением
            string expectedError = "Test error";

            var result = CalculatorResult.Fail(expectedError);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(expectedError, result.ErrorMessage);
            Assert.AreEqual(0, result.Value);
        }

        [TestMethod]
        public void CalculatorResult_Fail_WithEmptyMessage_ShouldWorkCorrectly()
        {
            // Тест создания неуспешного результата с пустым сообщением
            var result = CalculatorResult.Fail("");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("", result.ErrorMessage);
        }

        [TestMethod]
        public void CalculatorResult_Fail_WithNullMessage_ShouldWorkCorrectly()
        {
            // Тест создания неуспешного результата с null сообщением
            var result = CalculatorResult.Fail(null);

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.ErrorMessage);
        }
    }
}