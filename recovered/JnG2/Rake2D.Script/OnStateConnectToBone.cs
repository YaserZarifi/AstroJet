using Spine;

namespace Rake2D.Script;

public class OnStateConnectToBone : BehaviorScript
{
	private string _boneName;

	public override void RegisterEvents()
	{
		BoneData boneData = PartData.FindBoneDataByName(_boneName, MyObject.GetMainPart().PartData);
		if (boneData != null)
		{
			MyObject.ConnectBySpineData(boneData);
			return;
		}
		LogErr("Spine bone '{0}' not found in main part '{1}'", _boneName, MyObject.GetMainPart().Id);
	}

	public override void Init()
	{
		base.Init();
		_boneName = ParamString("bone", "undefined");
	}
}
