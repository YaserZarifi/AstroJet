namespace Rake2D.Script;

public class OnKillCountExit : ExitStateBehavior
{
	private int _killCount;

	private int _wantedKillCount;

	private PartData _wantedPartData;

	public override void RegisterEvents()
	{
		if (_wantedPartData != null)
		{
			MyLevel.OnObjectKill += OnObjectKill;
		}
	}

	public override void Init()
	{
		base.Init();
		_wantedPartData = GameCore.GetPartData(ParamString("object_id"));
		_wantedKillCount = ParamInt("count");
	}

	private void OnObjectKill(GameObject go)
	{
		if (go.PartData == _wantedPartData)
		{
			_killCount++;
			if (_killCount >= _wantedKillCount)
			{
				StartExit();
			}
		}
	}
}
