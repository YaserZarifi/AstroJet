using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Rake2D;

namespace FarseerPhysics.DebugView;

public class DebugViewXNA : DebugViewBase, IDisposable
{
	private struct ContactPoint
	{
		public Vector2 Normal;

		public Vector2 Position;

		public PointState State;
	}

	private struct StringData(Vector2 position, string text, Color color)
	{
		public Color Color = color;

		public string Text = text;

		public Vector2 Position = position;
	}

	private PrimitiveBatch _primitiveBatch;

	private SpriteBatch _batch;

	private GraphicsDevice _device;

	private Vector2[] _tempVertices = new Vector2[Settings.MaxPolygonVertices];

	private List<StringData> _stringData;

	private Matrix _localProjection;

	private Matrix _localView;

	public Color DefaultShapeColor = new Color(0.9f, 0.7f, 0.7f);

	public Color InactiveShapeColor = new Color(0.5f, 0.5f, 0.3f);

	public Color KinematicShapeColor = new Color(0.5f, 0.5f, 0.9f);

	public Color KinematicShapeImmortalColor = new Color(255, 180, 60);

	public Color SleepingShapeColor = new Color(0.6f, 0.6f, 0.6f);

	public Color StaticShapeColor = new Color(0.5f, 0.9f, 0.5f);

	public Color StaticShapeImmortalColor = new Color(255, 150, 0);

	public Color SensorShapeColor = new Color(0.2f, 0.8f, 1f);

	public Color TextColor = Color.White;

	private int _pointCount;

	private const int MaxContactPoints = 2048;

	private ContactPoint[] _points = new ContactPoint[2048];

	public Vector2 DebugPanelPosition = new Vector2(55f, 100f);

	private float _max;

	private float _avg;

	private float _min;

	private StringBuilder _debugPanelSb = new StringBuilder();

	public bool AdaptiveLimits = true;

	public int ValuesToGraph = 500;

	public float MinimumValue;

	public float MaximumValue = 10f;

	private List<float> _graphValues = new List<float>(500);

	public Rectangle PerformancePanelBounds = new Rectangle(330, 100, 200, 100);

	private Vector2[] _background = new Vector2[4];

	public bool Enabled = true;

	public const int CircleSegments = 32;

	public DebugViewXNA(World world)
		: base(world)
	{
		ContactManager contactManager = world.ContactManager;
		contactManager.PreSolve = (PreSolveDelegate)Delegate.Combine(contactManager.PreSolve, new PreSolveDelegate(PreSolve));
		AppendFlags(DebugViewFlags.Shape);
		AppendFlags(DebugViewFlags.Controllers);
		AppendFlags(DebugViewFlags.Joint);
	}

	public void Dispose()
	{
		ContactManager contactManager = World.ContactManager;
		contactManager.PreSolve = (PreSolveDelegate)Delegate.Remove(contactManager.PreSolve, new PreSolveDelegate(PreSolve));
	}

	private void PreSolve(Contact contact, ref Manifold oldManifold)
	{
		if ((Flags & DebugViewFlags.ContactPoints) != DebugViewFlags.ContactPoints)
		{
			return;
		}
		Manifold manifold = contact.Manifold;
		if (manifold.PointCount == 0)
		{
			return;
		}
		Fixture fixtureA = contact.FixtureA;
		FarseerPhysics.Collision.Collision.GetPointStates(out var _, out var state2, ref oldManifold, ref manifold);
		contact.GetWorldManifold(out var normal, out var points);
		for (int i = 0; i < manifold.PointCount; i++)
		{
			if (_pointCount >= 2048)
			{
				break;
			}
			if (fixtureA == null)
			{
				_points[i] = default;
			}
			ContactPoint contactPoint = _points[_pointCount];
			contactPoint.Position = points[i];
			contactPoint.Normal = normal;
			contactPoint.State = state2[i];
			_points[_pointCount] = contactPoint;
			_pointCount++;
		}
	}

