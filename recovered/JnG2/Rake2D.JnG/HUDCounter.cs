using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;

namespace Rake2D.JnG;

public class HUDCounter
{
	public string Id;

	public int Value;

	public int Maximum;

	public GameObject GameObject;

	public Vector2 Position;

	public Vector2 Offset;

	public Color ValueColor;

	public Vector2 ValueScale;

	public Vector2 ValueOffset;

	public bool MaximumReached;

	private HUD _hud;

	private Vector2 _hudPosition;

	private BMFont _font;

	private float _hideAlphaFactor;

	public HUDCounter(HUD hud, string id)
	{
		_hud = hud;
		Id = id;
		ValueScale = Vector2.One;
		ValueColor = Color.White;
		_font = GameCore.GetBMFont("shop_font");
	}

	public GameObject CreateObject(string partDataId)
	{
		if (GameObject != null)
		{
			GameObject.Destroy();
		}
		GameObject = _hud.HUDLevel.MainLayer.CreateObjectByPartData(partDataId, Vector2.Zero, Vector2.One, 0f, 0, bCreateRigidBody: false);
		if (GameObject != null)
		{
			GameObject.Init();
			GameObject.Activate();
		}
		return GameObject;
	}

	public HUDCounter Clone()
	{
		return new HUDCounter(_hud, Id)
		{
			Value = Value,
			Maximum = Maximum,
			GameObject = GameObject,
			Position = Position,
			Offset = Offset,
			ValueColor = ValueColor,
			ValueScale = ValueScale,
			ValueOffset = ValueOffset,
			MaximumReached = MaximumReached
		};
	}

	public bool RaiseValue(int delta)
	{
		Value++;
		if (GameObject != null)
		{
			GameObject.ChangePartState("raise");
		}
		if (!MaximumReached && Value >= Maximum)
		{
			MaximumReached = true;
			return true;
		}
		return false;
	}

	public void Update(float elapsedTime)
	{
		_hudPosition = Position + Offset;
		if (GameObject != null)
		{
			GameObject.SetPosition(_hudPosition);
		}
		foreach (Hero hero in GameJnG.HeroList)
		{
			Vector2 virtualScreenPosition = hero.GetControlledObject().VirtualScreenPosition;
			if (virtualScreenPosition.X < 350f && virtualScreenPosition.Y < Position.Y + 60f)
			{
				Utils.ChangeValueToTarget(ref _hideAlphaFactor, 0.3f, 3f * elapsedTime);
			}
			else
			{
				Utils.ChangeValueToTarget(ref _hideAlphaFactor, 1f, 3f * elapsedTime);
			}
		}
		GameObject.Alpha = (byte)(255f * _hideAlphaFactor);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		string text = ((Maximum <= 0) ? Value.ToString() : $"{Value} / {Maximum}");
		_font.Write(spriteBatch, text, _hudPosition.X + ValueOffset.X, _hudPosition.Y + ValueOffset.Y, ValueScale, ValueColor, TextAlign.Left, 0, _hideAlphaFactor);
	}
}
