using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Rake2D;

public class RigidBody
{
	private PhysClass _physClass;

	private PhysClass _hitTo;

	private Body _body;

	private World _physWorld;

	private GameObject _myObject;

	private HashSet<GameObject> _igoreObjectSet;

	public bool OnCollisionEnabled;

	private Vector2 _localCenter;

	private bool _isSensor;

	private float _mass;

	private float _friction;

	private float _restitution;

	public Body PhysBody => _body;

	public bool IsDestroyed { get; private set; }

	public Vector2 Scale { get; private set; }

	public ContactEdge ContactList => _body.ContactList;

	public PhysClass PhysClass => _physClass;

	internal BodyType BodyType
	{
		get
		{
			if (_body != null)
			{
				return _body.BodyType;
			}
			return BodyType.Static;
		}
		set
		{
			if (_body != null)
			{
				_body.BodyType = value;
			}
		}
	}

	public bool FixedRotation
	{
		get
		{
			return _body.FixedRotation;
		}
		set
		{
			_body.FixedRotation = value;
		}
	}

	public float LinearDamping
	{
		get
		{
			return _body.LinearDamping;
		}
		set
		{
			_body.LinearDamping = value;
		}
	}

	public bool IgnoreGravity
	{
		get
		{
			if (_body != null)
			{
				return _body.IgnoreGravity;
			}
			return true;
		}
		set
		{
			if (_body != null)
			{
				_body.IgnoreGravity = value;
			}
		}
	}

	public float GravityScale
	{
		get
		{
			if (_body != null)
			{
				return _body.GravityScale;
			}
			return 1f;
		}
		set
		{
			if (_body != null)
			{
				_body.GravityScale = value;
			}
		}
	}

	public Vector2 LocalCenter
	{
		get
		{
			return ConvertUnits.ToDisplayUnits(_body.LocalCenter);
		}
		set
		{
			_localCenter = ConvertUnits.ToSimUnits(value);
			_body.LocalCenter = _localCenter * Scale;
		}
	}

	public Vector2 WorldCenter => ConvertUnits.ToDisplayUnits(_body.WorldCenter);

	public bool IsStatic
	{
		get
		{
			if (_body != null)
			{
				return _body.IsStatic;
			}
			return true;
		}
		set
		{
			if (_body != null)
			{
				_body.IsStatic = value;
			}
		}
	}

	public bool IsSensor
	{
		get
		{
			return _isSensor;
		}
		set
		{
			_isSensor = value;
			if (_body != null)
			{
				_body.IsSensor = _isSensor;
			}
		}
	}

	internal Vector2 Velocity
	{
		get
		{
			return _body.LinearVelocity;
		}
		set
		{
			_body.LinearVelocity = value;
		}
	}

	public float AngularVelocity
	{
		get
		{
			return _body.AngularVelocity;
		}
		set
		{
			_body.AngularVelocity = value;
		}
	}

	public float AngularDamping
	{
		get
		{
			return _body.AngularDamping;
		}
		set
		{
			_body.AngularDamping = value;
		}
	}

	internal float RotationSpeed
	{
		get
		{
			return _body.AngularVelocity;
		}
		set
		{
			_body.AngularVelocity = value;
		}
	}

	public bool IsBullet
	{
		get
		{
			if (_body != null)
			{
				return _body.IsBullet;
			}
			return false;
		}
		set
		{
			if (_body != null)
			{
				_body.IsBullet = value;
			}
		}
	}

	public PhysClass HitTo
	{
		get
		{
			return _hitTo;
		}
		set
		{
			_hitTo = value;
			_body.CollidesWith = (Category)_hitTo;
		}
	}

	public float WorldRotation
	{
		get
		{
			return _body.Rotation;
		}
		set
		{
			_body.Rotation = value;
		}
	}

	public bool IsEnabled
	{
		get
		{
			return _body.Enabled;
		}
		set
		{
			_body.Enabled = value;
		}
	}

