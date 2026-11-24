using Microsoft.Xna.Framework;
using Spine;

namespace Rake2D.Script;

public class ColorBySpineSlot : BehaviorScript
{
	private Slot _slot;

	private bool _applyToChildren;

	private bool _alphaOnly;

	public override void RegisterEvents()
	{
		if (_slot != null)
		{
			MyObject.OnUpdate += Update;
			MyObject.OnEditorUpdate += Update;
		}
	}

	public override void Init()
	{
		_applyToChildren = ParamBool("apply_to_children");
		_alphaOnly = ParamBool("alpha_only");
		string text = ParamString("slot");
		_slot = MyObject.GetCurrentSpineRenderer().FindSlot(text);
		if (_slot == null)
		{
			LogErr("Unable to find spine slot '{0}'", text);
		}
	}

	private void Update(float elapsedTime)
	{
		if (_applyToChildren)
		{
			if (_alphaOnly)
			{
				SetColor(MyObject, new Color(MyObject.Color, _slot.A * (float)(int)MyObject.CurrentState.Color.A / 255f));
			}
			else
			{
				SetColor(MyObject, new Color(_slot.R, _slot.G, _slot.B, _slot.A * (float)(int)MyObject.CurrentState.Color.A / 255f));
			}
		}
		else if (_alphaOnly)
		{
			MyObject.Color = new Color(MyObject.Color, _slot.A * (float)(int)MyObject.CurrentState.Color.A / 255f);
		}
		else
		{
			MyObject.Color = new Color(_slot.R, _slot.G, _slot.B, _slot.A * (float)(int)MyObject.CurrentState.Color.A / 255f);
		}
	}

	private void SetColor(GameObject go, Color color)
	{
		go.SetColor(color);
		if (go.ChildObjectListFront != null)
		{
			int count = go.ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				SetColor(go.ChildObjectListFront[i], color);
			}
		}
		if (go.ChildObjectListBack != null)
		{
			int count2 = go.ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				SetColor(go.ChildObjectListBack[j], color);
			}
		}
	}
}
