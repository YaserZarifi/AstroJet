using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace Rake2D;

public static class Utils
{
	public const float PI = (float)Math.PI;

	public const float PI2 = (float)Math.PI * 2f;

	public static NumberFormatInfo FloatFI;

	public static NumberFormatInfo MoneyFI;

	public static Random Random;

	public const float TO_RAD = (float)Math.PI / 180f;

	static Utils()
	{
		Random = new Random();
		FloatFI = new NumberFormatInfo();
		FloatFI.NumberDecimalSeparator = ".";
		FloatFI.NumberDecimalDigits = 2;
		MoneyFI = new NumberFormatInfo();
		int[] numberGroupSizes = new int[3] { 3, 3, 3 };
		MoneyFI.NumberGroupSizes = numberGroupSizes;
		MoneyFI.NumberGroupSeparator = " ";
	}

	public static BlendState GetBlendState(BlendModeEnum blendType)
	{
		if (blendType == BlendModeEnum.Additive)
		{
			return BlendState.Additive;
		}
		return BlendState.AlphaBlend;
	}

	public static void DecomposeMatrix(ref Matrix matrix, out Vector2 position, out float rotation, out Vector2 scale)
	{
		try
		{
			matrix.Decompose(out var scale2, out var rotation2, out var translation);
			Vector2 vector = Vector2.Transform(Vector2.UnitX, rotation2);
			rotation = MathUtils.Atan2(vector.Y, vector.X);
			position = new Vector2(translation.X, translation.Y);
			scale = new Vector2(scale2.X, scale2.Y);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to decompose matrix: {0}", ex);
			rotation = 0f;
			position = Vector2.Zero;
			scale = Vector2.One;
		}
	}

	public static Vector2 GetVector2(int[] valueArray)
	{
		if (valueArray.Length > 1)
		{
			return new Vector2(valueArray[0], valueArray[1]);
		}
		return new Vector2(valueArray[0], valueArray[0]);
	}

	public static Vector2 GetVector2(float[] valueArray)
	{
		if (valueArray.Length > 1)
		{
			return new Vector2(valueArray[0], valueArray[1]);
		}
		return new Vector2(valueArray[0], valueArray[0]);
	}

	public static Vector3 GetVector3(float[] valueArray)
	{
		if (valueArray.Length > 2)
		{
			return new Vector3(valueArray[0], valueArray[1], valueArray[2]);
		}
		if (valueArray.Length > 1)
		{
			return new Vector3(valueArray[0], valueArray[1], valueArray[1]);
		}
		return new Vector3(valueArray[0], valueArray[0], valueArray[0]);
	}

	public static Vector2[] GetRectangleVertices(ref Rectangle rect, Transformation transform)
	{
		Matrix mx = Matrix.CreateScale(transform.Scale.X, transform.Scale.Y, 1f) * Matrix.CreateRotationZ(transform.Rotation);
		return GetRectangleVertices(ref rect, mx);
	}

