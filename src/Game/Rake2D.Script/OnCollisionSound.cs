using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionSound : BehaviorScript
{
	private int volume;

	private float _repeatDelay;

	private float _lastRepeatTime;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		volume = ParamInt("volume", 100);
		_repeatDelay = Param("repeat_delay") / 1000f;
		_lastRepeatTime = 0f;
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (MyObject.Timer - _lastRepeatTime > _repeatDelay)
		{
			_lastRepeatTime = MyObject.Timer;
			string text = ParamRandomString("sound_id");
			if (!text.IsNullOrEmpty())
			{
				MyObject.PlaySound(text, volume);
			}
		}
	}
}
