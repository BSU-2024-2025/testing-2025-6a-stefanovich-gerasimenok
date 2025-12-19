using CalculatorCore;
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Калькулятор ===");
        Console.WriteLine("Выберите режим:");
        Console.WriteLine("1. Простой калькулятор");
        Console.WriteLine("2. Расширенный калькулятор (переменные, циклы, условия)");
        Console.Write("Ваш выбор (1-2): ");

        var choice = Console.ReadLine();

        if (choice == "2")
        {
            RunAdvancedCalculator();
        }
        else
        {
            RunSimpleCalculator();
        }
    }

    static void RunSimpleCalculator()
    {
        var calculator = new Calculator();
        var calculatorService = new CalculatorService(calculator);

        Console.WriteLine("\n=== Простой калькулятор ===");
        Console.WriteLine("Доступные операции: +, -, *, /, ^ (степень), скобки (), факториал (!)");
        Console.WriteLine("Тригонометрические функции (в градусах): sin, cos, tan, ctg");
        Console.WriteLine("Константы: e (2.71828...), pi (3.14159...)");
        Console.WriteLine("Поддерживаются числа с экспонентой: 12e3 = 12000, 1.23e-5 = 0.0000123");
        Console.WriteLine("Примеры:");
        Console.WriteLine("  5+3, (2+3)*4, 10/2, -1*(-2)");
        Console.WriteLine("  2^3 (8), 4^0.5 (2), 10^-2 (0.01)");
        Console.WriteLine("  sin(30), cos(45), tan(60), ctg(45)");
        Console.WriteLine("  5!, 3!*2, sin(90)+cos(0)");
        Console.WriteLine("  12e3 (12000), 1.23e-5 (0.0000123), 12*e (32.619)");
        Console.WriteLine("  pi, 2*pi, sin(pi/2)");
        Console.WriteLine("  (2+3)^2 (25), 2^3^2 (512)");
        Console.WriteLine("Пробелы между числами не допускаются!");
        Console.WriteLine("Введите 'exit' для выхода");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Введите выражение: ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                break;

            try
            {
                var result = calculatorService.EvaluateExpression(input);

                if (result.Success)
                    Console.WriteLine($"Результат: {result.Value}");
                else
                    Console.WriteLine($"Ошибка: {result.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    static void RunAdvancedCalculator()
    {
        var calculator = new AdvancedCalculator();
        var calculatorService = new CalculatorService(calculator);

        Console.WriteLine("\n=== Расширенный калькулятор ===");
        Console.WriteLine("Поддерживаются все функции простого калькулятора + дополнительные возможности:");
        Console.WriteLine("- Переменные: x=5; y=x+3");
        Console.WriteLine("- Условные операторы: if (x > 3) { y=10; } else { y=20; }");
        Console.WriteLine("- Циклы: while (x < 10) { x=x+1; }");
        Console.WriteLine("- Функция возврата: return 5+3");
        Console.WriteLine("- Операторы сравнения: ==, !=, >, <, >=, <=");
        Console.WriteLine("- Остаток от деления: 10 % 3");
        Console.WriteLine("- Экспоненциальная функция: exp(1)");
        Console.WriteLine("- Комментарии: // это комментарий");
        Console.WriteLine("Примеры:");
        Console.WriteLine("  x=5; y=3; x*y");
        Console.WriteLine("  if (5>3) { 10; } else { 20; }");
        Console.WriteLine("  x=0; while (x < 3) { x=x+1; } x");
        Console.WriteLine("  return 2+3");
        Console.WriteLine("Введите 'exit' для выхода");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Введите выражение: ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                break;

            try
            {
                var result = calculatorService.EvaluateExpression(input);

                if (result.Success)
                    Console.WriteLine($"Результат: {result.Value}");
                else
                    Console.WriteLine($"Ошибка: {result.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
            }

            Console.WriteLine();
        }
    }
}