	public static Color GetColorByBlendMode(Color color, BlendModeEnum blendMode)
	{
		if (blendMode == BlendModeEnum.Additive)
		{
			color *= (float)(int)color.A / 255f;
			color.A = 0;
			return color;
		}
		return Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);
	}

	public static Vector2[] GetRectangleVertices(ref Rectangle rect, Matrix mx)
	{
		Vector2[] rectangleVertices = GameCore.Allocator.GetRectangleVertices();
		rectangleVertices[0] = Vector2.Transform(new Vector2(rect.Left, rect.Top), mx);
		rectangleVertices[1] = Vector2.Transform(new Vector2(rect.Right, rect.Top), mx);
		rectangleVertices[2] = Vector2.Transform(new Vector2(rect.Right, rect.Bottom), mx);
		rectangleVertices[3] = Vector2.Transform(new Vector2(rect.Left, rect.Bottom), mx);
		return rectangleVertices;
	}

	public static int GetRandom(int from, int to, bool bRandonNegative = false)
	{
		int num = ((to < from) ? Random.Next(to, from + 1) : Random.Next(from, to + 1));
		if (bRandonNegative && GetRandom(0, 1) == 1)
		{
			num *= -1;
		}
		return num;
	}

	public static int GetRandom(IntRange range)
	{
		return GetRandom(range.From, range.To);
	}

	public static float GetRandom(FloatRange range)
	{
		return GetRandom(range.From, range.To);
	}

	public static int GetRandom(int max)
	{
		return Random.Next(max + 1);
	}

	public static float GetRandom(float from, float to)
	{
		return (float)(Random.NextDouble() * (double)(to - from) + (double)from);
	}

	public static float GetRandom(float[] minMaxArray)
	{
		if (minMaxArray != null)
		{
			if (minMaxArray.Length == 1)
			{
				return minMaxArray[0];
			}
			if (minMaxArray.Length > 1)
			{
				return GetRandom(minMaxArray[0], minMaxArray[1]);
			}
		}
		return 0f;
	}

	public static Color GetRandomColor(int r1, int g1, int b1, int r2, int g2, int b2)
	{
		return new Color(GetRandom(r1, r2), GetRandom(g1, g2), GetRandom(b1, b2));
	}

	public static Vector2 GetRandomVector2(float fromX, float fromY, float toX, float toY)
	{
		return new Vector2(GetRandom(fromX, toX), GetRandom(fromY, toY));
	}

	public static T GetRandomListItem<T>(List<T> itemList, T defaultValue)
	{
		if (itemList != null)
		{
			int num = Enumerable.Count(itemList);
			if (num > 0)
			{
				int index = Random.Next(num);
				return itemList.ElementAt(index);
			}
		}
		return defaultValue;
	}

	public static int GetRandomItem(int[] array, int defaultValue = 0)
	{
		if (array == null || array.Length < 1)
		{
			return defaultValue;
		}
		return array[Random.Next(array.Length)];
	}

	public static float GetRandomItem(float[] array, float defaultValue = 0f)
	{
		if (array == null || array.Length < 1)
		{
			return defaultValue;
		}
		return array[Random.Next(array.Length)];
	}

	public static string GetRandomItem(string[] array, string defaultValue = "")
	{
		if (array == null || array.Length < 1)
		{
			return defaultValue;
		}
		return array[Random.Next(array.Length)];
	}

	public static T GetRandomItem<T>(List<T> inputList)
	{
		if (inputList == null || inputList.Count < 1)
		{
			return default;
		}
		return inputList[GetRandom(inputList.Count - 1)];
	}

	public static List<T> GetRandomItemList<T>(this List<T> inputList, int maxItemCount)
	{
		List<T> list = new List<T>();
		List<T> list2 = new List<T>(inputList);
		int num = 0;
		while (list2.Count > 0 && num < maxItemCount)
		{
			int random = GetRandom(0, list2.Count - 1);
			list.Add(list2[random]);
			list2.RemoveAt(random);
			num++;
		}
		return list;
	}

	public static void AddList<T>(this List<T> self, List<T> other)
	{
		if (other != null)
		{
			int count = other.Count;
			for (int i = 0; i < count; i++)
			{
				self.Add(other[i]);
			}
		}
	}

	public static void MoveList<T>(this List<T> self, List<T> other)
	{
		self.AddList(other);
		other.Clear();
	}

	public static void AddVertex(this List<PolygonVertex> list, Vector2 position)
	{
		list.Add(GameCore.Allocator.GetPolygonVertex(position));
	}

	public static string ToLowerLazy(this string s)
	{
		bool flag = true;
		foreach (char c in s)
		{
			if (c >= 'A' && c <= 'Z')
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return s;
		}
		return s.ToLowerInvariant();
	}

	public static float DegToRad(float angle)
	{
		return (float)Math.PI * angle / 180f;
	}

	public static float RadToDeg(float angle)
	{
		return angle * (180f / (float)Math.PI);
	}

	public static float GetAngleToInDeg(Vector2 vec1, Vector2 vec2, float north = 0f)
	{
		return RadToDeg(GetRotationTo(vec1, vec2, DegToRad(north)));
	}

	public static float GetRotationTo(Vector2 vec1, Vector2 vec2, float north = 0f)
	{
		float num = MathUtils.Atan2(vec2.Y - vec1.Y, vec2.X - vec1.X) + (float)Math.PI / 2f - north;
		if (num > (float)Math.PI)
		{
			num -= (float)Math.PI * 2f;
		}
		else if (num < -(float)Math.PI)
		{
			num += (float)Math.PI * 2f;
		}
		return num;
	}

	public static float GetRotationFromZero(Vector2 vec1)
	{
		float num = 0f - MathUtils.Atan2(vec1.X, vec1.Y) + (float)Math.PI;
		if (num > (float)Math.PI)
		{
			num = (float)Math.PI * -2f + num;
		}
		return num;
	}

	public static Vector2 GetParallelPoint(Vector2 startPoint, float rotation, float offset)
	{
		Vector2 vector = new Vector2(startPoint.X + Sin(rotation), startPoint.Y - Cos(rotation)) - startPoint;
		vector.Normalize();
		return startPoint + new Vector2(vector.Y, 0f - vector.X) * offset;
	}

	public static Vector2 GetVelocityVector2(float speed, float rotation)
	{
		return new Vector2(speed * Sin(rotation), (0f - speed) * Cos(rotation));
	}

	public static float GetDistance(Vector2 pos1, Vector2 pos2)
	{
		return Vector2.Distance(pos1, pos2);
	}

	public static Vector2 RotateVector2(Vector2 vec, float rotation)
	{
		float num = Cos(rotation);
		float num2 = Sin(rotation);
		return new Vector2(num * vec.X - num2 * vec.Y, num2 * vec.X + num * vec.Y);
	}

	public static bool IsPointInRect(Vector2 pointPos, Vector2 rectCenterPos, float width, float height, float rotation, Vector2 scale)
	{
		Matrix matrix = Matrix.CreateRotationZ(0f - rotation);
		Vector2 position = pointPos - rectCenterPos;
		position = Vector2.Transform(position, matrix);
		Vector2 vector = new Vector2((0f - width) * scale.X / 2f, (0f - height) * scale.Y / 2f);
		Vector2 vector2 = new Vector2(width * scale.X / 2f, height * scale.Y / 2f);
		if (position.X > vector.X && position.X < vector2.X && position.Y > vector.Y && position.Y < vector2.Y)
		{
			return true;
		}
		return false;
	}

	public static bool IsPointInFixedRect(Vector2 pointPos, Vector2 rectCenterPos, float width, float height)
	{
		Vector2 vector = pointPos - rectCenterPos;
		Vector2 vector2 = new Vector2((0f - width) / 2f, (0f - height) / 2f);
		Vector2 vector3 = new Vector2(width / 2f, height / 2f);
		if (vector.X > vector2.X && vector.X < vector3.X && vector.Y > vector2.Y && vector.Y < vector3.Y)
		{
			return true;
		}
		return false;
	}

	public static bool Equals(string value1, string value2)
	{
		return value1?.Equals(value2, StringComparison.OrdinalIgnoreCase) ?? (value2 == null);
	}

	public static bool ChangeValueToTarget(ref Vector2 currentValue, Vector2 targetValue, float speed)
	{
		Vector2 vector = targetValue - currentValue;
		if (vector == Vector2.Zero)
		{
			return true;
		}
		vector.Normalize();
		vector *= speed;
		bool flag = ChangeValueToTarget(ref currentValue.X, targetValue.X, Math.Abs(vector.X));
		return ChangeValueToTarget(ref currentValue.Y, targetValue.Y, Math.Abs(vector.Y)) & flag;
	}

	public static bool ChangeValueToTarget(ref Vector2 currentValue, Vector2 targetValue, float speed, Vector2 velocity)
	{
		Vector2 vector = targetValue - currentValue;
		if (vector == Vector2.Zero)
		{
			return true;
		}
		vector.Normalize();
		vector *= speed;
		bool flag = ChangeValueToTarget(ref currentValue.X, targetValue.X, Math.Abs(vector.X) + velocity.X);
		return ChangeValueToTarget(ref currentValue.Y, targetValue.Y, Math.Abs(vector.Y) + velocity.Y) & flag;
	}

	public static bool ChangeValueToTarget(ref Vector3 currentValue, Vector3 targetValue, float speed)
	{
		Vector3 vector = targetValue - currentValue;
		if (vector == Vector3.Zero)
		{
			return true;
		}
		vector.Normalize();
		vector *= speed;
		bool flag = ChangeValueToTarget(ref currentValue.X, targetValue.X, Math.Abs(vector.X));
		bool flag2 = ChangeValueToTarget(ref currentValue.Y, targetValue.Y, Math.Abs(vector.Y));
		return ChangeValueToTarget(ref currentValue.Z, targetValue.Z, Math.Abs(vector.Z)) & flag & flag2;
	}

	public static bool ChangeValueToTarget(ref float currentValue, float targetValue, float speed)
	{
		if (currentValue < targetValue)
		{
			currentValue += speed;
			if (currentValue >= targetValue)
			{
				currentValue = targetValue;
				return true;
			}
		}
		else
		{
			if (!(currentValue > targetValue))
			{
				return true;
			}
			currentValue -= speed;
			if (currentValue <= targetValue)
			{
				currentValue = targetValue;
				return true;
			}
		}
		return false;
	}

	public static bool ChangeValueToTarget(ref double currentValue, double targetValue, double speed)
	{
		if (currentValue < targetValue)
		{
			currentValue += speed;
			if (currentValue >= targetValue)
			{
				currentValue = targetValue;
				return true;
			}
		}
		else
		{
			if (!(currentValue > targetValue))
			{
				return true;
			}
			currentValue -= speed;
			if (currentValue <= targetValue)
			{
				currentValue = targetValue;
				return true;
			}
		}
		return false;
	}

	public static bool ChangeAngleToTarget(ref float currentAngle, float targetAngle, float step, bool ccw)
	{
		if (ccw)
		{
			currentAngle -= step;
			if (currentAngle < -180f)
			{
				currentAngle += 360f;
			}
			else if (currentAngle <= targetAngle && ((currentAngle <= 0f && targetAngle <= 0f) || (currentAngle > 0f && targetAngle > 0f)))
			{
				currentAngle = targetAngle;
				return true;
			}
		}
		else
		{
			currentAngle += step;
			if (currentAngle > 180f)
			{
				currentAngle -= 360f;
			}
			if (currentAngle >= targetAngle && ((currentAngle < 0f && targetAngle < 0f) || (currentAngle >= 0f && targetAngle >= 0f)))
			{
				currentAngle = targetAngle;
				return true;
			}
		}
		return false;
	}

	[Obsolete]
	public static bool ChangeAngleToTarget(ref float currentAngle, float targetAngle, float speed)
	{
		if (currentAngle < 0f && targetAngle > 0f)
		{
			currentAngle -= speed;
			if (currentAngle < -180f)
			{
				currentAngle += 360f;
				if (currentAngle < targetAngle)
				{
					currentAngle = targetAngle;
					return true;
				}
			}
		}
		else
		{
			if (!(currentAngle > 0f) || !(targetAngle < 0f))
			{
				return ChangeValueToTarget(ref currentAngle, targetAngle, speed);
			}
			currentAngle += speed;
			if (currentAngle < 180f)
			{
				currentAngle -= 360f;
				if (currentAngle > targetAngle)
				{
					currentAngle = targetAngle;
					return true;
				}
			}
		}
		return false;
	}

	public static int Clamp(int value, int min, int max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}

	public static float Clamp(float value, float min, float max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}

	public static long Clamp(long value, long min, long max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}

	public static double Clamp(double value, double min, double max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}

	public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
	{
		return new Vector2(Clamp(value.X, min.X, max.X), Clamp(value.Y, min.Y, max.Y));
	}

	public static float Clamp(float value, IntRange range)
	{
		return Clamp(value, range.From, range.To);
	}

	public static float Clamp(float value, FloatRange range)
	{
		return Clamp(value, range.From, range.To);
	}

	public static float ClampAngle(float value, float min, float max, bool clockwise)
	{
		if (clockwise)
		{
			return Clamp(value, min, max);
		}
		if (value > min && value <= 0f)
		{
			return min;
		}
		if (value < max && value >= 0f)
		{
			return max;
		}
		return value;
	}

	public static bool ParseBool(string value)
	{
		if (!bool.TryParse(value, out var result))
		{
			if (!int.TryParse(value, out var result2))
			{
				return false;
			}
			return result2 != 0;
		}
		return result;
	}

	public static float ClampAngle(float value, IntRange range, bool clockwise)
	{
		return ClampAngle(value, range.From, range.To, clockwise);
	}

	public static Color MultiplyColor(Color color, float factor)
	{
		Color result = color * factor;
		result.A = color.A;
		return result;
	}

	public static int[] GetIntArray(string str)
	{
		int[] array = null;
		if (str != null)
		{
			string[] array2 = str.Split(',');
			array = new int[array2.Length];
			int num = 0;
			string[] array3 = array2;
			foreach (string s in array3)
			{
				int result = 0;
				int.TryParse(s, out result);
				array[num++] = result;
			}
		}
		else
		{
			GameCore.LogErr("Wrong integer params");
		}
		return array;
	}

	public static float[] GetFloatArray(string str)
	{
		float[] array = null;
		if (str != null)
		{
			string[] array2 = str.Split(',');
			array = new float[array2.Length];
			int num = 0;
			string[] array3 = array2;
			foreach (string str2 in array3)
			{
				array[num++] = StringToFloat(str2);
			}
		}
		return array;
	}

	public static bool TryGetIntArray(string intListStr, int lenMin, int lenMax, out int[] resultArray)
	{
		int[] intArray = GetIntArray(intListStr);
		if (intArray == null)
		{
			resultArray = null;
			return false;
		}
		if (intArray != null && intArray.Length >= lenMin && intArray.Length <= lenMax)
		{
			resultArray = intArray;
			return true;
		}
		GameCore.LogErr("Wrong param count ({0} <> [{1} - {2}])", intArray.Length, lenMin, lenMax);
		resultArray = null;
		return false;
	}

	public static bool TryGetFloatArray(string intListStr, int lenMin, int lenMax, out float[] resultArray)
	{
		float[] floatArray = GetFloatArray(intListStr);
		if (floatArray == null)
		{
			resultArray = null;
			return false;
		}
		if (floatArray != null && floatArray.Length >= lenMin && floatArray.Length <= lenMax)
		{
			resultArray = floatArray;
			return true;
		}
		GameCore.LogErr("Wrong param count ({0} <> [{1} - {2}])", floatArray.Length, lenMin, lenMax);
		resultArray = null;
		return false;
	}

	public static float GetMinimumDistance(Vector2 v, Vector2 w, Vector2 p)
	{
		float num = (w - v).LengthSquared();
		if (num == 0f)
		{
			return GetDistance(p, v);
		}
		float num2 = Math.Max(0f, Math.Min(1f, Vector2.Dot(p - v, w - v) / num));
		Vector2 pos = v + num2 * (w - v);
		return GetDistance(p, pos);
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable == null)
		{
			return true;
		}
		if (enumerable is ICollection<T> collection)
		{
			return collection.Count < 1;
		}
		return !enumerable.Any();
	}

	public static int[,] RotateMatrixClockwise(int[,] oldMatrix)
	{
		int[,] array = new int[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
		int num = 0;
		for (int i = 0; i < oldMatrix.GetLength(1); i++)
		{
			int num2 = 0;
			for (int num3 = oldMatrix.GetLength(0) - 1; num3 >= 0; num3--)
			{
				array[num, num2] = oldMatrix[num3, i];
				num2++;
			}
			num++;
		}
		return array;
	}

	public static int[,] RotateMatrixCounterClockwise(int[,] oldMatrix)
	{
		int[,] array = new int[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
		int num = 0;
		for (int num2 = oldMatrix.GetLength(1) - 1; num2 >= 0; num2--)
		{
			int num3 = 0;
			for (int i = 0; i < oldMatrix.GetLength(0); i++)
			{
				array[num, num3] = oldMatrix[i, num2];
				num3++;
			}
			num++;
		}
		return array;
	}

	public static Vector2 Abs(Vector2 vec)
	{
		return new Vector2(Math.Abs(vec.X), Math.Abs(vec.Y));
	}

	public static float Sin(float rotation)
	{
		return MathUtils.Sin(rotation);
	}

	public static float Cos(float rotation)
	{
		return MathUtils.Cos(rotation);
	}

	public static float Sqrt(float value)
	{
		return (float)Math.Sqrt(value);
	}

	public static string FormatDecimal1(double value)
	{
		return value.ToString("0.#", FloatFI);
	}

	public static string FormatFloat2(float value)
	{
		return value.ToString("0.##", FloatFI);
	}

	public static string FormatFloat4(float value)
	{
		return value.ToString("0.####", FloatFI);
	}

	public static string FormatDouble2(double value)
	{
		return value.ToString("0.##", FloatFI);
	}

	public static string FormatMoney(long value)
	{
		return value.ToString("N0", MoneyFI);
	}

	public static string FormatPrice(long value, bool isRed)
	{
		string arg;
		string arg2;
		if (isRed)
		{
			arg = "\\9";
			arg2 = "\\9";
		}
		else
		{
			arg = "\\1";
			arg2 = "\\0";
		}
		return $"{arg2}✐ {arg}{FormatMoney(value)}";
	}

	public static string FormatBigNumber(double value, bool keepSmallDecimal = true)
	{
		double num = Math.Abs(value);
		if (keepSmallDecimal && num < 100.0)
		{
			return FormatDecimal1(value);
		}
		if (num < 10000.0)
		{
			return value.ToString("N0", MoneyFI);
		}
		if (num < 100000.0)
		{
			return $"{FormatDecimal1(value / 1000.0)} K";
		}
		if (num < 10000000.0)
		{
			return string.Format("{0} K", ((int)value / 1000).ToString("N0", MoneyFI));
		}
		if (num < 100000000.0)
		{
			return $"{FormatDecimal1(value / 1000000.0)} M";
		}
		if (num < 10000000000.0)
		{
			return string.Format("{0} M", ((long)value / 1000000).ToString("N0", MoneyFI));
		}
		if (num < 100000000000.0)
		{
			return string.Format("{0} B", ((long)value / 1000000000).ToString("N0", MoneyFI));
		}
		if (num < 10000000000000.0)
		{
			return string.Format("{0} B", ((long)value / 1000000000).ToString("N0", MoneyFI));
		}
		if (num < 100000000000000.0)
		{
			return string.Format("{0} T", ((long)value / 1000000000000L).ToString("N0", MoneyFI));
		}
		if (num < 10000000000000000.0)
		{
			return string.Format("{0} T", ((long)value / 1000000000000L).ToString("N0", MoneyFI));
		}
		if (double.IsNaN(num) || double.IsInfinity(num))
		{
			return "Err";
		}
		return string.Format("{0} S", ((long)value / 1000000000000L).ToString("N0", MoneyFI));
	}

	public static float StringToFloat(string str, float defaultValue = 0f)
	{
		if (!float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	internal static float RadToDeg(object rotation)
	{
		throw new NotImplementedException();
	}

	public static string GetDotNetVersion()
	{
		using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\");
		if (registryKey != null && registryKey.GetValue("Release") != null)
		{
			return CheckFor45PlusVersion((int)registryKey.GetValue("Release"));
		}
		return "< 4.5";
	}

	private static string CheckFor45PlusVersion(int releaseKey)
	{
		if (releaseKey >= 461808)
		{
			return "4.7.2 or later";
		}
		if (releaseKey >= 461308)
		{
			return "4.7.1";
		}
		if (releaseKey >= 460798)
		{
			return "4.7";
		}
		if (releaseKey >= 394802)
		{
			return "4.6.2";
		}
		if (releaseKey >= 394254)
		{
			return "4.6.1";
		}
		if (releaseKey >= 393295)
		{
			return "4.6";
		}
		if (releaseKey >= 379893)
		{
			return "4.5.2";
		}
		if (releaseKey >= 378675)
		{
			return "4.5.1";
		}
		if (releaseKey >= 378389)
		{
			return "4.5";
		}
		return "No 4.5 or later version detected";
	}

	internal static Vector2 GetVelocityVector2(float beamLength, object beamRotation)
	{
		throw new NotImplementedException();
	}
}
