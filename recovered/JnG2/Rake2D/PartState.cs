using Microsoft.Xna.Framework;

namespace Rake2D;

public class PartState
{
	public const string DEFAULT_ID = "idle";

	public string Id;

	public Vector2 Offset;

	public Vector2 Scale;

	public float Rotation;

	public SpriteAtlas Atlas;

	public string AtlasId;

	public int Frame;

	public Color Color;

	public int ZOrder;

	public string AnimId;

	public string RemconText;

	public float AnimDuration;

	public string SpineAnimId;

	public bool IsAnimLooped;

	public bool IsEnabled;

	public bool IsVisible;

	public bool IsImmortal;

	public bool EmitterReset;

	public bool PhysEnabled;

	public bool PathPaused;

	public Vector2 Velocity;

	public string[] SoundIdArray;

	public bool IsSoundLooped;

	public bool UseOffset;

	public bool UseScale;

	public bool UseAtlas;

	public bool UseFrame;

	public bool UseRotation;

	public bool UseColor;

	public bool UseZOrder;

	public bool UseAnim;

	public bool UseSound;

	public bool UseSpineAnim;

	public bool UseEnabled;

	public bool UseVisible;

	public bool UseImmortal;

	public bool UseEmitterReset;

	public bool UsePhysEnabled;

	public bool UsePathPaused;

	public bool UseRemconText;

	public bool UseVelocity;

	public PartState(string id)
	{
		Id = id;
		ZOrder = 0;
		Scale = new Vector2(1f, 1f);
		AnimDuration = 1f;
		IsAnimLooped = false;
		IsEnabled = true;
		IsVisible = true;
		IsImmortal = false;
		PhysEnabled = true;
		Color = Color.White;
	}

	public PartState Clone()
	{
		PartState partState = new PartState(Id);
		partState.Offset = Offset;
		partState.Scale = Scale;
		partState.Rotation = Rotation;
		partState.Velocity = Velocity;
		partState.ZOrder = ZOrder;
		partState.AtlasId = AtlasId;
		partState.Atlas = Atlas;
		partState.Frame = Frame;
		partState.Color = Color;
		partState.AnimId = AnimId;
		partState.SpineAnimId = SpineAnimId;
		partState.RemconText = RemconText;
		partState.AnimDuration = AnimDuration;
		partState.IsAnimLooped = IsAnimLooped;
		partState.IsEnabled = IsEnabled;
		partState.IsVisible = IsVisible;
		partState.IsImmortal = IsImmortal;
		partState.EmitterReset = EmitterReset;
		partState.PhysEnabled = PhysEnabled;
		partState.PathPaused = PathPaused;
		partState.IsSoundLooped = IsSoundLooped;
		if (!SoundIdArray.IsNullOrEmpty())
		{
			partState.SoundIdArray = new string[SoundIdArray.Length];
			SoundIdArray.CopyTo(partState.SoundIdArray, 0);
		}
		partState.UseOffset = UseOffset;
		partState.UseScale = UseScale;
		partState.UseRotation = UseRotation;
		partState.UseVelocity = UseVelocity;
		partState.UseZOrder = UseZOrder;
		partState.UseAtlas = UseAtlas;
		partState.UseFrame = UseFrame;
		partState.UseColor = UseColor;
		partState.UseAnim = UseAnim;
		partState.UseSound = UseSound;
		partState.UseSpineAnim = UseSpineAnim;
		partState.UseEnabled = UseEnabled;
		partState.UseVisible = UseVisible;
		partState.UseImmortal = UseImmortal;
		partState.UseEmitterReset = UseEmitterReset;
		partState.UsePhysEnabled = UsePhysEnabled;
		partState.UsePathPaused = UsePathPaused;
		partState.UseRemconText = UseRemconText;
		return partState;
	}

	public PartState MergeCopy(PartState mergedState)
	{
		PartState partState = mergedState.Clone();
		if (UseOffset)
		{
			partState.Offset += Offset;
			partState.UseOffset = true;
		}
		if (UseScale)
		{
			partState.Scale *= Scale;
			partState.UseScale = true;
		}
		if (UseRotation)
		{
			partState.Rotation += Rotation;
			partState.UseRotation = true;
		}
		if (UseZOrder)
		{
			partState.ZOrder = ZOrder;
			partState.UseZOrder = true;
		}
		return partState;
	}

	public override string ToString()
	{
		return Id;
	}
}
