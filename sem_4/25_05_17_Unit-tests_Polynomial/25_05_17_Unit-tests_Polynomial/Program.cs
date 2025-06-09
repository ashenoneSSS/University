using System;

class Node
{
    public int Coeff;
    public int Exp;
    public Node Next;

    public Node(int coeff, int exp)
    {
        Coeff = coeff;
        Exp = exp;
        Next = null;
    }
}


public class Polynomial
{
    private Node head;

    public Polynomial()
    {
        head = null;
    }

    public void Insert(int coefficient, int exponent)
    {
        Node newNode = new Node(coefficient, exponent);
        if (head == null || head.Exp < exponent)
        {
            newNode.Next = head;
            head = newNode;
        }
        else
        {
            Node temp = head;
            while (temp.Next != null && temp.Next.Exp >= exponent)
                temp = temp.Next;
            newNode.Next = temp.Next;
            temp.Next = newNode;
        }
    }

    public void AddTerm(int coeff, int exp)
    {
        Node temp = head, prev = null;
        while (temp != null && temp.Exp > exp)
        {
            prev = temp;
            temp = temp.Next;
        }
        if (temp != null && temp.Exp == exp)
        {
            temp.Coeff += coeff;
            if (temp.Coeff == 0)
            {
                if (prev != null) prev.Next = temp.Next;
                else head = temp.Next;
            }
        }
        else
        {
            Node newNode = new Node(coeff, exp);
            if (prev != null)
            {
                newNode.Next = prev.Next;
                prev.Next = newNode;
            }
            else
            {
                newNode.Next = head;
                head = newNode;
            }
        }
    }

    public Polynomial Add(Polynomial that)
    {
        Polynomial result = new Polynomial();
        for (Node temp = head; temp != null; temp = temp.Next)
            result.AddTerm(temp.Coeff, temp.Exp);
        for (Node temp = that.head; temp != null; temp = temp.Next)
            result.AddTerm(temp.Coeff, temp.Exp);
        return result;
    }

    public Polynomial Subtract(Polynomial that)
    {
        Polynomial result = new Polynomial();
        for (Node temp = head; temp != null; temp = temp.Next)
            result.AddTerm(temp.Coeff, temp.Exp);
        for (Node temp = that.head; temp != null; temp = temp.Next)
            result.AddTerm(-temp.Coeff, temp.Exp);
        return result;
    }

    public Polynomial Multiply(Polynomial that)
    {
        Polynomial result = new Polynomial();
        for (Node temp1 = head; temp1 != null; temp1 = temp1.Next)
        {
            for (Node temp2 = that.head; temp2 != null; temp2 = temp2.Next)
            {
                int coeff = temp1.Coeff * temp2.Coeff;
                int exp = temp1.Exp + temp2.Exp;
                result.AddTerm(coeff, exp);
            }
        }
        return result;
    }

    public void Display()
    {
        if (head == null)
        {
            Console.WriteLine("0");
            return;
        }
        Node temp = head;
        while (temp != null)
        {
            Console.Write($"{temp.Coeff}x^{temp.Exp}");
            if (temp.Next != null) Console.Write(" + ");
            temp = temp.Next;
        }
        Console.WriteLine();
    }
}

class Program
{
    static void Main()
    {
        Polynomial p1 = new Polynomial();
        Polynomial p2 = new Polynomial();

        p1.Insert(6, 2);
        p1.Insert(5, 1);
        p1.Insert(12, 3);
        p1.Insert(7, 0);

        p2.Insert(2, 1);
        p2.Insert(4, 2);
        p2.Insert(4, 0);

        Console.WriteLine("P1:");
        p1.Display();
        Console.WriteLine("P2:");
        p2.Display();

        Console.Write("Choose operation (+, -, *): ");
        string op = Console.ReadLine();

        Polynomial result = null;
        if (op == "+") result = p1.Add(p2);
        else if (op == "-") result = p1.Subtract(p2);
        else if (op == "*") result = p1.Multiply(p2);

        if (result == null) Console.WriteLine("Invalid operation");
        else
        {
            Console.WriteLine("Result:");
            result.Display();
        }

        Console.ReadLine();
    }
}
