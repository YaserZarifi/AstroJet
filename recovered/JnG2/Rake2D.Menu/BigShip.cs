using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class BigShip : ShopMenuSection
{
	public ShopMenuState State;

	public const float HIDDEN_Y = -400f;

	public const float VISIBLE_Y = 260f;

	public Hero MyHero;

	public GameObject MyObject;

	public Vector2 TargetPosition;

	private float _beginDelayTimer;

	private ShopMenu _shopMenu;

	public float ShowHidePercentage
	{
		get
		{
			if (State == ShopMenuState.Hidden)
			{
				return 0f;
			}
			if (State == ShopMenuState.Hiding)
			{
				return Math.Abs((TargetPosition.Y - MyObject.Position.Y) / -660f);
			}
			if (State == ShopMenuState.Showing || State == ShopMenuState.ShowingFromLevel)
			{
				return Math.Abs((MyObject.Position.Y - -400f) / -660f);
			}
			return 1f;
		}
	}

	public BigShip(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
	}

	public void Init(Hero hero)
	{
		MyHero = hero;
		Transformation transformation = null;
		if (MyObject != null)
		{
			transformation = MyObject.WorldTransformation;
			MyObject.Destroy();
			MyObject = null;
		}
		PartData partData = GameCore.GetPartData("big_hero");
		if (MyHero.MyShip.PartMap != null)
		{
			partData = MyHero.MyShip.PartMap.CreateModifiedData(partData, "big_hero", "big_");
		}
		MyObject = _shopMenu.GUILevel.MainLayer.CreateObjectByPartData(partData, Vector2.Zero, Vector2.One, 0f, 0, bCreateRigidBody: false);
		MyObject.ActiveMode = ActiveModeEnum.AlwaysActive;
		MyObject.Activate();
		if (MyHero.MyShip.PartMap != null)
		{
			MyHero.MyShip.PartMap.ApplyOffsetXToAllParts(MyObject);
		}
		if (transformation != null)
		{
			MyObject.SetTransformation(transformation);
		}
		else
		{
			MyObject.Position = new Vector2(770f, -400f);
		}
		GameObject childByIdFlat = MyObject.GetChildByIdFlat("Body");
		if (childByIdFlat != null)
		{
			GameObject childByIdFlat2 = childByIdFlat.GetChildByIdFlat("Nose");
			if (childByIdFlat2 != null)
			{
				float num = childByIdFlat2.Position.X + (float)childByIdFlat2.FrameRectangle.Width - childByIdFlat2.FramePivot.X;
				childByIdFlat.Position.X -= (childByIdFlat.FramePivot.X + num) / 2f - childByIdFlat.FramePivot.X;
			}
		}
	}

	public void Show(float beginDelay)
	{
		State = ShopMenuState.Showing;
		MyObject.Position = new Vector2(770f, -400f);
		TargetPosition = new Vector2(MyObject.Position.X, 260f);
		_beginDelayTimer = beginDelay;
	}

	public void Hide()
	{
		State = ShopMenuState.Hiding;
		TargetPosition = new Vector2(770f, -400f);
	}

	public virtual void UpdateMove(float elapsedTime)
	{
		if (State == ShopMenuState.Hiding)
		{
			if (Utils.ChangeValueToTarget(ref MyObject.Position, TargetPosition, 3000f * elapsedTime))
			{
				State = ShopMenuState.Hidden;
			}
		}
		else if (State == ShopMenuState.Showing && Utils.ChangeValueToTarget(ref _beginDelayTimer, 0f, elapsedTime))
		{
			float num = Utils.Clamp((TargetPosition.Y - MyObject.Position.Y) * 10f, 10f, 2000f);
			if (Utils.ChangeValueToTarget(ref MyObject.Position, TargetPosition, num * elapsedTime))
			{
				State = ShopMenuState.Visible;
			}
		}
	}

	public void Update(float elapsedTime)
	{
		UpdateMove(elapsedTime);
		MyObject.Update(elapsedTime);
	}

	public void Draw()
	{
		GameCore.DrawBegin(GameCore.ViewPortMatrix);
		MyObject.Draw(GameCore.SpriteBatch);
		GameCore.DrawEnd();
	}
}