	private bool IsMyLayerDisabled(Body body)
	{
		if (body.UserData is GameObject gameObject && !gameObject.MyLayer.IsEnabledByEditor && GameCore.IsEditorEnabled)
		{
			return true;
		}
		return false;
	}

	private void DrawDebugData()
	{
		if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
		{
			foreach (Body body in World.BodyList)
			{
				if (IsMyLayerDisabled(body))
				{
					continue;
				}
				body.GetTransform(out var transform);
				Color red = Color.Red;
				foreach (Fixture fixture in body.FixtureList)
				{
					DrawShape(color: body.Enabled ? ((!body.IsSensor) ? ((body.BodyType != BodyType.Static) ? ((body.BodyType != BodyType.Kinematic) ? (body.Awake ? DefaultShapeColor : SleepingShapeColor) : ((body.UserData == null || !(body.UserData.GetType() == typeof(GameObject)) || !((GameObject)body.UserData).IsImmortal) ? KinematicShapeColor : KinematicShapeImmortalColor)) : ((body.UserData == null || !(body.UserData.GetType() == typeof(GameObject)) || !((GameObject)body.UserData).IsImmortal) ? StaticShapeColor : StaticShapeImmortalColor)) : SensorShapeColor) : InactiveShapeColor, fixture: fixture, xf: transform);
				}
			}
		}
		if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
		{
			for (int i = 0; i < _pointCount; i++)
			{
				ContactPoint contactPoint = _points[i];
				if (contactPoint.State == PointState.Add)
				{
					DrawPoint(contactPoint.Position, 0.1f, new Color(0.3f, 0.95f, 0.3f));
				}
				else if (contactPoint.State == PointState.Persist)
				{
					DrawPoint(contactPoint.Position, 0.1f, new Color(0.3f, 0.3f, 0.95f));
				}
				if ((Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
				{
					Vector2 position = contactPoint.Position;
					Vector2 end = position + 0.3f * contactPoint.Normal;
					DrawSegment(position, end, new Color(0.4f, 0.9f, 0.4f));
				}
			}
			_pointCount = 0;
		}
		if ((Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
		{
			foreach (Body body2 in World.BodyList)
			{
				if (IsMyLayerDisabled(body2))
				{
					continue;
				}
				foreach (Fixture fixture2 in body2.FixtureList)
				{
					if (fixture2.Shape is PolygonShape polygonShape)
					{
						body2.GetTransform(out var transform2);
						for (int j = 0; j < polygonShape.Vertices.Count; j++)
						{
							Vector2 p = MathUtils.Mul(ref transform2, polygonShape.Vertices[j]);
							DrawPoint(p, 0.1f, Color.Red);
						}
					}
				}
			}
		}
		if ((Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
		{
			foreach (Joint joint in World.JointList)
			{
				DrawJoint(joint);
			}
		}
		if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
		{
			Color color = new Color(0.9f, 0.3f, 0.9f);
			IBroadPhase broadPhase = World.ContactManager.BroadPhase;
			foreach (Body body3 in World.BodyList)
			{
				if (!body3.Enabled || IsMyLayerDisabled(body3))
				{
					continue;
				}
				foreach (Fixture fixture3 in body3.FixtureList)
				{
					for (int k = 0; k < fixture3.ProxyCount; k++)
					{
						FixtureProxy fixtureProxy = fixture3.Proxies[k];
						broadPhase.GetFatAABB(fixtureProxy.ProxyId, out var aabb);
						DrawAABB(ref aabb, color);
					}
				}
			}
		}
		if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
		{
			foreach (Body body4 in World.BodyList)
			{
				if (!IsMyLayerDisabled(body4))
				{
					body4.GetTransform(out var transform3);
					transform3.p = body4.WorldCenter;
					DrawTransform(ref transform3);
				}
			}
		}
		if ((Flags & DebugViewFlags.Controllers) == DebugViewFlags.Controllers)
		{
			for (int l = 0; l < World.ControllerList.Count; l++)
			{
				if (World.ControllerList[l] is BuoyancyController { Container: var aabb2 })
				{
					DrawAABB(ref aabb2, Color.LightBlue);
				}
			}
		}
		if ((Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
		{
			DrawDebugPanel();
		}
	}

	private void DrawPerformanceGraph()
	{
		_graphValues.Add(World.UpdateTime / 10000f);
		if (_graphValues.Count > ValuesToGraph + 1)
		{
			_graphValues.RemoveAt(0);
		}
		float num = PerformancePanelBounds.X;
		float num2 = (float)PerformancePanelBounds.Width / (float)ValuesToGraph;
		float num3 = (float)PerformancePanelBounds.Bottom - (float)PerformancePanelBounds.Top;
		if (_graphValues.Count > 2)
		{
			_max = _graphValues.Max();
			_avg = _graphValues.Average();
			_min = _graphValues.Min();
			if (AdaptiveLimits)
			{
				MaximumValue = _max;
				MinimumValue = 0f;
			}
			for (int num4 = _graphValues.Count - 1; num4 > 0; num4--)
			{
				float value = (float)PerformancePanelBounds.Bottom - _graphValues[num4] / (MaximumValue - MinimumValue) * num3;
				float value2 = (float)PerformancePanelBounds.Bottom - _graphValues[num4 - 1] / (MaximumValue - MinimumValue) * num3;
				Vector2 start = new Vector2(MathHelper.Clamp(num, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(value, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
				Vector2 end = new Vector2(MathHelper.Clamp(num + num2, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(value2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
				DrawSegment(start, end, Color.LightGreen);
				num += num2;
			}
		}
		DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top, $"Max: {_max} ms");
		DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Center.Y - 7, $"Avg: {_avg} ms");
		DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15, $"Min: {_min} ms");
		_background[0] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
		_background[1] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
		_background[2] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
		_background[3] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y);
		DrawSolidPolygon(_background, 4, Color.DarkGray);
	}

	private void DrawDebugPanel()
	{
		int num = 0;
		for (int i = 0; i < World.BodyList.Count; i++)
		{
			num += World.BodyList[i].FixtureList.Count;
		}
		int num2 = (int)DebugPanelPosition.X;
		int y = (int)DebugPanelPosition.Y;
		_debugPanelSb.Clear();
		_debugPanelSb.AppendLine("Objects:");
		_debugPanelSb.Append("- Bodies: ").AppendLine(World.BodyList.Count.ToString());
		_debugPanelSb.Append("- Fixtures: ").AppendLine(num.ToString());
		_debugPanelSb.Append("- Contacts: ").AppendLine(World.ContactList.Count.ToString());
		_debugPanelSb.Append("- Joints: ").AppendLine(World.JointList.Count.ToString());
		_debugPanelSb.Append("- Controllers: ").AppendLine(World.ControllerList.Count.ToString());
		_debugPanelSb.Append("- Proxies: ").AppendLine(World.ProxyCount.ToString());
		DrawString(num2, y, _debugPanelSb.ToString());
		_debugPanelSb.Clear();
		_debugPanelSb.AppendLine("Update time:");
		_debugPanelSb.Append("- Body: ").AppendLine($"{World.SolveUpdateTime / 10000f} ms");
		_debugPanelSb.Append("- Contact: ").AppendLine($"{World.ContactsUpdateTime / 10000f} ms");
		_debugPanelSb.Append("- CCD: ").AppendLine($"{World.ContinuousPhysicsTime / 10000f} ms");
		_debugPanelSb.Append("- Joint: ").AppendLine($"{World.Island.JointUpdateTime / 10000f} ms");
		_debugPanelSb.Append("- Controller: ").AppendLine($"{World.ControllersUpdateTime / 10000f} ms");
		_debugPanelSb.Append("- Total: ").AppendLine($"{World.UpdateTime / 10000f} ms");
		DrawString(num2 + 110, y, _debugPanelSb.ToString());
	}

	public void DrawAABB(ref AABB aabb, Color color)
	{
		DrawPolygon(new Vector2[4]
		{
			new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y),
			new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y),
			new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y),
			new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y)
		}, 4, color);
	}

	private void DrawJoint(Joint joint)
	{
		if (joint.Enabled)
		{
			Body bodyA = joint.BodyA;
			Body bodyB = joint.BodyB;
			bodyA.GetTransform(out var transform);
			Vector2 vector = Vector2.Zero;
			if (!joint.IsFixedType())
			{
				bodyB.GetTransform(out var transform2);
				vector = transform2.p;
			}
			Vector2 worldAnchorA = joint.WorldAnchorA;
			Vector2 worldAnchorB = joint.WorldAnchorB;
			Vector2 p = transform.p;
			Color color = new Color(0.5f, 0.8f, 0.8f);
			switch (joint.JointType)
			{
			case JointType.Distance:
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.Pulley:
			{
				PulleyJoint pulleyJoint = (PulleyJoint)joint;
				Vector2 worldPoint = bodyA.GetWorldPoint(pulleyJoint.LocalAnchorA);
				Vector2 worldPoint2 = bodyB.GetWorldPoint(pulleyJoint.LocalAnchorB);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				DrawSegment(worldAnchorA, worldPoint, color);
				DrawSegment(worldAnchorB, worldPoint2, color);
				break;
			}
			case JointType.FixedMouse:
				DrawPoint(worldAnchorA, 0.5f, new Color(0f, 1f, 0f));
				DrawSegment(worldAnchorA, worldAnchorB, new Color(0.8f, 0.8f, 0.8f));
				break;
			case JointType.Revolute:
				DrawSegment(p, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				DrawSegment(vector, worldAnchorB, color);
				DrawSolidCircle(worldAnchorB, 0.1f, Vector2.Zero, Color.Red);
				DrawSolidCircle(worldAnchorA, 0.1f, Vector2.Zero, Color.Blue);
				break;
			case JointType.FixedRevolute:
				DrawSegment(p, worldAnchorA, color);
				DrawSolidCircle(worldAnchorA, 0.1f, Vector2.Zero, Color.Pink);
				break;
			case JointType.FixedLine:
				DrawSegment(p, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.FixedDistance:
				DrawSegment(p, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.FixedPrismatic:
				DrawSegment(p, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.Gear:
				DrawSegment(p, vector, color);
				break;
			default:
				DrawSegment(p, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				DrawSegment(vector, worldAnchorB, color);
				break;
			case JointType.FixedAngle:
				break;
			}
		}
	}

	public void DrawShape(Fixture fixture, Transform xf, Color color)
	{
		switch (fixture.Shape.ShapeType)
		{
		case ShapeType.Circle:
		{
			CircleShape circleShape = (CircleShape)fixture.Shape;
			Vector2 center = MathUtils.Mul(ref xf, circleShape.Position);
			float radius = circleShape.Radius;
			Vector2 axis = MathUtils.Mul(xf.q, new Vector2(1f, 0f));
			DrawSolidCircle(center, radius, axis, color);
			break;
		}
		case ShapeType.Polygon:
		{
			PolygonShape polygonShape = (PolygonShape)fixture.Shape;
			int count = polygonShape.Vertices.Count;
			for (int j = 0; j < count; j++)
			{
				_tempVertices[j] = MathUtils.Mul(ref xf, polygonShape.Vertices[j]);
			}
			DrawSolidPolygon(_tempVertices, count, color);
			break;
		}
		case ShapeType.Edge:
		{
			EdgeShape edgeShape = (EdgeShape)fixture.Shape;
			Vector2 start2 = MathUtils.Mul(ref xf, edgeShape.Vertex1);
			Vector2 end2 = MathUtils.Mul(ref xf, edgeShape.Vertex2);
			DrawSegment(start2, end2, color);
			break;
		}
		case ShapeType.Chain:
		{
			ChainShape chainShape = (ChainShape)fixture.Shape;
			for (int i = 0; i < chainShape.Vertices.Count - 1; i++)
			{
				Vector2 start = MathUtils.Mul(ref xf, chainShape.Vertices[i]);
				Vector2 end = MathUtils.Mul(ref xf, chainShape.Vertices[i + 1]);
				DrawSegment(start, end, color);
			}
			break;
		}
		}
	}

	public override void DrawPolygon(Vector2[] vertices, int count, float red, float green, float blue, bool closed = true)
	{
		DrawPolygon(vertices, count, new Color(red, green, blue), closed);
	}

	public void DrawPolygon(Vector2[] vertices, int count, Color color, bool closed = true)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		for (int i = 0; i < count - 1; i++)
		{
			_primitiveBatch.AddVertex(vertices[i], color, PrimitiveType.LineList);
			_primitiveBatch.AddVertex(vertices[i + 1], color, PrimitiveType.LineList);
		}
		if (closed)
		{
			_primitiveBatch.AddVertex(vertices[count - 1], color, PrimitiveType.LineList);
			_primitiveBatch.AddVertex(vertices[0], color, PrimitiveType.LineList);
		}
	}

	public override void DrawSolidPolygon(Vector2[] vertices, int count, float red, float green, float blue)
	{
		DrawSolidPolygon(vertices, count, new Color(red, green, blue));
	}

	public void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline = true)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		if (count == 2)
		{
			DrawPolygon(vertices, count, color);
			return;
		}
		Color color2 = color * (outline ? 0.5f : 1f);
		for (int i = 1; i < count - 1; i++)
		{
			_primitiveBatch.AddVertex(vertices[0], color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertices[i], color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertices[i + 1], color2, PrimitiveType.TriangleList);
		}
		if (outline)
		{
			DrawPolygon(vertices, count, color);
		}
	}

	public override void DrawCircle(Vector2 center, float radius, float red, float green, float blue)
	{
		DrawCircle(center, radius, new Color(red, green, blue));
	}

	public void DrawCircle(Vector2 center, float radius, Color color)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		double num = 0.0;
		for (int i = 0; i < 32; i++)
		{
			Vector2 vertex = center + radius * new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
			Vector2 vertex2 = center + radius * new Vector2((float)Math.Cos(num + Math.PI / 16.0), (float)Math.Sin(num + Math.PI / 16.0));
			_primitiveBatch.AddVertex(vertex, color, PrimitiveType.LineList);
			_primitiveBatch.AddVertex(vertex2, color, PrimitiveType.LineList);
			num += Math.PI / 16.0;
		}
	}

	public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float green, float blue)
	{
		DrawSolidCircle(center, radius, axis, new Color(red, green, blue));
	}

	public void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		double num = 0.0;
		Color color2 = color * 0.5f;
		Vector2 vertex = center + radius * new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
		num += Math.PI / 16.0;
		for (int i = 1; i < 31; i++)
		{
			Vector2 vertex2 = center + radius * new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
			Vector2 vertex3 = center + radius * new Vector2((float)Math.Cos(num + Math.PI / 16.0), (float)Math.Sin(num + Math.PI / 16.0));
			_primitiveBatch.AddVertex(vertex, color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertex2, color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertex3, color2, PrimitiveType.TriangleList);
			num += Math.PI / 16.0;
		}
		DrawCircle(center, radius, color);
		DrawSegment(center, center + axis * radius, color);
	}

	public override void DrawSegment(Vector2 start, Vector2 end, float red, float green, float blue)
	{
		DrawSegment(start, end, new Color(red, green, blue));
	}

	public void DrawSegment(Vector2 start, Vector2 end, Color color)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		_primitiveBatch.AddVertex(start, color, PrimitiveType.LineList);
		_primitiveBatch.AddVertex(end, color, PrimitiveType.LineList);
	}

	public override void DrawTransform(ref Transform transform)
	{
		Vector2 p = transform.p;
		Vector2 end = p + 0.4f * transform.q.GetXAxis();
		DrawSegment(p, end, Color.Red);
		end = p + 0.4f * transform.q.GetYAxis();
		DrawSegment(p, end, Color.Green);
	}

	public void DrawPoint(Vector2 p, float size, Color color)
	{
		Vector2[] array = new Vector2[4];
		float num = size / 2f;
		array[0] = p + new Vector2(0f - num, 0f - num);
		array[1] = p + new Vector2(num, 0f - num);
		array[2] = p + new Vector2(num, num);
		array[3] = p + new Vector2(0f - num, num);
		DrawSolidPolygon(array, 4, color);
	}

	public void DrawString(int x, int y, string text)
	{
		DrawString(new Vector2(x, y), text);
	}

	public void DrawString(Vector2 position, string text)
	{
		_stringData.Add(new StringData(position, text, TextColor));
	}

	public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color)
	{
		DrawSegment(start, end, color);
		float num = width / 2f;
		Vector2 vector = start - end;
		vector.Normalize();
		Matrix matrix = Matrix.CreateRotationZ((float)Math.Atan2(vector.X, 0f - vector.Y));
		Matrix matrix2 = Matrix.CreateTranslation(end.X, end.Y, 0f);
		Vector2[] array = new Vector2[3]
		{
			new Vector2(0f, 0f),
			new Vector2(0f - num, 0f - length),
			new Vector2(num, 0f - length)
		};
		Vector2.Transform(array, ref matrix, array);
		Vector2.Transform(array, ref matrix2, array);
		DrawSolidPolygon(array, 3, color, outline: false);
		if (drawStartIndicator)
		{
			Matrix matrix3 = Matrix.CreateTranslation(start.X, start.Y, 0f);
			Vector2[] array2 = new Vector2[4]
			{
				new Vector2(0f - num, length / 4f),
				new Vector2(num, length / 4f),
				new Vector2(num, 0f),
				new Vector2(0f - num, 0f)
			};
			Vector2.Transform(array2, ref matrix, array2);
			Vector2.Transform(array2, ref matrix3, array2);
			DrawSolidPolygon(array2, 4, color, outline: false);
		}
	}

	public void BeginCustomDraw(Matrix projection, Matrix view)
	{
		BeginCustomDraw(ref projection, ref view);
	}

	public void BeginCustomDraw(ref Matrix projection, ref Matrix view)
	{
		_ = Matrix.Identity;
		_primitiveBatch.Begin(ref projection, ref view);
	}

	public void EndCustomDraw()
	{
		_primitiveBatch.End();
	}

	public void RenderDebugData(Matrix projection, Matrix view)
	{
		_ = Matrix.Identity;
		RenderDebugData(ref projection, ref view);
	}

	public void RenderDebugData(ref Matrix projection, ref Matrix view)
	{
		if (Enabled && Flags != 0)
		{
			_device.RasterizerState = RasterizerState.CullNone;
			_device.DepthStencilState = DepthStencilState.Default;
			_primitiveBatch.Begin(ref projection, ref view);
			DrawDebugData();
			_primitiveBatch.End();
			if ((Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
			{
				_primitiveBatch.Begin(ref _localProjection, ref _localView);
				DrawPerformanceGraph();
				_primitiveBatch.End();
			}
			_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			for (int i = 0; i < _stringData.Count; i++)
			{
			}
			_batch.End();
			_stringData.Clear();
		}
	}

	public void RenderDebugData(ref Matrix projection)
	{
		if (Enabled)
		{
			Matrix view = Matrix.Identity;
			_ = Matrix.Identity;
			RenderDebugData(ref projection, ref view);
		}
	}

	public void LoadContent(GraphicsDevice device, ContentManager content)
	{
		_device = device;
		_batch = new SpriteBatch(_device);
		_primitiveBatch = new PrimitiveBatch(_device, 1000);
		_stringData = new List<StringData>();
		_localProjection = Matrix.CreateOrthographicOffCenter(0f, _device.Viewport.Width, _device.Viewport.Height, 0f, 0f, 1f);
		_localView = Matrix.Identity;
	}
}
