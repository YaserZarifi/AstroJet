namespace Rake2D.Script;

public class OnHeroButtonExit : ExitStateBehavior
{
	private GameInput.KeyTypeEnum _keyType;

	private GameObject _mainPart;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_keyType = GameInput.KeyTypeEnum.FireGuns;
		_mainPart = MyObject.GetMainPart();
	}

	private void OnUpdate(float elepsedTime)
	{
		if (_mainPart.MyHero != null && _mainPart.MyHero.KeyDown(_keyType))
		{
			StartExit();
		}
	}
}
