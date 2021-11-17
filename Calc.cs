namespace net6_api;
public class Calc
{
    public decimal Calculate(decimal x, decimal y, Operation op)
    {
        switch (op)
        {
            case Operation.Sum:
                return Sum(x, y);
            case Operation.Subtraction:
                return Subtraction(x, y);
            case Operation.Multiplication:
                return Multiplication(x, y);
            case Operation.Division:
                return Division(x, y);
            default:
                return 0;
        }
    }

    private decimal Sum(decimal x, decimal y)
    {
        return x + y;
    }

    public decimal Multiplication(decimal x, decimal y)
    {
        return x * y;
    }

    public decimal Division(decimal x, decimal y)
    {
        return x / y;
    }

    private decimal Subtraction(decimal x, decimal y)
    {
        return x - y;
    }
}