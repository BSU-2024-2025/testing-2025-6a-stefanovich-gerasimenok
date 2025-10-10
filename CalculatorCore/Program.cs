using CalculatorCore;
using System;

class Program
{
    static void Main(string[] args)
    {
        var calculator = new Calculator();
        var calculatorService = new CalculatorService(calculator);

        Console.WriteLine("=== Калькулятор ===");
        Console.WriteLine("Доступные операции: +, -, *, /, скобки (), факториал (!)");
        Console.WriteLine("Тригонометрические функции (в градусах): sin, cos, tan, ctg");
        Console.WriteLine("Константы: e (2.71828...), pi (3.14159...)");
        Console.WriteLine("Поддерживаются числа с экспонентой: 12e3 = 12000, 1.23e-5 = 0.0000123");
        Console.WriteLine("Примеры:");
        Console.WriteLine("  5+3, (2+3)*4, 10/2, -1*(-2)");
        Console.WriteLine("  sin(30), cos(45), tan(60), ctg(45)");
        Console.WriteLine("  5!, 3!*2, sin(90)+cos(0)");
        Console.WriteLine("  12e3 (12000), 1.23e-5 (0.0000123), 12*e (32.619)");
        Console.WriteLine("  pi, 2*pi, sin(pi/2)");
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

        Console.WriteLine("Работа завершена. Нажмите любую клавишу...");
        Console.ReadKey();
    }
}