using System;
using System.Collections.Generic;
using UnityEngine;

public interface INGUIAtlas
{
	Material spriteMaterial { get; set; }

	List<UISpriteData> spriteList { get; set; }

	Texture texture { get; }

	float pixelSize { get; set; }

	bool premultipliedAlpha { get; }

	INGUIAtlas replacement { get; set; }

	UISpriteData GetSprite(string name);

	BetterList<string> GetListOfSprites();

	BetterList<string> GetListOfSprites(string match);

	bool References(INGUIAtlas atlas);

	void MarkAsChanged();

	void SortAlphabetically();
}

public class NGUIAtlas : ScriptableObject, INGUIAtlas
{
	private enum Coordinates
	{
		Pixels,
		TexCoords
	}

	[SerializeField]
	[HideInInspector]
	private Material material;

	[SerializeField]
	[HideInInspector]
	private List<UISpriteData> mSprites = new List<UISpriteData>();

	[SerializeField]
	[HideInInspector]
	private float mPixelSize = 1f;

	[SerializeField]
	[HideInInspector]
	private UnityEngine.Object mReplacement;

	[NonSerialized]
	private int mPMA = -1;

	[NonSerialized]
	private Dictionary<string, int> mSpriteIndices = new Dictionary<string, int>();

	public Material spriteMaterial
	{
		get
		{
			INGUIAtlas iNGUIAtlas = replacement;
			return (iNGUIAtlas == null) ? material : iNGUIAtlas.spriteMaterial;
		}
		set
		{
			INGUIAtlas iNGUIAtlas = replacement;
			if (iNGUIAtlas != null)
			{
				iNGUIAtlas.spriteMaterial = value;
				return;
			}
			if (material == null)
			{
				mPMA = 0;
				material = value;
				return;
			}
			MarkAsChanged();
			mPMA = -1;
			material = value;
			MarkAsChanged();
		}
	}

	public bool premultipliedAlpha
	{
		get
		{
			INGUIAtlas iNGUIAtlas = replacement;
			if (iNGUIAtlas != null)
			{
				return iNGUIAtlas.premultipliedAlpha;
			}
			if (mPMA == -1)
			{
				Material material = spriteMaterial;
				mPMA = ((material != null && material.shader != null && material.shader.name.Contains("Premultiplied")) ? 1 : 0);
			}
			return mPMA == 1;
		}
	}

	public List<UISpriteData> spriteList
	{
		get
		{
			INGUIAtlas iNGUIAtlas = replacement;
			if (iNGUIAtlas != null)
			{
				return iNGUIAtlas.spriteList;
			}
			return mSprites;
		}
		set
		{
			INGUIAtlas iNGUIAtlas = replacement;
			if (iNGUIAtlas != null)
			{
				iNGUIAtlas.spriteList = value;
			}
			else
			{
				mSprites = value;
			}
		}
	}

	public Texture texture
	{
		get
		{
			INGUIAtlas iNGUIAtlas = replacement;
			return (iNGUIAtlas != null) ? iNGUIAtlas.texture : ((!(material != null)) ? null : material.mainTexture);
		}
	}

	public float pixelSize
	{
		get
		{
			INGUIAtlas iNGUIAtlas = replacement;
			return (iNGUIAtlas == null) ? mPixelSize : iNGUIAtlas.pixelSize;
		}
		set
		{
			INGUIAtlas iNGUIAtlas = replacement;
			if (iNGUIAtlas != null)
			{
				iNGUIAtlas.pixelSize = value;
				return;
			}
			float num = Mathf.Clamp(value, 0.25f, 4f);
			if (mPixelSize != num)
			{
				mPixelSize = num;
				MarkAsChanged();
			}
		}
	}

	public INGUIAtlas replacement
	{
		get
		{
			if (mReplacement == null)
			{
				return null;
			}
			return mReplacement as INGUIAtlas;
		}
		set
		{
			INGUIAtlas iNGUIAtlas = value;
			if (iNGUIAtlas == this as INGUIAtlas)
			{
				iNGUIAtlas = null;
			}
			if (mReplacement as INGUIAtlas != iNGUIAtlas)
			{
				if (iNGUIAtlas != null && iNGUIAtlas.replacement == this as INGUIAtlas)
				{
					iNGUIAtlas.replacement = null;
				}
				if (mReplacement != null)
				{
					MarkAsChanged();
				}
				mReplacement = iNGUIAtlas as UnityEngine.Object;
				if (iNGUIAtlas != null)
				{
					material = null;
				}
				MarkAsChanged();
			}
		}
	}

	public UISpriteData GetSprite(string name)
	{
		INGUIAtlas iNGUIAtlas = replacement;
		if (iNGUIAtlas != null)
		{
			return iNGUIAtlas.GetSprite(name);
		}
		if (!string.IsNullOrEmpty(name))
		{
			if (mSprites.Count == 0) return null;

			// O(1) lookup via a dictionary
#if UNITY_EDITOR
			if (Application.isPlaying)
#endif
			{
				// The number of indices differs from the sprite list? Rebuild the indices.
				if (mSpriteIndices.Count != mSprites.Count)
					MarkSpriteListAsChanged();

				int index;
				if (mSpriteIndices.TryGetValue(name, out index))
				{
					// If the sprite is present, return it as-is
					if (index > -1 && index < mSprites.Count) return mSprites[index];

					// The sprite index was out of range -- perhaps the sprite was removed? Rebuild the indices.
					MarkSpriteListAsChanged();

					// Try to look up the index again
					return mSpriteIndices.TryGetValue(name, out index) ? mSprites[index] : null;
				}
			}

			// Sequential O(N) lookup.
			for (int i = 0, imax = mSprites.Count; i < imax; ++i)
			{
				UISpriteData s = mSprites[i];

				// string.Equals doesn't seem to work with Flash export
				if (!string.IsNullOrEmpty(s.name) && name == s.name)
				{
#if UNITY_EDITOR
					if (!Application.isPlaying) return s;
#endif
					// If this point was reached then the sprite is present in the non-indexed list,
					// so the sprite indices should be updated.
					MarkSpriteListAsChanged();
					return s;
				}
			}
		}
		return null;
	}

