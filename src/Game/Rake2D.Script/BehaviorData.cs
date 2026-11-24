using System;
using System.Collections.Generic;
using Rake2D.JnG;

namespace Rake2D.Script;

public class BehaviorData : ICloneable
{
	internal struct MultipliedFloats
	{
		internal float _factor;

		internal float[] _values;

		internal MultipliedFloats(float factor, float[] values)
		{
			_factor = factor;
			_values = values;
		}
	}

	internal class ParamValue
	{
		internal string[] _strings;

		internal float[] _floats;

		internal List<MultipliedFloats> _multipliedFloats;

		internal int[] _ints;

		internal HeroItemDataParam _itemData;

		internal ParamValue()
		{
			_strings = null;
			_floats = null;
			_multipliedFloats = null;
			_ints = null;
			_itemData = null;
		}

		internal ParamValue(string[] strings)
		{
			_strings = strings;
			_floats = null;
			_multipliedFloats = null;
			_ints = null;
			_itemData = null;
		}

		internal ParamValue(float[] floats)
		{
			_strings = null;
			_floats = floats;
			_multipliedFloats = null;
			_ints = null;
			_itemData = null;
		}

		internal ParamValue(HeroItemDataParam itemData)
		{
			_strings = null;
			_floats = null;
			_multipliedFloats = null;
			_ints = null;
			_itemData = itemData;
		}

		internal float[] MultiplyFloats(float factor)
		{
			if (factor == 1f)
			{
				return _floats;
			}
			if (_multipliedFloats != null)
			{
				int count = _multipliedFloats.Count;
				for (int i = 0; i < count; i++)
				{
					if (_multipliedFloats[i]._factor == factor)
					{
						return _multipliedFloats[i]._values;
					}
				}
			}
			float[] array = new float[_floats.Length];
			for (int j = 0; j < _floats.Length; j++)
			{
				array[j] = _floats[j] * factor;
			}
			if (_multipliedFloats == null)
			{
				_multipliedFloats = new List<MultipliedFloats>();
			}
			_multipliedFloats.Add(new MultipliedFloats(factor, array));
			return array;
		}

		internal int[] GetInts()
		{
			if (_ints != null)
			{
				return _ints;
			}
			if (_floats == null)
			{
				return null;
			}
			_ints = new int[_floats.Length];
			for (int i = 0; i < _floats.Length; i++)
			{
				_ints[i] = (int)_floats[i];
			}
			return _ints;
		}
	}

	public string Id;

	public string TypeName;

	public Type MyType;

	public List<string> PartStateIdList;

	private Dictionary<string, ParamValue> _parameters;

	internal Dictionary<string, ParamValue> ParametersInternal => _parameters;

	public bool IsForState(PartState partState)
	{
		if (PartStateIdList != null)
		{
			return PartStateIdList.Contains(partState.Id);
		}
		return true;
	}

	public bool HasParam(string key)
	{
		if (_parameters != null)
		{
			return _parameters.ContainsKey(key.ToLowerLazy());
		}
		return false;
	}

	public string[] GetStrings(string key)
	{
		if (_parameters == null || !_parameters.TryGetValue(key.ToLowerLazy(), out var value) || value._strings == null)
		{
			return null;
		}
		return value._strings;
	}

	public float[] GetFloats(string key)
	{
		if (_parameters == null || !_parameters.TryGetValue(key.ToLowerLazy(), out var value) || value._floats == null)
		{
			return null;
		}
		return value._floats;
	}

	public float[] GetMultipliedFloats(string key, float factor)
	{
		if (_parameters == null || !_parameters.TryGetValue(key.ToLowerLazy(), out var value) || value._floats == null)
		{
			return null;
		}
		return value.MultiplyFloats(factor);
	}

	public int[] GetInts(string key)
	{
		if (_parameters == null || !_parameters.TryGetValue(key.ToLowerLazy(), out var value) || value._floats == null)
		{
			return null;
		}
		return value.GetInts();
	}

	public float GetFloatOrItemData(string key, int itemLevel, float defaultValue = 0f)
	{
		if (_parameters == null || !_parameters.TryGetValue(key.ToLowerLazy(), out var value))
		{
			return defaultValue;
		}
		if (value._floats != null && value._floats.Length != 0)
		{
			return value._floats[0];
		}
		if (value._itemData != null)
		{
			return value._itemData.GetValue(itemLevel);
		}
		return defaultValue;
	}

	public void SetParam(string key, string[] strings)
	{
		if (_parameters == null)
		{
			_parameters = new Dictionary<string, ParamValue>();
		}
		_parameters[key.ToLowerLazy()] = new ParamValue(strings);
	}

	public void SetParam(string key, float[] floats)
	{
		if (_parameters == null)
		{
			_parameters = new Dictionary<string, ParamValue>();
		}
		_parameters[key.ToLowerLazy()] = new ParamValue(floats);
	}

	public void SetParam(string key, HeroItemDataParam itemData)
	{
		if (_parameters == null)
		{
			_parameters = new Dictionary<string, ParamValue>();
		}
		_parameters[key.ToLowerLazy()] = new ParamValue(itemData);
	}

	public void AddParametersFromItemBehaviorData(BehaviorData behData, int itemLevel)
	{
		if (behData == null || behData._parameters == null)
		{
			return;
		}
		if (_parameters == null)
		{
			_parameters = new Dictionary<string, ParamValue>();
		}
		foreach (KeyValuePair<string, ParamValue> parameter in behData._parameters)
		{
			ParamValue value;
			if (parameter.Value._strings != null)
			{
				value = new ParamValue((string[])parameter.Value._strings.Clone());
			}
			else if (parameter.Value._floats != null)
			{
				value = new ParamValue((float[])parameter.Value._floats.Clone());
			}
			else if (parameter.Value._itemData != null)
			{
				parameter.Value._itemData.GetValue(itemLevel);
				value = new ParamValue(new float[1] { parameter.Value._itemData.GetValue(itemLevel) });
			}
			else
			{
				value = new ParamValue();
			}
			_parameters[parameter.Key] = value;
		}
	}

	public Type SetTypeName(string typeName)
	{
		TypeName = typeName;
		if (Id == null)
		{
			Id = TypeName;
		}
		MyType = Type.GetType($"Rake2D.Script.{TypeName}");
		return MyType;
	}

	public void AddPartStates(List<PartState> stateList)
	{
		if (PartStateIdList == null)
		{
			PartStateIdList = new List<string>();
		}
		foreach (PartState state in stateList)
		{
			PartStateIdList.Add(state.Id);
		}
	}

	public object Clone()
	{
		BehaviorData behaviorData = new BehaviorData();
		behaviorData.Id = Id;
		behaviorData.TypeName = TypeName;
		behaviorData.MyType = MyType;
		if (PartStateIdList != null)
		{
			behaviorData.PartStateIdList = new List<string>();
			behaviorData.PartStateIdList.AddRange(PartStateIdList);
		}
		if (_parameters != null)
		{
			behaviorData._parameters = new Dictionary<string, ParamValue>();
			foreach (KeyValuePair<string, ParamValue> parameter in _parameters)
			{
				behaviorData._parameters.Add(parameter.Key, parameter.Value);
			}
		}
		return behaviorData;
	}
}
