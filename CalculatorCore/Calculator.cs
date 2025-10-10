using System;

namespace CalculatorCore
{
    public class Calculator : ICalculator
    {
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
    }
}