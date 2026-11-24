using System.Linq;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Rake2D.Editor;

namespace Rake2D.Script;

public class FuhrerPhys : BehaviorScript
{
	private Transformation _oldTrans;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdateBegin += OnEditorUpdateBegin;
		MyObject.OnEditorUpdateEnd += OnEditorUpdateEnd;
	}

	public override void Init()
	{
		base.Init();
		if (GameCore.IsEditorEnabled || MyObject.LinkedObjectList == null)
		{
			return;
		}
		GameObject[] array = MyObject.LinkedObjectList.OrderBy((GameObject o) => o.ZOrder).ToArray();
		MyObject.RigidBody.IsStatic = false;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject.RigidBody != null)
			{
				gameObject.RigidBody.IsStatic = false;
				gameObject.RigidBody.FixedRotation = false;
				gameObject.DestroyOutOfView = OutOfFrame.None;
				MyObject.RigidBody.AddJoint(JointType.Weld, gameObject.RigidBody, gameObject.WorldPosition, gameObject.WorldPosition);
			}
			else
			{
				gameObject.MyLayer.GameObjectList.Remove(gameObject);
				gameObject.MyLayer.VisibleGameObjectList.Remove(gameObject);
				Vector2 vector = gameObject.Position - MyObject.Position;
				MyObject.AddChildRelative(gameObject, vector.X, vector.Y, gameObject.ZOrder - MyObject.ZOrder);
			}
		}
	}

	public void OnEditorUpdateBegin(Rake2D.Editor.Editor editor, float elapsedTime)
	{
		_oldTrans = new Transformation(MyObject.WorldTransformation);
		_ = editor.CurrentObject;
		_ = MyObject;
	}

	public void OnEditorUpdateEnd(Rake2D.Editor.Editor editor, float elapsedTime)
	{
		if (_oldTrans == null || editor.CurrentObject != MyObject || !(_oldTrans.Position != MyObject.Position) || !GameInput.ControlDown)
		{
			return;
		}
		foreach (GameObject linkedObject in MyObject.LinkedObjectList)
		{
			linkedObject.SetPosition(linkedObject.Position + MyObject.Position - _oldTrans.Position);
		}
	}

	public void OnUpdate(float elapsedTime)
	{
		Vector2 vector = MyObject.MyPath.GetPosition(MyObject.PathTime) + MyObject.PathOscilation;
		Vector2 velocityVector = Utils.GetVelocityVector2(20000f, Utils.GetRotationTo(MyObject.WorldPosition, vector));
		MyObject.ApplyForce(velocityVector * 0.02f * (MyObject.WorldPosition - vector).Length());
	}
}
