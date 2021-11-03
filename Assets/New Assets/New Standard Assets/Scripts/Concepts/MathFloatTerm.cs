public class MathFloatTerm
{
	public float number;
	public Variable<float?>[] variables = new Variable<float?>[0];

	public MathFloatTerm (float number, Variable<float?>[] variables)
	{
		this.number = number;
		this.variables = variables;
	}

	public float? GetValue ()
	{
		float? output = number;
		for (int i = 0; i < variables.Length; i ++)
		{
			Variable<float?> variable = variables[i];
			if (variable != null)
				output *= (float) variable.value;
			else
				return null;
		}
		return output;
	}
}