/*
 * This is the Vector2D data structure, a two-dimensional vector with double precision. It includes operator overloads
 * as well as basic vector operations, such as obtaining its magnitude, the distance between two vectors, etc.
 */
using System.Numerics;

namespace NBodiesSim.Source.Core;

public struct Vector2D
{
    public Vector2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static Vector2D Zero => new Vector2D(0, 0); // Defines a Vector2D with components (0,0)

    public double X { get; set; }

    public double Y { get; set; }

    // Operator overloads
    
    public static Vector2D operator +(Vector2D operand) => operand;

    public static Vector2D operator -(Vector2D operand) => new Vector2D(-operand.X, -operand.Y);

    public static Vector2D operator +(Vector2D left, Vector2D right) => new Vector2D(left.X + right.X, left.Y + right.Y);

    public static Vector2D operator -(Vector2D left, Vector2D right) => new Vector2D(left.X - right.X, left.Y - right.Y);

    public static Vector2D operator /(Vector2D vector, int escalar)
    {
        return escalar == 0 ? throw new ArgumentException("Cannot divide by 0", nameof(escalar)) :
            new Vector2D(vector.X / (double)escalar, vector.Y / (double)escalar);
    }

    public static Vector2D operator /(Vector2D vector, float escalar)
    {
        return escalar == 0 ? throw new ArgumentException("Cannot divide by 0", nameof(escalar)) :
            new Vector2D(vector.X / (double)escalar, vector.Y / (double)escalar);
    }

    public static Vector2D operator /(Vector2D vector, decimal escalar)
    {
        return escalar == 0 ? throw new ArgumentException("Cannot divide by 0", nameof(escalar)) :
            new Vector2D(vector.X / (double)escalar, vector.Y / (double)escalar);
    }

    public static Vector2D operator /(Vector2D vector, double escalar)
    {
        return escalar == 0 ? throw new ArgumentException("Cannot divide by 0", nameof(escalar)) :
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

    // Dot product of two Vector2D vectors
    public static double Dot(Vector2D left, Vector2D right) => (left.X * right.X) + (left.Y * right.Y);
    
    // Returns a Vector2D with components (1,1)

    public static Vector2D One => new Vector2D(1, 1);

    // Unit vector on the X axis
    public static Vector2D UnitX => new Vector2D(1, 0);
    
    // Unit vector on the Y axis
    public static Vector2D UnitY => new Vector2D(0, 1);

    // Addition of two Vector2D vectors
    public static Vector2D Add(Vector2D left, Vector2D right) => new Vector2D(left.X + right.X, left.Y + right.Y);

    // Subtraction of two Vector2D vectors
    public static Vector2D Subtract(Vector2D left, Vector2D right) => new Vector2D(left.X - right.X, left.Y - right.Y);

    // Normalization of a Vector2D vector
    public static Vector2D Normalize(Vector2D vector) => vector.Length() < 1e-10 ? Zero :
        new Vector2D(vector.X, vector.Y) / vector.Length();

    // Distance between two Vector2D vectors
    public static double Distance(Vector2D left, Vector2D right) =>
        Math.Sqrt(((left.X - right.X) * (left.X - right.X)) + ((left.Y - right.Y) * (left.Y - right.Y)));

    // Squared distance between two Vector2D vectors
    public static double DistanceSquared(Vector2D left, Vector2D right) =>
        ((left.X - right.X) * (left.X - right.X)) + ((left.Y - right.Y) * (left.Y - right.Y));

    // Division of Vector2D components by a scalar
    public static Vector2D Divide(Vector2D vector, double escalar) =>
        escalar == 0
            ? throw new ArgumentException("No se puede dividir entre 0", nameof(escalar))
            : new Vector2D(vector.X / escalar, vector.Y / escalar);
    public static Vector2D Divide(double escalar, Vector2D vector) =>
        escalar == 0
            ? throw new ArgumentException("No se puede dividir entre 0", nameof(escalar))
            : new Vector2D(vector.X / escalar, vector.Y / escalar);

    // Negated vector
    public static Vector2D Negate(Vector2D vector) => -vector;

    public Vector2D Negate() => new Vector2D(-X, -Y);

    // Multiplication of Vector2D components by a scalar
    public static Vector2D Multiply(Vector2D vector, double escalar) => vector * escalar;

    public static Vector2D Multiply(Vector2D vector, float escalar) => vector * escalar;

    public static Vector2D Multiply(Vector2D vector, decimal escalar) => vector * escalar;

    public static Vector2D Multiply(Vector2D vector, int escalar) => vector * escalar;

    public static Vector2D Multiply(double escalar, Vector2D vector) => escalar * vector;

    public static Vector2D Multiply(float escalar, Vector2D vector) => escalar * vector;

    public static Vector2D Multiply(decimal escalar, Vector2D vector) => escalar * vector;

    public static Vector2D Multiply(int escalar, Vector2D vector) => escalar * vector;
    
    // Vector normalization
    public readonly Vector2D Normalize() => Length() < 1e-10 ? Zero : this / Length();

    public readonly double Length() => Math.Sqrt((X * X) + (Y * Y));

    public readonly double Length(Vector2D vector) => Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));

    public readonly double LengthSquared() => (X * X) + (Y * Y);

    public readonly double LengthSquared(Vector2D vector) => (vector.X * vector.X) + (vector.Y * vector.Y);

    // Conversion from Vector2D to Vector2 with a scale factor. Intended for use in the simulator
    public Vector2 ToVector2(float scale) => new Vector2((float)X / scale, (float)Y / scale);

    public Vector2 ToVector2(Vector2D vector, float scale) => new Vector2((float)vector.X / scale, (float)vector.Y / scale);

    public Vector2 ToVector2(float scale, Vector2D vector) => new Vector2((float)vector.X / scale, (float)vector.Y / scale);
}
