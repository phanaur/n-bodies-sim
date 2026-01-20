using System.Numerics;

namespace NBodiesSim.Source.Core;

public struct Vector2D
{
    public Vector2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static Vector2D Zero => new Vector2D(0, 0);

    private double X { get; set; }

    private double Y { get; set; }

    public static Vector2D operator +(Vector2D operand) => operand;

    public static Vector2D operator -(Vector2D operand) => new Vector2D(-operand.X, -operand.Y);

    public static Vector2D operator +(Vector2D left, Vector2D right) => new Vector2D(left.X + right.X, left.Y + right.Y);

    public static Vector2D operator -(Vector2D left, Vector2D right) => new Vector2D(left.X - right.X, left.Y - right.Y);

    public static Vector2D operator /(Vector2D vector, int escalar)
    {
        return escalar == 0 ? throw new ArgumentException("No se puede dividir entre 0", nameof(escalar)) :
            new Vector2D(vector.X / (double)escalar, vector.Y / (double)escalar);
    }

    public static Vector2D operator /(Vector2D vector, float escalar)
    {
        return escalar == 0 ? throw new ArgumentException("No se puede dividir entre 0", nameof(escalar)) :
            new Vector2D(vector.X / (double)escalar, vector.Y / (double)escalar);
    }

    public static Vector2D operator /(Vector2D vector, decimal escalar)
    {
        return escalar == 0 ? throw new ArgumentException("No se puede dividir entre 0", nameof(escalar)) :
            new Vector2D(vector.X / (double)escalar, vector.Y / (double)escalar);
    }

    public static Vector2D operator /(Vector2D vector, double escalar)
    {
        return escalar == 0 ? throw new ArgumentException("No se puede dividir entre 0", nameof(escalar)) :
            new Vector2D(vector.X / escalar, vector.Y / escalar);
    }

    public static Vector2D operator /(Vector2D left, Vector2D right) => new Vector2D(left.X / right.X, left.Y / right.Y);

    public static Vector2D operator *(Vector2D vector, int escalar) =>
        new Vector2D(vector.X * (double)escalar, vector.Y * (double)escalar);

    public static Vector2D operator *(int escalar, Vector2D vector) =>
        new Vector2D(vector.X * (double)escalar, vector.Y * (double)escalar);

    public static Vector2D operator *(Vector2D vector, float escalar) =>
        new Vector2D(vector.X * (double)escalar, vector.Y * (double)escalar);

    public static Vector2D operator *(float escalar, Vector2D vector) =>
        new Vector2D(vector.X * (double)escalar, vector.Y * (double)escalar);

    public static Vector2D operator *(Vector2D vector, decimal escalar) =>
        new Vector2D(vector.X * (double)escalar, vector.Y * (double)escalar);

    public static Vector2D operator *(decimal escalar, Vector2D vector) =>
        new Vector2D(vector.X * (double)escalar, vector.Y * (double)escalar);

    public static Vector2D operator *(Vector2D vector, double escalar) =>
        new Vector2D(vector.X * escalar, vector.Y * escalar);

    public static Vector2D operator *(double escalar, Vector2D vector) =>
        new Vector2D(vector.X * escalar, vector.Y * escalar);

    public static Vector2D operator *(Vector2D left, Vector2D right) => new Vector2D(left.X * right.X, left.Y * right.Y);

    public static double Dot(Vector2D left, Vector2D right) => (left.X * right.X) + (left.Y * right.Y);

    public static Vector2D One() => new Vector2D(1, 1);

    public static Vector2D UnitX() => new Vector2D(1, 0);

    public static Vector2D UnitY() => new Vector2D(0, 1);

    public static Vector2D Add(Vector2D left, Vector2D right) => new Vector2D(left.X + right.X, left.Y + right.Y);

    public static Vector2D Subtract(Vector2D left, Vector2D right) => new Vector2D(left.X - right.X, left.Y - right.Y);

    public static Vector2D Normalize(Vector2D vector) => vector.Length() < 1e-10 ? Zero :
        new Vector2D(vector.X, vector.Y) / vector.Length();

    public static double Distance(Vector2D left, Vector2D right) =>
        Math.Sqrt(((left.X - right.X) * (left.X - right.X)) + ((left.Y - right.Y) * (left.Y - right.Y)));

    public static double DistanceSquared(Vector2D left, Vector2D right) =>
        ((left.X - right.X) * (left.X - right.X)) + ((left.Y - right.Y) * (left.Y - right.Y));

    public static Vector2D Divide(Vector2D vector, double escalar) =>
        escalar == 0
            ? throw new ArgumentException("No se puede dividir entre 0", nameof(escalar))
            : new Vector2D(vector.X / escalar, vector.Y / escalar);

    public static Vector2D Negate(Vector2D vector) => -vector;

    public static Vector2D Multiply(Vector2D vector, double escalar) => vector * escalar;

    public static Vector2D Multiply(Vector2D vector, float escalar) => vector * escalar;

    public static Vector2D Multiply(Vector2D vector, decimal escalar) => vector * escalar;

    public static Vector2D Multiply(Vector2D vector, int escalar) => vector * escalar;

    public static Vector2D Multiply(double escalar, Vector2D vector) => escalar * vector;

    public static Vector2D Multiply(float escalar, Vector2D vector) => escalar * vector;

    public static Vector2D Multiply(decimal escalar, Vector2D vector) => escalar * vector;

    public static Vector2D Multiply(int escalar, Vector2D vector) => escalar * vector;

    public readonly Vector2D Normalize() => Length() < 1e-10 ? Zero : this / Length();

    public readonly double GetX() => X;

    public readonly double GetY() => Y;

    public readonly double Length() => Math.Sqrt((X * X) + (Y * Y));

    public readonly double Length(Vector2D vector) => Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));

    public readonly double LengthSquared() => (X * X) + (Y * Y);

    public readonly double LengthSquared(Vector2D vector) => (vector.X * vector.X) + (vector.Y * vector.Y);

    public Vector2 ToVector2(float scale) => new Vector2((float)X / scale, (float)Y / scale);

    public Vector2 ToVector2(Vector2D vector, float scale) => new Vector2((float)vector.X / scale, (float)vector.Y / scale);

    public Vector2 ToVector2(float scale, Vector2D vector) => new Vector2((float)vector.X / scale, (float)vector.Y / scale);
}