	public void MarkSpriteListAsChanged()
	{
#if UNITY_EDITOR
		if (Application.isPlaying)
#endif
		{
			mSpriteIndices.Clear();
			int i = 0;
			for (int count = mSprites.Count; i < count; i++)
			{
				mSpriteIndices[mSprites[i].name] = i;
			}
		}
	}

	public void SortAlphabetically()
	{
		mSprites.Sort((UISpriteData s1, UISpriteData s2) => s1.name.CompareTo(s2.name));
#if UNITY_EDITOR
		NGUITools.SetDirty(this);
#endif
	}

	public BetterList<string> GetListOfSprites()
	{
		INGUIAtlas iNGUIAtlas = replacement;
		if (iNGUIAtlas != null)
		{
			return iNGUIAtlas.GetListOfSprites();
		}
		BetterList<string> betterList = new BetterList<string>();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			UISpriteData uISpriteData = mSprites[i];
			if (uISpriteData != null && !string.IsNullOrEmpty(uISpriteData.name))
			{
				betterList.Add(uISpriteData.name);
			}
		}
		return betterList;
	}

	public BetterList<string> GetListOfSprites(string match)
	{
		INGUIAtlas iNGUIAtlas = replacement;
		if (iNGUIAtlas != null)
		{
			return iNGUIAtlas.GetListOfSprites(match);
		}
		if (string.IsNullOrEmpty(match))
		{
			return GetListOfSprites();
		}
		BetterList<string> betterList = new BetterList<string>();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			UISpriteData uISpriteData = mSprites[i];
			if (uISpriteData != null && !string.IsNullOrEmpty(uISpriteData.name) && string.Equals(match, uISpriteData.name, StringComparison.OrdinalIgnoreCase))
			{
				betterList.Add(uISpriteData.name);
				return betterList;
			}
		}
		string[] array = match.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = array[j].ToLower();
		}
		int k = 0;
		for (int count2 = mSprites.Count; k < count2; k++)
		{
			UISpriteData uISpriteData2 = mSprites[k];
			if (uISpriteData2 == null || string.IsNullOrEmpty(uISpriteData2.name))
			{
				continue;
			}
			string text = uISpriteData2.name.ToLower();
			int num = 0;
			for (int l = 0; l < array.Length; l++)
			{
				if (text.Contains(array[l]))
				{
					num++;
				}
			}
			if (num == array.Length)
			{
				betterList.Add(uISpriteData2.name);
			}
		}
		return betterList;
	}

	public bool References(INGUIAtlas atlas)
	{
		if (atlas == null)
		{
			return false;
		}
		if (atlas == this as INGUIAtlas)
		{
			return true;
		}
		INGUIAtlas iNGUIAtlas = replacement;
		return iNGUIAtlas != null && iNGUIAtlas.References(atlas);
	}

	public void MarkAsChanged()
	{
#if UNITY_EDITOR
		NGUITools.SetDirty(this);
#endif
		INGUIAtlas iNGUIAtlas = replacement;
		if (iNGUIAtlas != null)
		{
			iNGUIAtlas.MarkAsChanged();
		}
		UISprite[] array = NGUITools.FindActive<UISprite>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UISprite uISprite = array[i];
			if (NGUITools.CheckIfRelated(this, uISprite.atlas))
			{
				INGUIAtlas atlas = uISprite.atlas;
				uISprite.atlas = null;
				uISprite.atlas = atlas;
#if UNITY_EDITOR
				NGUITools.SetDirty(uISprite);
#endif
			}
		}
		NGUIFont[] array2 = Resources.FindObjectsOfTypeAll<NGUIFont>();
		int j = 0;
		for (int num2 = array2.Length; j < num2; j++)
		{
			NGUIFont nGUIFont = array2[j];
			if (nGUIFont.atlas != null && NGUITools.CheckIfRelated(this, nGUIFont.atlas))
			{
				INGUIAtlas atlas2 = nGUIFont.atlas;
				nGUIFont.atlas = null;
				nGUIFont.atlas = atlas2;
#if UNITY_EDITOR
				NGUITools.SetDirty(nGUIFont);
#endif
			}
		}
		UIFont[] array3 = Resources.FindObjectsOfTypeAll<UIFont>();
		int k = 0;
		for (int num3 = array3.Length; k < num3; k++)
		{
			UIFont uIFont = array3[k];
			if (NGUITools.CheckIfRelated(this, uIFont.atlas))
			{
				INGUIAtlas atlas3 = uIFont.atlas;
				uIFont.atlas = null;
				uIFont.atlas = atlas3;
#if UNITY_EDITOR
				NGUITools.SetDirty(uIFont);
#endif
			}
		}
		UILabel[] array4 = NGUITools.FindActive<UILabel>();
		int l = 0;
		for (int num4 = array4.Length; l < num4; l++)
		{
			UILabel uILabel = array4[l];
			if (uILabel.atlas != null && NGUITools.CheckIfRelated(this, uILabel.atlas))
			{
				INGUIFont bitmapFont = uILabel.bitmapFont;
				uILabel.bitmapFont = null;
				uILabel.bitmapFont = bitmapFont;
#if UNITY_EDITOR
				NGUITools.SetDirty(uILabel);
#endif
			}
		}
	}
}
