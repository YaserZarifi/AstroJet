using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class OnHeroCollisionSendLink : BehaviorScript
{
	private string _linkOut;

	private bool _player1Done;

	private bool _player2Done;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_linkOut = ParamString("link_out");
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.IsHero)
		{
			if (collisionObject.MyHero.PlayerNumber == 1)
			{
				_player2Done = true;
			}
			else
			{
				_player1Done = true;
			}
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if (GameJnG.PlayerCount < 2)
		{
			_player2Done = true;
		}
		foreach (Hero hero in GameJnG.HeroList)
		{
			if (hero.IsDestroyed)
			{
				if (hero.PlayerNumber == 0)
				{
					_player1Done = true;
				}
				else
				{
					_player2Done = true;
				}
			}
		}
		if (_player1Done && _player2Done)
		{
			MyObject.SendLinkToAll(MyObject, _linkOut, 0);
			MyObject.OnCollision -= OnCollision;
			MyObject.OnUpdate -= OnUpdate;
			if (ParamBool("destroy"))
			{
				MyObject.Destroy();
			}
		}
	}
}
