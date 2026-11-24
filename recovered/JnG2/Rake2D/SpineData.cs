using System;
using System.Collections.Generic;
using System.IO;
using Rake2D.Spine;
using Spine;

namespace Rake2D;

public class SpineData
{
	public string Id;

	public string FileName;

	public Atlas Atlas;

	public string SkeletonFileName;

	public SkeletonData SkeletonData;

	public AnimationStateData AnimationStateData;

	public bool IsBinary;

	public bool IsInitialized;

	private List<SpineMix> _mixList;

	public SpineData(string id)
	{
		Id = id;
		IsBinary = false;
	}

	public SpineMix AddMix(string animNameFrom, string animNameTo, float duration)
	{
		if (_mixList == null)
		{
			_mixList = new List<SpineMix>();
		}
		SpineMix spineMix = new SpineMix
		{
			AnimNameFrom = animNameFrom,
			AnimNameTo = animNameTo,
			Duration = duration
		};
		_mixList.Add(spineMix);
		return spineMix;
	}

	public void Init()
	{
		try
		{
			if (SkeletonFileName == null)
			{
				GameCore.LogErr($"Undefined skeleton file name for [spine = {Id}] in file '{FileName}'");
				return;
			}
			if (IsBinary)
			{
				SkeletonBinary skeletonBinary = new SkeletonBinary(Atlas);
				using (GameContent.GetFileStream(SkeletonFileName))
				{
					SkeletonData = skeletonBinary.ReadSkeletonData(SkeletonFileName);
				}
			}
			else
			{
				SkeletonJson skeletonJson = new SkeletonJson(Atlas);
				using Stream stream = GameContent.GetFileStream(SkeletonFileName);
				if (stream != null)
				{
					SkeletonData = skeletonJson.ReadSkeletonData(new StreamReader(stream));
				}
			}
			int num = 0;
			if (SkeletonData != null)
			{
				for (int num2 = SkeletonData.Bones.Count - 1; num2 >= 0; num2--)
				{
					BoneData boneData = SkeletonData.Bones.Items[num2];
					for (int num3 = num2 - 1; num3 >= 0; num3--)
					{
						BoneData boneData2 = SkeletonData.Bones.Items[num3];
						if (Utils.Equals(boneData.Name, boneData2.Name))
						{
							GameCore.LogErr($"Duplicity spine bone name {boneData.Name} = {boneData2.Name} in file '{FileName}' - renamed to {boneData.Name}_duplicate_{num}");
							boneData.SetName(boneData.Name + "_duplicate_" + num);
							num++;
							break;
						}
					}
				}
			}
			AnimationStateData = new AnimationStateData(SkeletonData);
			if (_mixList != null)
			{
				foreach (SpineMix mix in _mixList)
				{
					try
					{
						AnimationStateData.SetMix(mix.AnimNameFrom, mix.AnimNameTo, mix.Duration);
					}
					catch (Exception ex)
					{
						GameCore.LogErr($"Error in mix in [Spine = {Id}] from file '{SkeletonFileName}' in file '{FileName}': {ex.ToString()}");
					}
				}
			}
			IsInitialized = true;
		}
		catch (Exception ex2)
		{
			GameCore.LogErr($"Unable to load skeleton for [Spine = {Id}] from file '{SkeletonFileName}' in file '{FileName}': {ex2.ToString()}");
		}
	}

	internal void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
		FileName = iniFile.FileName;
		while (iniFile.NextLine())
		{
			if (DataLoader.CheckParams(iniFile, content))
			{
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection)
			{
				break;
			}
			string[] resultArray;
			if (iniFile.KeyEquals("atlas"))
			{
				try
				{
					string pathFromValue = iniFile.GetPathFromValue();
					using Stream stream = GameContent.GetFileStream(pathFromValue);
					if (stream != null)
					{
						Atlas = new Atlas(new StreamReader(stream), Path.GetDirectoryName(pathFromValue), new XnaTextureLoader(GameCore.GraphicsDeviceStatic));
					}
				}
				catch (Exception ex)
				{
					iniFile.LogErr("Unable to load Spine atlas from file '{0}': {1}", iniFile.Value, ex.ToString());
				}
			}
			else if (iniFile.KeyEquals("skeleton"))
			{
				SkeletonFileName = iniFile.GetPathFromValue();
			}
			else if (iniFile.KeyEquals("files"))
			{
				string pathFromValue2 = DataUtils.GetPathFromValue(iniFile.Value + ".atlas", Path.GetDirectoryName(iniFile.FileName));
				try
				{
					using Stream stream2 = GameContent.GetFileStream(pathFromValue2);
					if (stream2 != null)
					{
						Atlas = new Atlas(new StreamReader(stream2), Path.GetDirectoryName(pathFromValue2), new XnaTextureLoader(GameCore.GraphicsDeviceStatic));
					}
				}
				catch (Exception ex2)
				{
					iniFile.LogErr("Unable to load Spine atlas from file '{0}': {1}", pathFromValue2, ex2.ToString());
				}
				SkeletonFileName = DataUtils.GetPathFromValue(iniFile.Value + ".json", Path.GetDirectoryName(iniFile.FileName));
			}
			else if (iniFile.KeyEquals("binary"))
			{
				IsBinary = iniFile.GetIntValue() != 0;
			}
			else if (iniFile.KeyEquals("mix") && iniFile.TryGetStringArray(3, out resultArray))
			{
				if (float.TryParse(resultArray[2], out var result))
				{
					AddMix(resultArray[0], resultArray[1], result / 1000f);
				}
				else
				{
					iniFile.LogErr("Wrong duration param.");
				}
			}
			else
			{
				iniFile.LogErr("Unknown spine command");
			}
		}
	}
}