	public float Mass
	{
		get
		{
			if (_body != null)
			{
				return ConvertUnits.ToDisplayUnits(_body.Mass);
			}
			return 0f;
		}
		set
		{
			if (_body != null)
			{
				_mass = ConvertUnits.ToSimUnits(value);
				_body.Mass = _mass;
			}
		}
	}

	public float Friction
	{
		get
		{
			return _body.Friction;
		}
		set
		{
			_friction = value;
			_body.Friction = _friction;
		}
	}

	public float Restitution
	{
		get
		{
			return _body.Restitution;
		}
		set
		{
			_restitution = value;
			_body.Restitution = _restitution;
		}
	}

	public Vector2 WorldPosition
	{
		get
		{
			return ConvertUnits.ToDisplayUnits(_body.Position);
		}
		set
		{
			Vector2 position = ConvertUnits.ToSimUnits(value);
			_body.Position = position;
		}
	}

	public float Rotation
	{
		get
		{
			return _body.Rotation;
		}
		set
		{
			_body.Rotation = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			return _body.Position;
		}
		set
		{
			_body.Position = value;
		}
	}

	public bool SleepingAllowed
	{
		get
		{
			return _body.SleepingAllowed;
		}
		set
		{
			_body.SleepingAllowed = value;
		}
	}

	public RigidBody(GameObject gameObject)
	{
		_myObject = gameObject;
	}

	public RigidBody(Vector2 position, GameObject gameObject)
		: this(gameObject)
	{
		_physWorld = gameObject.MyLayer.MyLevel.PhysWorld;
		_body = BodyFactory.CreateBody(_physWorld, ConvertUnits.ToSimUnits(position), 0f, _myObject);
		_mass = _body.Mass;
		_friction = 1f;
		_restitution = 0f;
		Scale = Vector2.One;
	}

	public void ResetBody()
	{
		float rotation = 0f;
		Vector2 position = Vector2.Zero;
		if (_body != null)
		{
			rotation = Rotation;
			position = Position;
			RemoveBody();
		}
		_body = BodyFactory.CreateBody(_physWorld, position, 0f, _myObject);
		_body.Rotation = rotation;
	}

	public RigidBody CloneOnto(GameObject go)
	{
		RigidBody rigidBody = null;
		rigidBody = new RigidBody(go);
		rigidBody.OnCollisionEnabled = OnCollisionEnabled;
		if (_body != null)
		{
			rigidBody._body = _body.DeepClone();
			rigidBody._body.UserData = go;
		}
		rigidBody.SetPhysClass(PhysClass);
		rigidBody.Scale = Scale;
		rigidBody.Mass = _mass;
		rigidBody.Friction = _friction;
		rigidBody.Restitution = _restitution;
		rigidBody.ResetDynamics();
		return rigidBody;
	}

	public void SetPhysClass(PhysClass physClass)
	{
		HitTo = GetHitToByPhysClass(physClass);
		_physClass = physClass;
		_body.CollisionCategories = (Category)_physClass;
	}

	public void SetCollisionCategories(PhysClass hitTo, PhysClass category)
	{
		HitTo = hitTo;
		_physClass = category;
		_body.CollisionCategories = (Category)_physClass;
	}

	internal void UpdateHitGroups()
	{
		HitTo = _hitTo;
		SetPhysClass(_physClass);
	}

	public void SetWorldTransform(Vector2 position, float rotation)
	{
		_body.SetTransform(ConvertUnits.ToSimUnits(position), rotation);
	}

	public bool HasJoints()
	{
		return _body.JointList != null;
	}

	public List<Joint> GetJointList()
	{
		List<Joint> jointList = new List<Joint>();
		_physWorld.JointList?.ForEach((Joint joint) =>
		{
			if (joint.JointType == JointType.Revolute && (joint as RevoluteJoint).BodyA == _body)
			{
				jointList.Add(joint);
			}
		});
		return jointList;
	}

