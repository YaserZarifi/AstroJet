using System;
using System.Collections.Generic;

namespace Rake2D.Script;

public class OnStateThrowAndLinkToNearby : ThrowObjectBehavior
{
	protected List<GameObject> _nearestObjectList;

	protected TargetClass _targetClass;

	protected float _radius;

	public override void RegisterEvents()
	{
		OnThrow = (OnGameObjectEventHandler)Delegate.Combine(OnThrow, new OnGameObjectEventHandler(OnThrowLink));
		StartThrow();
	}

	public override void Init()
	{
		base.Init();
		_radius = Param("radius", 500f);
		string[] array = ParamStringArray("target_class");
		if (array != null)
		{
			_targetClass = DataUtils.GetTargetClassByStringArray(array);
		}
		else
		{
			_targetClass = TargetClass.AllEnemy;
		}
	}

	private void OnThrowLink(GameObject throwedObject)
	{
		if (_nearestObjectList == null)
		{
			List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
			gameObjectList.Add(MyObject);
			_nearestObjectList = MyLevel.GetVisibleObjectsInRangeInAllPhysLayers(MyObject.WorldPosition, _targetClass, _radius, gameObjectList, bOlyHittable: true);
		}
		if (_nearestObjectList.Count > 0)
		{
			int random = Utils.GetRandom(0, _nearestObjectList.Count - 1);
			throwedObject.AddLinkedObject(_nearestObjectList[random]);
			_nearestObjectList.RemoveAt(random);
		}
		if (_nearestObjectList.Count <= 0)
		{
			MyObject.OnUpdate -= UpdateLoop;
			MyObject.OnEditorUpdate -= UpdateLoop;
			OnThrow = (OnGameObjectEventHandler)Delegate.Remove(OnThrow, new OnGameObjectEventHandler(OnThrowLink));
		}
	}
}
