namespace Rake2D.Font.BMFont;

public struct Kerning
{
	public int Amount { get; set; }

	public char FirstCharacter { get; set; }

	public char SecondCharacter { get; set; }

	public Kerning(char firstCharacter, char secondCharacter, int amount)
	{
		this = default;
		FirstCharacter = firstCharacter;
		SecondCharacter = secondCharacter;
		Amount = amount;
	}

	public override string ToString()
	{
		return $"{FirstCharacter} to {SecondCharacter} = {Amount}";
	}

	public override bool Equals(object obj)
	{
		if (obj is Kerning kerning)
		{
			if (kerning.FirstCharacter == FirstCharacter)
			{
				return kerning.SecondCharacter == SecondCharacter;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		try
		{
			return FirstCharacter * 9999 + SecondCharacter.GetHashCode();
		}
		catch
		{
			return base.GetHashCode();
		}
	}
}