	private void UpdateBodyAfterAddingFixture(Fixture fix)
	{
		_body.Mass = _mass;
		_body.Friction = _friction;
		_body.Restitution = _restitution;
		if (OnCollisionEnabled)
		{
			fix.OnCollision = (OnCollisionEventHandler)Delegate.Combine(fix.OnCollision, new OnCollisionEventHandler(OnCollision));
			fix.OnSeparation = (OnSeparationEventHandler)Delegate.Combine(fix.OnSeparation, new OnSeparationEventHandler(OnSeparation));
			fix.BeforeCollision = (BeforeCollisionEventHandler)Delegate.Combine(fix.BeforeCollision, new BeforeCollisionEventHandler(BeforeCollision));
		}
		UpdateHitGroups();
		_body.IsSensor = _isSensor;
	}

	public void AddRectangle(float width, float height, float pivotX = 0f, float pivotY = 0f)
	{
		Fixture fix = FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(Math.Abs(width)), ConvertUnits.ToSimUnits(Math.Abs(height)), 1f, ConvertUnits.ToSimUnits(width / 2f - pivotX, height / 2f - pivotY), _body);
		UpdateBodyAfterAddingFixture(fix);
	}

	public void AddRectangle(Rectangle rect, float pivotX = 0f, float pivotY = 0f)
	{
		AddRectangle(rect.Width, rect.Height, pivotX, pivotY);
	}

	public void AddCircle(float radius, float centerX = 0f, float centerY = 0f)
	{
		Fixture fix = FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(radius), 1f, _body, ConvertUnits.ToSimUnits(centerX, centerY));
		UpdateBodyAfterAddingFixture(fix);
	}

	public void AddEdge(Vector2 startPos, Vector2 endPos)
	{
		Fixture fix = FixtureFactory.AttachEdge(ConvertUnits.ToSimUnits(startPos), ConvertUnits.ToSimUnits(endPos), _body);
		UpdateBodyAfterAddingFixture(fix);
	}

	public void RemoveFixtures()
	{
		for (int num = _body.FixtureList.Count - 1; num >= 0; num--)
		{
			_body.DestroyFixture(_body.FixtureList[num]);
		}
	}

	public void AddPolygon(Vertices vertices)
	{
		if (vertices.Count < 3 || vertices.Count > Settings.MaxPolygonVertices)
		{
			GameCore.LogErr("Wrong vertex count ({0} <> [3 - 8]) for phys body of '{1}'", vertices.Count, _myObject.Id);
			return;
		}
		Fixture fixture = FixtureFactory.AttachPolygon(vertices, 1f, _body);
		int count = ((PolygonShape)fixture.Shape).Vertices.Count;
		if (count != vertices.Count)
		{
			_myObject.LogErr("Polygon vertex reduction ({0} to {1}) - probably is not convex.", vertices.Count, count);
		}
		UpdateBodyAfterAddingFixture(fixture);
	}

	public void AddPolygon(float[] vertexArray, Matrix matrix)
	{
		Vertices vertices = new Vertices();
		for (int i = 0; i < vertexArray.Length; i += 2)
		{
			Vector2 vector = new Vector2(vertexArray[i], vertexArray[i + 1]);
			if (matrix != Matrix.Identity)
			{
				vector = Vector2.Transform(vector, matrix);
			}
			vertices.Add(ConvertUnits.ToSimUnits(vector));
		}
		AddPolygon(vertices);
	}

	public List<Fixture> GetFixtureList()
	{
		if (_body == null)
		{
			return null;
		}
		return _body.FixtureList;
	}

	public Transform GePhysicsTransform()
	{
		if (_body == null)
		{
			return default;
		}
		_body.GetTransform(out var transform);
		return transform;
	}

	public List<Vector2> GetEdgeVertexList()
	{
		if (_body == null)
		{
			return null;
		}
		List<Vector2> vector2List = GameCore.Allocator.GetVector2List();
		Transform T = GePhysicsTransform();
		Vector2 zero = Vector2.Zero;
		Vector2 one = Vector2.One;
		List<Fixture> fixtureList = GetFixtureList();
		for (int i = 0; i < fixtureList.Count; i++)
		{
			if (fixtureList[i].Shape is EdgeShape edgeShape)
			{
				if (i == 0)
				{
					zero = MathUtils.Mul(ref T, edgeShape.Vertex1);
					vector2List.Add(ConvertUnits.ToDisplayUnits(zero));
				}
				one = MathUtils.Mul(ref T, edgeShape.Vertex2);
				vector2List.Add(ConvertUnits.ToDisplayUnits(one));
			}
		}
		return vector2List;
	}

	public void BuildEdgeBody(List<Vector2> vertexList)
	{
		ResetBody();
		Vector2 startPos = Vector2.Zero;
		bool flag = true;
		foreach (Vector2 vertex in vertexList)
		{
			if (!flag)
			{
				AddEdge(startPos, vertex);
			}
			startPos = vertex;
			flag = false;
		}
		Vector2 scale = Scale;
		Scale = Vector2.One;
		ChangeScale(scale);
	}

	public void ChangeScale(Vector2 scaleFactor)
	{
		if (scaleFactor == Vector2.One)
		{
			return;
		}
		Scale *= scaleFactor;
		List<Fixture> list = new List<Fixture>();
		int count = _body.FixtureList.Count;
		for (int i = 0; i < count; i++)
		{
			Fixture fixture = _body.FixtureList[i];
			Shape shape = fixture.Shape.Clone();
			if (fixture.Shape.ShapeType == ShapeType.Polygon)
			{
				((PolygonShape)shape).Vertices.Scale(new Vector2(scaleFactor.X, scaleFactor.Y));
				((PolygonShape)shape).Vertices.ForceCounterClockWise();
			}
			else if (fixture.Shape.ShapeType == ShapeType.Circle)
			{
				((CircleShape)shape).Position *= scaleFactor;
				((CircleShape)shape).Radius *= Math.Abs(scaleFactor.X);
			}
			else
			{
				if (fixture.Shape.ShapeType != ShapeType.Edge)
				{
					continue;
				}
				((EdgeShape)shape).Vertex1 *= scaleFactor;
				((EdgeShape)shape).Vertex2 *= scaleFactor;
				if (((EdgeShape)shape).HasVertex0)
				{
					((EdgeShape)shape).Vertex0 *= scaleFactor;
				}
				if (((EdgeShape)shape).HasVertex3)
				{
					((EdgeShape)shape).Vertex3 *= scaleFactor;
				}
			}
			Fixture fixture2 = _body.CreateFixture(shape);
			UpdateFixture(fixture2, fixture);
			if (OnCollisionEnabled)
			{
				fixture2.OnCollision = (OnCollisionEventHandler)Delegate.Combine(fixture2.OnCollision, new OnCollisionEventHandler(OnCollision));
				fixture2.OnSeparation = (OnSeparationEventHandler)Delegate.Combine(fixture2.OnSeparation, new OnSeparationEventHandler(OnSeparation));
				fixture2.BeforeCollision = (BeforeCollisionEventHandler)Delegate.Combine(fixture2.BeforeCollision, new BeforeCollisionEventHandler(BeforeCollision));
			}
			list.Add(fixture);
		}
		ChngeScaleToJoints(scaleFactor);
		foreach (Fixture item in list)
		{
			item.OnSeparation = (OnSeparationEventHandler)Delegate.Remove(item.OnSeparation, new OnSeparationEventHandler(OnSeparation));
			_body.DestroyFixture(item);
		}
		_body.Mass = _mass;
		_body.Friction = _friction;
		_body.Restitution = _restitution;
		_body.LocalCenter = _localCenter * Scale;
		_body.IsSensor = _isSensor;
	}

	private void ChngeScaleToJoints(Vector2 scaleFactor)
	{
		_physWorld.JointList?.ForEach((Joint joint) =>
		{
			if (joint.JointType == JointType.Revolute)
			{
				RevoluteJoint revoluteJoint = joint as RevoluteJoint;
				if (revoluteJoint.BodyA == _body)
				{
					revoluteJoint.LocalAnchorA *= scaleFactor;
				}
				if (revoluteJoint.BodyB == _body)
				{
					revoluteJoint.LocalAnchorB *= scaleFactor;
				}
			}
		});
	}

	private void UpdateFixture(Fixture newFixture, Fixture oldFixture)
	{
		newFixture.CollisionCategories = oldFixture.CollisionCategories;
		newFixture.CollisionGroup = oldFixture.CollisionGroup;
		newFixture.CollidesWith = oldFixture.CollidesWith;
		for (int num = _physWorld.ContactManager.ContactList.Count - 1; num >= 0; num--)
		{
			if (_physWorld.ContactManager.ContactList[num].FixtureA == oldFixture)
			{
				_physWorld.ContactManager.ContactList[num].FixtureA = newFixture;
			}
			if (_physWorld.ContactManager.ContactList[num].FixtureB == oldFixture)
			{
				_physWorld.ContactManager.ContactList[num].FixtureB = newFixture;
			}
		}
	}

	public void WakeUp(bool resetDynamics)
	{
		_body.Awake = true;
		if (resetDynamics)
		{
			ResetDynamics();
		}
	}

	public void ResetDynamics()
	{
		_body.ResetDynamics();
	}

	public void Sleep()
	{
		_body.Awake = false;
	}

	public void Destroy()
	{
		if (!IsDestroyed)
		{
			IsDestroyed = true;
			RemoveBody();
		}
	}

	public void RemoveBody()
	{
		if (_body != null)
		{
			_physWorld.RemoveBody(_body);
			_body = null;
		}
	}

	internal void ApplyLinearImpulse(Vector2 forceVector)
	{
		_body.ApplyLinearImpulse(ConvertUnits.ToSimUnits(forceVector));
	}

	public void ApplyLinearImpulse(float forceX, float forceY)
	{
		ApplyLinearImpulse(new Vector2(forceX, forceY));
	}

	public void ApplyForce(Vector2 forceVector)
	{
		_body.ApplyForce(ConvertUnits.ToSimUnits(forceVector));
	}

	public void ApplyForce(float forceX, float forceY)
	{
		ApplyForce(new Vector2(forceX, forceY));
	}

	public void ApplyAngularImpulse(float force)
	{
		_body.ApplyAngularImpulse(ConvertUnits.ToSimUnits(force));
	}

	public void IgnoreCollisionWith(GameObject go)
	{
		if (_igoreObjectSet == null)
		{
			_igoreObjectSet = new HashSet<GameObject>();
		}
		_igoreObjectSet.Add(go);
	}

	public void RestoreCollisionWith(GameObject go)
	{
		if (_igoreObjectSet != null && _igoreObjectSet.Contains(go))
		{
			_igoreObjectSet.Remove(go);
		}
	}

	public void RestoreCollisionWithAll()
	{
		if (_igoreObjectSet != null)
		{
			_igoreObjectSet.Clear();
		}
	}

	private bool BeforeCollision(Fixture f1, Fixture f2)
	{
		((GameObject)f1.Body.UserData).DoBeforeCollision((GameObject)f2.Body.UserData);
		return true;
	}

	private bool OnCollision(Fixture f1, Fixture f2, Contact contact)
	{
		GameObject gameObject = (GameObject)f2.Body.UserData;
		if (_igoreObjectSet != null && _igoreObjectSet.Contains(gameObject))
		{
			return false;
		}
		contact.GetWorldManifold(out var normal, out var points);
		if (gameObject.PartData.PhysEvent && (contact.Manifold.PointCount >= 1 || f1.IsSensor || f2.IsSensor))
		{
			GameObject gameObject2 = (GameObject)f1.Body.UserData;
			Vector2 contactPoint;
			if (f1.IsSensor)
			{
				contactPoint = gameObject.WorldPosition;
			}
			else
			{
				contactPoint = ((!f2.IsSensor) ? ConvertUnits.ToDisplayUnits(points[0]) : gameObject2.WorldPosition);
			}
			return gameObject2.DoOnCollision(gameObject, contactPoint, normal);
		}
		return true;
	}

	private void OnSeparation(Fixture f1, Fixture f2)
	{
		((GameObject)f1.Body.UserData).DoOnSeparation((GameObject)f2.Body.UserData);
	}

	public void AddJoint(JointType jointType, RigidBody rigidBody, Vector2 myLocalAnchor, Vector2 otherLocalAnchor)
	{
		switch (jointType)
		{
		case JointType.Revolute:
			JointFactory.CreateRevoluteJoint(_physWorld, _body, rigidBody._body, ConvertUnits.ToSimUnits(myLocalAnchor), ConvertUnits.ToSimUnits(otherLocalAnchor), useWorldCoordinates: true).CollideConnected = false;
			break;
		case JointType.Distance:
			JointFactory.CreateDistanceJoint(_physWorld, _body, rigidBody._body, myLocalAnchor, otherLocalAnchor).CollideConnected = false;
			break;
		case JointType.Weld:
			JointFactory.CreateWeldJoint(_physWorld, _body, rigidBody._body, ConvertUnits.ToSimUnits(myLocalAnchor), ConvertUnits.ToSimUnits(otherLocalAnchor), useWorldCoordinates: true).CollideConnected = false;
			break;
		case JointType.Rope:
			JointFactory.CreateRopeJoint(_physWorld, _body, rigidBody._body, myLocalAnchor, otherLocalAnchor).CollideConnected = false;
			break;
		}
	}

	public static PhysClass GetHitToByPhysClass(PhysClass physClass)
	{
		return physClass switch
		{
			PhysClass.Hero => (PhysClass)309507208, 
			PhysClass.Drone => (PhysClass)309376184, 
			PhysClass.JetPack => (PhysClass)41071800, 
			PhysClass.ShotHero => (PhysClass)45102136, 
			PhysClass.BombHero => (PhysClass)44053528, 
			PhysClass.ElementHero => (PhysClass)13328, 
			PhysClass.HeroSensor => (PhysClass)48, 
			PhysClass.Bonus => (PhysClass)67108865, 
			PhysClass.ShotEnemy => (PhysClass)69207043, 
			PhysClass.MissileEnemy => (PhysClass)69219395, 
			PhysClass.EnemyAir => (PhysClass)352649542, 
			PhysClass.EnemyGround => (PhysClass)354752323, 
			PhysClass.EnemyBack => (PhysClass)320, 
			PhysClass.Ufo => (PhysClass)2110488, 
			PhysClass.Wreck => (PhysClass)2110472, 
			PhysClass.Man => (PhysClass)337712198, 
			PhysClass.Building => (PhysClass)125567811, 
			PhysClass.Ruins => (PhysClass)2102272, 
			PhysClass.Ghost => PhysClass.Ground, 
			PhysClass.Ball => (PhysClass)134222848, 
			PhysClass.Decoration => (PhysClass)103822915, 
			PhysClass.DecorationLarge => (PhysClass)103823171, 
			PhysClass.Ground => (PhysClass)257706987, 
			PhysClass.Platform => (PhysClass)257713003, 
			PhysClass.Water => (PhysClass)106718187, 
			PhysClass.Particle => (PhysClass)35664896, 
			PhysClass.MiscEnemy => (PhysClass)56, 
			PhysClass.MiscHero => (PhysClass)67108867, 
			PhysClass.XenoSensor => (PhysClass)59, 
			_ => PhysClass.None, 
		};
	}
}
