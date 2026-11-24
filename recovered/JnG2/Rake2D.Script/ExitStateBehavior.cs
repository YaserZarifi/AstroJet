using System.Collections.Generic;

namespace Rake2D.Script;

public abstract class ExitStateBehavior : BehaviorScript
{
	protected bool _bExitStateToAll;

	protected string[] _exitStateArray;

	protected FloatRange _exitPauseRange;

	protected List<GameObject> _exitPartListAll;

	protected List<GameObject> _exitPartListRandom;

	protected float _exitPauseTimer;

	protected bool _destroyOnExit;

	protected bool _bIgnoreKilled;

	protected int _rootMode;

	public static List<GameObject> GetExitPartList(string[] idArray, GameObject myObject, out bool bExistStateToAll, int rootMode)
	{
		List<GameObject> list = null;
		bExistStateToAll = false;
		if (!idArray.IsNullOrEmpty())
		{
			List<string> list2 = new List<string>(idArray);
			if (list2.Contains("all"))
			{
				bExistStateToAll = true;
			}
			else
			{
				list = GameCore.Allocator.GetGameObjectList();
				foreach (string item in list2)
				{
					GameObject gameObject;
					switch (item)
					{
					case "main":
						gameObject = myObject.GetMainPart();
						break;
					case "parent":
						gameObject = myObject.ParentObject;
						break;
					case "this":
						gameObject = myObject;
						break;
					case "spine":
						gameObject = myObject.GetParentSpineRendererPart();
						break;
					default:
					{
						if (rootMode == -1)
						{
							gameObject = myObject.GetChildByIdFlat(item);
							break;
						}
						GameObject gameObject2;
						if (rootMode == 1)
						{
							gameObject2 = myObject.GetMainPart();
						}
						else
						{
							gameObject2 = ((rootMode == 2 && myObject.ParentObject != null) ? myObject.ParentObject : myObject);
						}
						gameObject = gameObject2.GetChildByIdDeep(item);
						if (gameObject == null)
						{
							continue;
						}
						break;
					}
					}
					if (!list.Contains(gameObject))
					{
						list.Add(gameObject);
						continue;
					}
					GameCore.LogErr("exit part list already contains part '{0}'", gameObject.Id);
				}
			}
		}
		return list;
	}

	public override void Init()
	{
		base.Init();
		_destroyOnExit = ParamBool("exit_destroy");
		_exitPauseRange = ParamRange("exit_delay", 0f, 0f) / 1000f;
		_exitStateArray = ParamStringArray("exit_state");
		_rootMode = ParamInt("root_mode", 1);
		_bIgnoreKilled = ParamBool("ignore_killed", defaultValue: true);
		_exitPartListAll = GetExitPartList(ParamStringArray("exit_all"), MyObject, out _bExitStateToAll, _rootMode);
		if (!_bExitStateToAll)
		{
			_exitPartListRandom = GetExitPartList(ParamStringArray("exit_rand"), MyObject, out var _, _rootMode);
		}
	}

	private void OnUpdateEnd(float elapsedTime)
	{
		MyObject.OnUpdateEnd -= OnUpdateEnd;
		DoExit();
	}

	private void UpdateExitPause(float elapsedTime)
	{
		_exitPauseTimer -= elapsedTime;
		if (_exitPauseTimer <= 0f)
		{
			MyObject.OnUpdate -= UpdateExitPause;
			MyObject.OnEditorUpdate -= UpdateExitPause;
			MyObject.OnUpdateEnd += OnUpdateEnd;
		}
	}

	protected void DoExit()
	{
		if (_exitStateArray != null)
		{
			string text = Utils.GetRandomItem(_exitStateArray);
			if (Utils.Equals(text, "current"))
			{
				text = MyObject.CurrentState.Id;
			}
			if (_bExitStateToAll)
			{
				if (_rootMode == 0)
				{
					MyObject.ChangePartStateToAll(text);
				}
				else if (_rootMode == 1 && MyObject.ParentObject != null)
				{
					MyObject.ParentObject.ChangePartStateToAll(text);
				}
				else
				{
					MyObject.GetMainPart().ChangePartStateToAll(text);
				}
			}
			else
			{
				if (_exitPartListAll != null)
				{
					for (int num = _exitPartListAll.Count - 1; num >= 0; num--)
					{
						GameObject gameObject = _exitPartListAll[num];
						if (gameObject.IsDestroyed)
						{
							_exitPartListAll.RemoveAt(num);
						}
						else if (_bIgnoreKilled || gameObject.IsAlive)
						{
							gameObject.ChangePartState(text);
						}
					}
				}
				if (_exitPartListRandom != null)
				{
					List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
					for (int num2 = _exitPartListRandom.Count - 1; num2 >= 0; num2--)
					{
						GameObject gameObject2 = _exitPartListRandom[num2];
						if (gameObject2.IsDestroyed)
						{
							_exitPartListRandom.RemoveAt(num2);
						}
						else if (gameObject2.IsEnabled && (_bIgnoreKilled || gameObject2.IsAlive))
						{
							gameObjectList.Add(gameObject2);
						}
					}
					if (gameObjectList.Count > 0)
					{
						Utils.GetRandomListItem(gameObjectList, null).ChangePartState(text);
					}
					GameCore.Allocator.DeleteAliasingGameObjectList(gameObjectList);
				}
				if (_exitPartListRandom == null && _exitPartListAll == null)
				{
					MyObject.ChangePartState(text);
				}
			}
		}
		if (_destroyOnExit)
		{
			MyObject.Destroy();
		}
	}

	protected void StartExit()
	{
		_exitPauseTimer = _exitPauseRange.GetRandom();
		if (_exitPauseTimer <= 0f)
		{
			DoExit();
			return;
		}
		MyObject.OnUpdate += UpdateExitPause;
		MyObject.OnEditorUpdate += UpdateExitPause;
	}
}
