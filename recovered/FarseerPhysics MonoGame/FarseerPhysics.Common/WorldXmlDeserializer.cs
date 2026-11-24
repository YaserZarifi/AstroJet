using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common;

internal static class WorldXmlDeserializer
{
	internal static World Deserialize(Stream stream)
	{
		World world = new World(Vector2.Zero);
		Deserialize(world, stream);
		return world;
	}

	private static void Deserialize(World world, Stream stream)
	{
		List<Body> list = new List<Body>();
		List<Fixture> list2 = new List<Fixture>();
		List<Joint> list3 = new List<Joint>();
		List<Shape> list4 = new List<Shape>();
		XMLFragmentElement xMLFragmentElement = XMLFragmentParser.LoadFromStream(stream);
		if (xMLFragmentElement.Name.ToLower() != "world")
		{
			throw new Exception();
		}
		foreach (XMLFragmentElement element in xMLFragmentElement.Elements)
		{
			if (element.Name.ToLower() == "gravity")
			{
				world.Gravity = ReadVector(element);
				break;
			}
		}
		foreach (XMLFragmentElement element2 in xMLFragmentElement.Elements)
		{
			if (!(element2.Name.ToLower() == "shapes"))
			{
				continue;
			}
			foreach (XMLFragmentElement element3 in element2.Elements)
			{
				if (element3.Name.ToLower() != "shape")
				{
					throw new Exception();
				}
				ShapeType shapeType = (ShapeType)Enum.Parse(typeof(ShapeType), element3.Attributes[0].Value, ignoreCase: true);
				float density = float.Parse(element3.Attributes[1].Value);
				switch (shapeType)
				{
				case ShapeType.Circle:
				{
					CircleShape circleShape = new CircleShape();
					circleShape._density = density;
					foreach (XMLFragmentElement element4 in element3.Elements)
					{
						string text = element4.Name.ToLower();
						if (!(text == "radius"))
						{
							if (!(text == "position"))
							{
								throw new Exception();
							}
							circleShape.Position = ReadVector(element4);
						}
						else
						{
							circleShape.Radius = float.Parse(element4.Value);
						}
					}
					list4.Add(circleShape);
					break;
				}
				case ShapeType.Polygon:
				{
					PolygonShape polygonShape = new PolygonShape();
					polygonShape._density = density;
					foreach (XMLFragmentElement element5 in element3.Elements)
					{
						string text = element5.Name.ToLower();
						if (!(text == "vertices"))
						{
							if (text == "centroid")
							{
								polygonShape.MassData.Centroid = ReadVector(element5);
							}
							continue;
						}
						List<Vector2> list6 = new List<Vector2>(element5.Elements.Count);
						foreach (XMLFragmentElement element6 in element5.Elements)
						{
							list6.Add(ReadVector(element6));
						}
						polygonShape.Vertices = new Vertices(list6);
					}
					list4.Add(polygonShape);
					break;
				}
				case ShapeType.Edge:
				{
					EdgeShape edgeShape = new EdgeShape();
					edgeShape._density = density;
					foreach (XMLFragmentElement element7 in element3.Elements)
					{
						switch (element7.Name.ToLower())
						{
						case "hasvertex0":
							edgeShape.HasVertex0 = bool.Parse(element7.Value);
							break;
						case "hasvertex3":
							edgeShape.HasVertex0 = bool.Parse(element7.Value);
							break;
						case "vertex0":
							edgeShape.Vertex0 = ReadVector(element7);
							break;
						case "vertex1":
							edgeShape.Vertex1 = ReadVector(element7);
							break;
						case "vertex2":
							edgeShape.Vertex2 = ReadVector(element7);
							break;
						case "vertex3":
							edgeShape.Vertex3 = ReadVector(element7);
							break;
						default:
							throw new Exception();
						}
					}
					list4.Add(edgeShape);
					break;
				}
				case ShapeType.Chain:
				{
					ChainShape chainShape = new ChainShape();
					chainShape._density = density;
					foreach (XMLFragmentElement element8 in element3.Elements)
					{
						switch (element8.Name.ToLower())
						{
						case "vertices":
						{
							List<Vector2> list5 = new List<Vector2>(element8.Elements.Count);
							foreach (XMLFragmentElement element9 in element8.Elements)
							{
								list5.Add(ReadVector(element9));
							}
							chainShape.Vertices = new Vertices(list5);
							break;
						}
						case "nextvertex":
							chainShape.NextVertex = ReadVector(element8);
							break;
						case "prevvertex":
							chainShape.PrevVertex = ReadVector(element8);
							break;
						default:
							throw new Exception();
						}
					}
					list4.Add(chainShape);
					break;
				}
				}
			}
		}
		foreach (XMLFragmentElement element10 in xMLFragmentElement.Elements)
		{
			if (!(element10.Name.ToLower() == "fixtures"))
			{
				continue;
			}
			foreach (XMLFragmentElement element11 in element10.Elements)
			{
				Fixture fixture = new Fixture();
				if (element11.Name.ToLower() != "fixture")
				{
					throw new Exception();
				}
				fixture.FixtureId = int.Parse(element11.Attributes[0].Value);
				foreach (XMLFragmentElement element12 in element11.Elements)
				{
					switch (element12.Name.ToLower())
					{
					case "filterdata":
						foreach (XMLFragmentElement element13 in element12.Elements)
						{
							switch (element13.Name.ToLower())
							{
							case "categorybits":
								fixture._collisionCategories = (Category)int.Parse(element13.Value);
								break;
							case "maskbits":
								fixture._collidesWith = (Category)int.Parse(element13.Value);
								break;
							case "groupindex":
								fixture._collisionGroup = short.Parse(element13.Value);
								break;
							case "CollisionIgnores":
							{
								string[] array = element13.Value.Split('|');
								foreach (string s in array)
								{
									fixture._collisionIgnores.Add(int.Parse(s));
								}
								break;
							}
							}
						}
						break;
					case "friction":
						fixture.Friction = float.Parse(element12.Value);
						break;
					case "issensor":
						fixture.IsSensor = bool.Parse(element12.Value);
						break;
					case "restitution":
						fixture.Restitution = float.Parse(element12.Value);
						break;
					case "userdata":
						fixture.UserData = ReadSimpleType(element12, null, outer: false);
						break;
					}
				}
				list2.Add(fixture);
			}
		}
		foreach (XMLFragmentElement element14 in xMLFragmentElement.Elements)
		{
			if (!(element14.Name.ToLower() == "bodies"))
			{
				continue;
			}
			foreach (XMLFragmentElement element15 in element14.Elements)
			{
				Body body = new Body(world);
				if (element15.Name.ToLower() != "body")
				{
					throw new Exception();
				}
				body.BodyType = (BodyType)Enum.Parse(typeof(BodyType), element15.Attributes[0].Value, ignoreCase: true);
				foreach (XMLFragmentElement element16 in element15.Elements)
				{
					switch (element16.Name.ToLower())
					{
					case "active":
						body._enabled = bool.Parse(element16.Value);
						break;
					case "allowsleep":
						body.SleepingAllowed = bool.Parse(element16.Value);
						break;
					case "angle":
					{
						Vector2 position2 = body.Position;
						body.SetTransformIgnoreContacts(ref position2, float.Parse(element16.Value));
						break;
					}
					case "angulardamping":
						body.AngularDamping = float.Parse(element16.Value);
						break;
					case "angularvelocity":
						body.AngularVelocity = float.Parse(element16.Value);
						break;
					case "awake":
						body.Awake = bool.Parse(element16.Value);
						break;
					case "bullet":
						body.IsBullet = bool.Parse(element16.Value);
						break;
					case "fixedrotation":
						body.FixedRotation = bool.Parse(element16.Value);
						break;
					case "lineardamping":
						body.LinearDamping = float.Parse(element16.Value);
						break;
					case "linearvelocity":
						body.LinearVelocity = ReadVector(element16);
						break;
					case "position":
					{
						float rotation = body.Rotation;
						Vector2 position = ReadVector(element16);
						body.SetTransformIgnoreContacts(ref position, rotation);
						break;
					}
					case "userdata":
						body.UserData = ReadSimpleType(element16, null, outer: false);
						break;
					case "bindings":
						foreach (XMLFragmentElement element17 in element16.Elements)
						{
							Fixture fixture2 = list2[int.Parse(element17.Attributes[0].Value)];
							fixture2.Shape = list4[int.Parse(element17.Attributes[1].Value)].Clone();
							fixture2.CloneOnto(body);
						}
						break;
					}
				}
				list.Add(body);
			}
		}
		foreach (XMLFragmentElement element18 in xMLFragmentElement.Elements)
		{
			if (!(element18.Name.ToLower() == "joints"))
			{
				continue;
			}
			foreach (XMLFragmentElement element19 in element18.Elements)
			{
				if (element19.Name.ToLower() != "joint")
				{
					throw new Exception();
				}
				JointType jointType = (JointType)Enum.Parse(typeof(JointType), element19.Attributes[0].Value, ignoreCase: true);
				int index = -1;
				int index2 = -1;
				bool collideConnected = false;
				object userData = null;
				foreach (XMLFragmentElement element20 in element19.Elements)
				{
					switch (element20.Name.ToLower())
					{
					case "bodya":
						index = int.Parse(element20.Value);
						break;
					case "bodyb":
						index2 = int.Parse(element20.Value);
						break;
					case "collideconnected":
						collideConnected = bool.Parse(element20.Value);
						break;
					case "userdata":
						userData = ReadSimpleType(element20, null, outer: false);
						break;
					}
				}
				Body bodyA = list[index];
				Body bodyB = list[index2];
				Joint joint = jointType switch
				{
					JointType.Distance => (Joint)new DistanceJoint(), 
					JointType.Friction => new FrictionJoint(), 
					JointType.Wheel => new WheelJoint(), 
					JointType.Prismatic => new PrismaticJoint(), 
					JointType.Pulley => new PulleyJoint(), 
					JointType.Revolute => new RevoluteJoint(), 
					JointType.Weld => new WeldJoint(), 
					JointType.Rope => new RopeJoint(), 
					JointType.Angle => new AngleJoint(), 
					JointType.Motor => new MotorJoint(), 
					JointType.Gear => throw new Exception("GearJoint is not supported."), 
					_ => throw new Exception("Invalid or unsupported joint."), 
				};
				joint.CollideConnected = collideConnected;
				joint.UserData = userData;
				joint.BodyA = bodyA;
				joint.BodyB = bodyB;
				list3.Add(joint);
				world.AddJoint(joint);
				foreach (XMLFragmentElement element21 in element19.Elements)
				{
					switch (jointType)
					{
					case JointType.Distance:
						switch (element21.Name.ToLower())
						{
						case "dampingratio":
							((DistanceJoint)joint).DampingRatio = float.Parse(element21.Value);
							break;
						case "frequencyhz":
							((DistanceJoint)joint).Frequency = float.Parse(element21.Value);
							break;
						case "length":
							((DistanceJoint)joint).Length = float.Parse(element21.Value);
							break;
						case "localanchora":
							((DistanceJoint)joint).LocalAnchorA = ReadVector(element21);
							break;
						case "localanchorb":
							((DistanceJoint)joint).LocalAnchorB = ReadVector(element21);
							break;
						}
						break;
					case JointType.Friction:
						switch (element21.Name.ToLower())
						{
						case "localanchora":
							((FrictionJoint)joint).LocalAnchorA = ReadVector(element21);
							break;
						case "localanchorb":
							((FrictionJoint)joint).LocalAnchorB = ReadVector(element21);
							break;
						case "maxforce":
							((FrictionJoint)joint).MaxForce = float.Parse(element21.Value);
							break;
						case "maxtorque":
							((FrictionJoint)joint).MaxTorque = float.Parse(element21.Value);
							break;
						}
						break;
					case JointType.Wheel:
						switch (element21.Name.ToLower())
						{
						case "enablemotor":
							((WheelJoint)joint).MotorEnabled = bool.Parse(element21.Value);
							break;
						case "localanchora":
							((WheelJoint)joint).LocalAnchorA = ReadVector(element21);
							break;
						case "localanchorb":
							((WheelJoint)joint).LocalAnchorB = ReadVector(element21);
							break;
						case "motorspeed":
							((WheelJoint)joint).MotorSpeed = float.Parse(element21.Value);
							break;
						case "dampingratio":
							((WheelJoint)joint).DampingRatio = float.Parse(element21.Value);
							break;
						case "maxmotortorque":
							((WheelJoint)joint).MaxMotorTorque = float.Parse(element21.Value);
							break;
						case "frequencyhz":
							((WheelJoint)joint).Frequency = float.Parse(element21.Value);
							break;
						case "axis":
							((WheelJoint)joint).Axis = ReadVector(element21);
							break;
						}
						break;
					case JointType.Prismatic:
						switch (element21.Name.ToLower())
						{
						case "enablelimit":
							((PrismaticJoint)joint).LimitEnabled = bool.Parse(element21.Value);
							break;
						case "enablemotor":
							((PrismaticJoint)joint).MotorEnabled = bool.Parse(element21.Value);
							break;
						case "localanchora":
							((PrismaticJoint)joint).LocalAnchorA = ReadVector(element21);
							break;
						case "localanchorb":
							((PrismaticJoint)joint).LocalAnchorB = ReadVector(element21);
							break;
						case "axis":
							((PrismaticJoint)joint).Axis = ReadVector(element21);
							break;
						case "maxmotorforce":
							((PrismaticJoint)joint).MaxMotorForce = float.Parse(element21.Value);
							break;
						case "motorspeed":
							((PrismaticJoint)joint).MotorSpeed = float.Parse(element21.Value);
							break;
						case "lowertranslation":
							((PrismaticJoint)joint).LowerLimit = float.Parse(element21.Value);
							break;
						case "uppertranslation":
							((PrismaticJoint)joint).UpperLimit = float.Parse(element21.Value);
							break;
						case "referenceangle":
							((PrismaticJoint)joint).ReferenceAngle = float.Parse(element21.Value);
							break;
						}
						break;
					case JointType.Pulley:
						switch (element21.Name.ToLower())
						{
						case "worldanchora":
							((PulleyJoint)joint).WorldAnchorA = ReadVector(element21);
							break;
						case "worldanchorb":
							((PulleyJoint)joint).WorldAnchorB = ReadVector(element21);
							break;
						case "lengtha":
							((PulleyJoint)joint).LengthA = float.Parse(element21.Value);
							break;
						case "lengthb":
							((PulleyJoint)joint).LengthB = float.Parse(element21.Value);
							break;
						case "localanchora":
							((PulleyJoint)joint).LocalAnchorA = ReadVector(element21);
							break;
						case "localanchorb":
							((PulleyJoint)joint).LocalAnchorB = ReadVector(element21);
							break;
						case "ratio":
							((PulleyJoint)joint).Ratio = float.Parse(element21.Value);
							break;
						case "constant":
							((PulleyJoint)joint).Constant = float.Parse(element21.Value);
							break;
						}
						break;
					case JointType.Revolute:
						switch (element21.Name.ToLower())
						{
						case "enablelimit":
							((RevoluteJoint)joint).LimitEnabled = bool.Parse(element21.Value);
							break;
						case "enablemotor":
							((RevoluteJoint)joint).MotorEnabled = bool.Parse(element21.Value);
							break;
						case "localanchora":
							((RevoluteJoint)joint).LocalAnchorA = ReadVector(element21);
							break;
						case "localanchorb":
							((RevoluteJoint)joint).LocalAnchorB = ReadVector(element21);
							break;
						case "maxmotortorque":
							((RevoluteJoint)joint).MaxMotorTorque = float.Parse(element21.Value);
							break;
						case "motorspeed":
							((RevoluteJoint)joint).MotorSpeed = float.Parse(element21.Value);
							break;
						case "lowerangle":
							((RevoluteJoint)joint).LowerLimit = float.Parse(element21.Value);
							break;
						case "upperangle":
							((RevoluteJoint)joint).UpperLimit = float.Parse(element21.Value);
							break;
						case "referenceangle":
							((RevoluteJoint)joint).ReferenceAngle = float.Parse(element21.Value);
							break;
						}
						break;
					case JointType.Weld:
					{
						string text = element21.Name.ToLower();
						if (!(text == "localanchora"))
						{
							if (text == "localanchorb")
							{
								((WeldJoint)joint).LocalAnchorB = ReadVector(element21);
							}
						}
						else
						{
							((WeldJoint)joint).LocalAnchorA = ReadVector(element21);
						}
						break;
					}
					case JointType.Rope:
						switch (element21.Name.ToLower())
						{
						case "localanchora":
							((RopeJoint)joint).LocalAnchorA = ReadVector(element21);
							break;
						case "localanchorb":
							((RopeJoint)joint).LocalAnchorB = ReadVector(element21);
							break;
						case "maxlength":
							((RopeJoint)joint).MaxLength = float.Parse(element21.Value);
							break;
						}
						break;
					case JointType.Gear:
						throw new Exception("Gear joint is unsupported");
					case JointType.Angle:
						switch (element21.Name.ToLower())
						{
						case "biasfactor":
							((AngleJoint)joint).BiasFactor = float.Parse(element21.Value);
							break;
						case "maximpulse":
							((AngleJoint)joint).MaxImpulse = float.Parse(element21.Value);
							break;
						case "softness":
							((AngleJoint)joint).Softness = float.Parse(element21.Value);
							break;
						case "targetangle":
							((AngleJoint)joint).TargetAngle = float.Parse(element21.Value);
							break;
						}
						break;
					case JointType.Motor:
						switch (element21.Name.ToLower())
						{
						case "angularoffset":
							((MotorJoint)joint).AngularOffset = float.Parse(element21.Value);
							break;
						case "linearoffset":
							((MotorJoint)joint).LinearOffset = ReadVector(element21);
							break;
						case "maxforce":
							((MotorJoint)joint).MaxForce = float.Parse(element21.Value);
							break;
						case "maxtorque":
							((MotorJoint)joint).MaxTorque = float.Parse(element21.Value);
							break;
						case "correctionfactor":
							((MotorJoint)joint).CorrectionFactor = float.Parse(element21.Value);
							break;
						}
						break;
					}
				}
			}
		}
		world.ProcessChanges();
	}

	private static Vector2 ReadVector(XMLFragmentElement node)
	{
		string[] array = node.Value.Split(' ');
		return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
	}

	private static object ReadSimpleType(XMLFragmentElement node, Type type, bool outer)
	{
		if (type == null)
		{
			return ReadSimpleType(node.Elements[1], Type.GetType(node.Elements[0].Value), outer);
		}
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		new XmlSerializerNamespaces().Add("", "");
		using MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(outer ? node.OuterXml : node.InnerXml);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
		return xmlSerializer.Deserialize(XmlReader.Create(memoryStream, xmlReaderSettings));
	}
}
