namespace Rake2D.Script;

public class OnHitSendLink : BehaviorScript
{
	private string _linkOut;

	public override void RegisterEvents()
	{
		MyObject.OnHit += OnHit;
	}

	public override void Init()
	{
		_linkOut = ParamString("link_out", "hit");
	}

	private void OnHit(float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (damage > 0f && MyObject.LinkedObjectList != null)
		{
			MyObject.SendLinkToAll(hittingObject, _linkOut, (int)damage);
		}
	}
}
