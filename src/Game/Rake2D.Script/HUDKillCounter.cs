using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HUDKillCounter : ExitStateBehavior
{
	private PartData _wantedPartData;

	private HUDCounter _hudCounter;

	public override void RegisterEvents()
	{
		if (_wantedPartData != null)
		{
			MyLevel.OnObjectKill += OnObjectKill;
			MyObject.OnActivate += OnActivate;
		}
	}

	public void OnActivate()
	{
		if (_hudCounter == null)
		{
			_hudCounter = GameJnG.HeroList[0].Hud.GetOrCreateCounter(ParamString("id"));
			_hudCounter.Offset = ParamVector2("offset");
			_hudCounter.ValueScale = ParamVector2("value_scale", 100f, 100f) / 100f;
			_hudCounter.ValueOffset = ParamVector2("value_offset");
			_hudCounter.ValueColor = ParamColor("value_color", Color.White);
			_hudCounter.Maximum = ParamInt("value_max");
			string text = ParamString("hud_object");
			if (text != null)
			{
				_hudCounter.CreateObject(text);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_wantedPartData = GameCore.GetPartData(ParamString("object_id"));
	}

	private void OnObjectKill(GameObject go)
	{
		if (go.PartData == _wantedPartData && _hudCounter.RaiseValue(1))
		{
			StartExit();
		}
	}
}
