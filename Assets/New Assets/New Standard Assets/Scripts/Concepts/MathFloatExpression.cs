using Extensions;
using System.Collections.Generic;

public class MathFloatExpression
{
	public MathFloatTerm[] terms = new MathFloatTerm[2];
	public MathOperation[] operations = new MathOperation[1];

	public MathFloatExpression (MathFloatTerm[] terms, MathOperation[] operations)
	{
		this.terms = terms;
		this.operations = operations;
	}

	public float? GetValue ()
	{
		MathFloatExpression expression = new MathFloatExpression(terms, operations);
		float? output = null;
		int[] indicesOfOperation = new List<MathOperation>(operations).GetIndicesOf<MathOperation>(MathOperation.Factorial);
		int removedOperationCount = 0;
		for (int i = 0; i < indicesOfOperation.Length; i ++)
		{
			MathFloatTerm term = terms[i];
			float value = term.number;
			for (int i2 = (int) term.number - 1; i2 > 1; i2 --)
				value *= i;
			output = value;
			expression.operations = expression.operations.RemoveAt(i - removedOperationCount);
		}
		return output;
	}

	public bool OperationsAreInOrder ()
	{
		List<MathOperation> operations = new List<MathOperation>(this.operations);
		bool? isSubtractAfterAdd = operations.AreAllInstancesFoundAfterAllOthers<MathOperation>(new MathOperation[1] { MathOperation.Subtract }, new MathOperation[1] { MathOperation.Add });
		bool? isAddAfterDivide = operations.AreAllInstancesFoundAfterAllOthers<MathOperation>(new MathOperation[1] { MathOperation.Add }, new MathOperation[1] { MathOperation.Divide });
		bool? isDivideAfterMultiply = operations.AreAllInstancesFoundAfterAllOthers<MathOperation>(new MathOperation[1] { MathOperation.Divide }, new MathOperation[1] { MathOperation.Multiply });
		bool? isMultiplyAfterExponent = operations.AreAllInstancesFoundAfterAllOthers<MathOperation>(new MathOperation[1] { MathOperation.Multiply }, new MathOperation[1] { MathOperation.Exponent });
		return (isSubtractAfterAdd == null || (bool) isSubtractAfterAdd) && (isAddAfterDivide == null || (bool) isAddAfterDivide) && (isDivideAfterMultiply == null || (bool) isDivideAfterMultiply) && (isMultiplyAfterExponent == null || (bool) isMultiplyAfterExponent);
	}
}