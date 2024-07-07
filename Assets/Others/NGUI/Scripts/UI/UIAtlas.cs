﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class UIAtlas : MonoBehaviour, INGUIAtlas
{
	[Serializable]
	private class Sprite
	{
		public string name = "Unity Bug";

		public Rect outer = new Rect(0f, 0f, 1f, 1f);

		public Rect inner = new Rect(0f, 0f, 1f, 1f);

		public bool rotated;

		public float paddingLeft;

		public float paddingRight;

		public float paddingTop;

		public float paddingBottom;

		public bool hasPadding
		{
			get
			{
				return paddingLeft != 0f || paddingRight != 0f || paddingTop != 0f || paddingBottom != 0f;
			}
		}
	}

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

	[HideInInspector]
	[SerializeField]
	private float mPixelSize = 1f;

	[HideInInspector]
	[SerializeField]
	private UnityEngine.Object mReplacement;

	[SerializeField]
	[HideInInspector]
	private Coordinates mCoordinates;

	[HideInInspector]
	[SerializeField]
	private List<Sprite> sprites = new List<Sprite>();

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
			if (mSprites.Count == 0)
			{
				Upgrade();
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
			if (mSprites.Count == 0)
			{
				Upgrade();
			}
			if (mSprites.Count == 0)
			{
				return null;
			}
#if UNITY_EDITOR
			if (Application.isPlaying)
#endif
			{
				if (mSpriteIndices.Count != mSprites.Count)
				{
					MarkSpriteListAsChanged();
				}
				int value;
				if (mSpriteIndices.TryGetValue(name, out value))
				{
					if (value > -1 && value < mSprites.Count)
					{
						return mSprites[value];
					}
					MarkSpriteListAsChanged();
					return (!mSpriteIndices.TryGetValue(name, out value)) ? null : mSprites[value];
				}
			}
			int i = 0;
			for (int count = mSprites.Count; i < count; i++)
			{
				UISpriteData uISpriteData = mSprites[i];
				if (!string.IsNullOrEmpty(uISpriteData.name) && name == uISpriteData.name)
				{
#if UNITY_EDITOR
					if (!Application.isPlaying) return uISpriteData;
#endif
					MarkSpriteListAsChanged();
					return uISpriteData;
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
		if (mSprites.Count == 0)
		{
			Upgrade();
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
		if (mSprites.Count == 0)
		{
			Upgrade();
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
		NGUITools.SetDirty(gameObject);
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
			}
		}
	}

	private bool Upgrade()
	{
		INGUIAtlas iNGUIAtlas = replacement;
		if (iNGUIAtlas != null)
		{
			UIAtlas uIAtlas = iNGUIAtlas as UIAtlas;
			if (uIAtlas != null)
			{
				return uIAtlas.Upgrade();
			}
		}
		if (mSprites.Count == 0 && sprites.Count > 0 && (bool)material)
		{
			Texture mainTexture = material.mainTexture;
			int width = ((!(mainTexture != null)) ? 512 : mainTexture.width);
			int height = ((!(mainTexture != null)) ? 512 : mainTexture.height);
			for (int i = 0; i < sprites.Count; i++)
			{
				Sprite sprite = sprites[i];
				Rect outer = sprite.outer;
				Rect inner = sprite.inner;
				if (mCoordinates == Coordinates.TexCoords)
				{
					NGUIMath.ConvertToPixels(outer, width, height, true);
					NGUIMath.ConvertToPixels(inner, width, height, true);
				}
				UISpriteData uISpriteData = new UISpriteData();
				uISpriteData.name = sprite.name;
				uISpriteData.x = Mathf.RoundToInt(outer.xMin);
				uISpriteData.y = Mathf.RoundToInt(outer.yMin);
				uISpriteData.width = Mathf.RoundToInt(outer.width);
				uISpriteData.height = Mathf.RoundToInt(outer.height);
				uISpriteData.paddingLeft = Mathf.RoundToInt(sprite.paddingLeft * outer.width);
				uISpriteData.paddingRight = Mathf.RoundToInt(sprite.paddingRight * outer.width);
				uISpriteData.paddingBottom = Mathf.RoundToInt(sprite.paddingBottom * outer.height);
				uISpriteData.paddingTop = Mathf.RoundToInt(sprite.paddingTop * outer.height);
				uISpriteData.borderLeft = Mathf.RoundToInt(inner.xMin - outer.xMin);
				uISpriteData.borderRight = Mathf.RoundToInt(outer.xMax - inner.xMax);
				uISpriteData.borderBottom = Mathf.RoundToInt(outer.yMax - inner.yMax);
				uISpriteData.borderTop = Mathf.RoundToInt(inner.yMin - outer.yMin);
				mSprites.Add(uISpriteData);
			}
			sprites.Clear();
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
#endif
			return true;
		}
		return false;
	}
}
