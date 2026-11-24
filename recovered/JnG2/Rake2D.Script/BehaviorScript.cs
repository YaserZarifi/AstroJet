using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class BehaviorScript : IBehaviorScript
{
	public bool IsDone;

	protected bool _doInEditor = true;

	public bool DoInEditor
	{
		get
		{
			if (!_doInEditor)
			{
				return !GameCore.IsEditorEnabled;
			}
			return true;
		}
	}

	public string Id
	{
		get
		{
			if (BehaviorData != null && BehaviorData.Id != null)
			{
				return BehaviorData.Id;
			}
			return ClassName;
		}
	}

	public string ClassName
	{
		get
		{
			if (BehaviorData != null)
			{
				return BehaviorData.TypeName;
			}
			return null;
		}
	}

	public virtual GameObject MyObject { get; private set; }

	public Layer MyLayer => MyObject.MyLayer;

	public GameLevel MyLevel => MyObject.MyLevel;

	public BehaviorData BehaviorData { get; internal set; }

	public bool IsHeroInLevelAutopilotMode
	{
		get
		{
			if (MyObject.MyHero != null)
			{
				if (MyObject.MyHero.AutopilotMode != HeroAutopilotMode.LevelIncomming && MyObject.MyHero.AutopilotMode != HeroAutopilotMode.LevelFlyOut)
				{
					return MyObject.MyHero.AutopilotMode == HeroAutopilotMode.BlackHole;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsForState(PartState partState)
	{
		if (BehaviorData == null)
		{
			return false;
		}
		if (!IsDone)
		{
			return BehaviorData.IsForState(partState);
		}
		return false;
	}

	public virtual void Init()
	{
		_doInEditor = ParamBool("in_editor", defaultValue: true);
	}

	public abstract void RegisterEvents();

	public virtual void DoEvent(BehaviorEvent pathEvent, BehaviorTimeLine sourceTimeLine)
	{
	}

	public void Log(string message, params object[] args)
	{
		GameCore.Log(message, args);
	}

	public void LogErr(string message, params object[] args)
	{
		GameCore.LogErr($"{GetType().Name} for object '{MyObject?.Id}' at pos {MyObject.WorldPosition.X}, {MyObject.WorldPosition.Y} in layer '{MyLayer.Id}': {message}", args);
	}

	public static BehaviorScript CreateFromBehaviorData(GameObject gameObject, BehaviorData data)
	{
		if (data.MyType == null)
		{
			GameCore.LogErr($"Unable instantiate script class '{data.TypeName}' - script type is null.");
			return null;
		}
		BehaviorScript behaviorScript = null;
		try
		{
			behaviorScript = (BehaviorScript)Activator.CreateInstance(data.MyType);
			if (behaviorScript == null)
			{
				GameCore.LogErr($"Unable instantiate behavior script class '{data.TypeName}'");
			}
			else
			{
				behaviorScript.MyObject = gameObject;
				behaviorScript.BehaviorData = data;
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Insntantiate behavior '{0}' failed: {1}", data.Id, ex);
			return null;
		}
		return behaviorScript;
	}

	public static BehaviorScript CreateFromType(GameObject gameObject, Type t)
	{
		if (t != null && t == typeof(BehaviorScript))
		{
			return (BehaviorScript)Activator.CreateInstance(t);
		}
		return null;
	}

	public bool HasParam(string name)
	{
		return BehaviorData.HasParam(name);
	}

	public float[] ParamArray(string name)
	{
		if (BehaviorData != null)
		{
			return BehaviorData.GetFloats(name);
		}
		return null;
	}

	public float[] ParamArrayMultiply(string name, float multiplicand)
	{
		if (BehaviorData != null)
		{
			return BehaviorData.GetMultipliedFloats(name, multiplicand);
		}
		return null;
	}

	public string[] ParamStringArray(string name)
	{
		if (BehaviorData != null)
		{
			return BehaviorData.GetStrings(name);
		}
		return null;
	}

	public int[] ParamIntArray(string name)
	{
		if (BehaviorData != null)
		{
			return BehaviorData.GetInts(name);
		}
		return null;
	}

	public IntRange ParamIntRange(string name, int defaultFromTo = 0)
	{
		return ParamIntRange(name, defaultFromTo, defaultFromTo);
	}

	public IntRange ParamIntRange(string name, int defaultFrom, int defaultTo)
	{
		IntRange result = default;
		int[] array = ParamIntArray(name);
		if (array != null && array.Length != 0)
		{
			result.From = array[0];
			if (array.Length > 1)
			{
				result.To = array[1];
			}
			else
			{
				result.To = result.From;
			}
		}
		else
		{
			result.From = defaultFrom;
			result.To = defaultTo;
		}
		return result;
	}

	public FloatRange ParamRange(string name, float defaultFromTo = 0f)
	{
		return ParamRange(name, defaultFromTo, defaultFromTo);
	}

	public FloatRange ParamRange(string name, float defaultFrom, float defaultTo)
	{
		FloatRange result = default;
		float[] array = ParamArray(name);
		if (array != null && array.Length != 0)
		{
			result.From = array[0];
			if (array.Length > 1)
			{
				result.To = array[1];
			}
			else
			{
				result.To = result.From;
			}
		}
		else
		{
			result.From = defaultFrom;
			result.To = defaultTo;
		}
		return result;
	}

	public virtual int ParamInt(string name, int defaultValue = 0)
	{
		int[] array = ParamIntArray(name);
		if (array != null && array.Length != 0)
		{
			return array[0];
		}
		return defaultValue;
	}

	public bool ParamBool(string name, bool defaultValue = false)
	{
		int[] array = ParamIntArray(name);
		if (array != null && array.Length != 0)
		{
			return array[0] != 0;
		}
		return defaultValue;
	}

	public float ParamRandom(string name, float defaultValue)
	{
		return Utils.GetRandomItem(ParamArray(name), defaultValue);
	}

	public float ParamRandomRange(string name, float defaultValue)
	{
		return Utils.GetRandom(ParamRange(name, defaultValue, defaultValue));
	}

	public float ParamRandomRange(string name, FloatRange defaultValue)
	{
		return Utils.GetRandom(ParamRange(name, defaultValue.From, defaultValue.To));
	}

	public int ParamRandomInt(string name, int defaultValue = 0)
	{
		return (int)Utils.GetRandomItem(ParamArray(name), defaultValue);
	}

	public int ParamRandomIntRange(string name, int defaultFrom = 0, int defaultTo = 0)
	{
		return Utils.GetRandom(ParamIntRange(name, defaultFrom, defaultTo));
	}

	public string ParamRandomString(string name, string defaultValue = "")
	{
		return Utils.GetRandomItem(ParamStringArray(name), defaultValue);
	}

	public Vector2 ParamRandomVector2Range(string name)
	{
		return ParamVector2Range(name).GetRandom();
	}

	public Vector2Range ParamVector2Range(string name)
	{
		return ParamVector2Range(name, 0f, 0f, 0f, 0f);
	}

	public Vector2Range ParamVector2Range(string name, float defaultX, float defaultY)
	{
		return ParamVector2Range(name, defaultX, defaultY, defaultX, defaultY);
	}

	public Vector2Range ParamVector2Range(string name, float defaultFromX, float defaultFromY, float defaultToX, float defaultToY)
	{
		float[] array = ParamArray(name);
		if (array != null)
		{
			switch (array.Length)
			{
			case 1:
				return new Vector2Range(array[0], array[0], array[0], array[0]);
			case 2:
				return new Vector2Range(array[0], array[1], array[0], array[1]);
			case 3:
				return new Vector2Range(array[0], array[1], array[2], array[2]);
			case 4:
				return new Vector2Range(array[0], array[1], array[2], array[3]);
			}
		}
		return new Vector2Range(defaultFromX, defaultFromY, defaultToX, defaultToY);
	}

	public virtual float Param(string name, float defaultValue = 0f)
	{
		float[] array = ParamArray(name);
		if (array != null && array.Length != 0)
		{
			return array[0];
		}
		return defaultValue;
	}

	public string ParamString(string name, string defaultValue = null)
	{
		string[] array = ParamStringArray(name);
		if (array != null && array.Length != 0)
		{
			return array[0];
		}
		return defaultValue;
	}

	public Color ParamColor(string name)
	{
		return ParamColor(name, Color.White);
	}

	public Color ParamColor(string name, Color defaultValue)
	{
		float[] array = ParamArray(name);
		if (array != null)
		{
			return array.Length switch
			{
				1 => new Color((int)array[0], (int)array[0], (int)array[0], 255), 
				2 => new Color((int)array[0], (int)array[0], (int)array[0], (int)array[1]), 
				3 => new Color((int)array[0], (int)array[1], (int)array[2]), 
				_ => new Color((int)array[0], (int)array[1], (int)array[2], (int)array[3]), 
			};
		}
		return defaultValue;
	}

	public Vector2 ParamVector2(string name, float defaultX = 0f, float defaultY = 0f)
	{
		float[] array = ParamArray(name);
		if (array != null)
		{
			return Utils.GetVector2(array);
		}
		return new Vector2(defaultX, defaultY);
	}

	public Vector3 ParamVector3(string name, float defaultX = 0f, float defaultY = 0f, float defaultZ = 0f)
	{
		float[] array = ParamArray(name);
		if (array != null)
		{
			return Utils.GetVector3(array);
		}
		return new Vector3(defaultX, defaultY, defaultZ);
	}
}
