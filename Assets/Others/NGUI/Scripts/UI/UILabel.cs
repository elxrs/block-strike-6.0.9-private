﻿using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Label")]
[ExecuteInEditMode]
public class UILabel : UIWidget
{
	[DoNotObfuscateNGUI]
	public enum Effect
	{
		None,
		Shadow,
		Outline,
		Outline8
	}

	[DoNotObfuscateNGUI]
	public enum Overflow
	{
		ShrinkContent,
		ClampContent,
		ResizeFreely,
		ResizeHeight
	}

	[DoNotObfuscateNGUI]
	public enum Crispness
	{
		Never,
		OnDesktop,
		Always
	}

	[DoNotObfuscateNGUI]
	public enum Modifier
	{
		None = 0,
		ToUppercase = 1,
		ToLowercase = 2,
		Custom = 255
	}

	public delegate string ModifierFunc(string s);

	public Crispness keepCrispWhenShrunk = Crispness.Always;

	[HideInInspector]
	[SerializeField]
	private Font mTrueTypeFont;

	[HideInInspector]
	[SerializeField]
	private UIFont mFont;

	[Multiline(6)]
	[SerializeField]
	[HideInInspector]
	private string mText = string.Empty;

	[SerializeField]
	[HideInInspector]
	private int mFontSize = 16;

	[HideInInspector]
	[SerializeField]
	private FontStyle mFontStyle;

	[SerializeField]
	[HideInInspector]
	private NGUIText.Alignment mAlignment;

	[HideInInspector]
	[SerializeField]
	private bool mEncoding;

	[SerializeField]
	[HideInInspector]
	private int mMaxLineCount = 1;

	[HideInInspector]
	[SerializeField]
	private Effect mEffectStyle;

	[HideInInspector]
	[SerializeField]
	private Color mEffectColor = Color.black;

	[HideInInspector]
	[SerializeField]
	private NGUIText.SymbolStyle mSymbols = NGUIText.SymbolStyle.Normal;

	[HideInInspector]
	[SerializeField]
	private Vector2 mEffectDistance = Vector2.one;

	[SerializeField]
	[HideInInspector]
	private Overflow mOverflow = Overflow.ResizeHeight;

	[HideInInspector]
	[SerializeField]
	private bool mApplyGradient;

	[SerializeField]
	[HideInInspector]
	private Color mGradientTop = Color.white;

	[SerializeField]
	[HideInInspector]
	private Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);

	[SerializeField]
	[HideInInspector]
	private int mSpacingX;

	[SerializeField]
	[HideInInspector]
	private int mSpacingY;

	[HideInInspector]
	[SerializeField]
	private bool mUseFloatSpacing;

	[SerializeField]
	[HideInInspector]
	private float mFloatSpacingX;

	[SerializeField]
	[HideInInspector]
	private float mFloatSpacingY;

	[HideInInspector]
	[SerializeField]
	private bool mOverflowEllipsis;

	[SerializeField]
	[HideInInspector]
	private int mOverflowWidth;

	[HideInInspector]
	[SerializeField]
	private int mOverflowHeight;

	[SerializeField]
	[HideInInspector]
	private Modifier mModifier;

	[SerializeField]
	[HideInInspector]
	private bool mShrinkToFit;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineWidth;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineHeight;

	[HideInInspector]
	[SerializeField]
	private float mLineWidth;

	[SerializeField]
	[HideInInspector]
	private bool mMultiline = true;

	private Font mActiveTTF;

	private float mDensity = 1f;

	private bool mShouldBeProcessed = true;

	private string mProcessedText;

	private bool mPremultiply;

	private Vector2 mCalculatedSize = Vector2.zero;

	private float mScale = 1f;

	private int mFinalFontSize;

	private int mLastWidth;

	private int mLastHeight;

	public ModifierFunc customModifier;

	private static BetterList<UILabel> mList = new BetterList<UILabel>();

	private static Dictionary<Font, int> mFontUsage = new Dictionary<Font, int>();

	[NonSerialized]
	private static BetterList<UIDrawCall> mTempDrawcalls;

	private static bool mTexRebuildAdded = false;

	private static List<Vector3> mTempVerts = new List<Vector3>();

	private static List<int> mTempIndices = new List<int>();

	public int finalFontSize
	{
		get
		{
			if ((bool)trueTypeFont)
			{
				return Mathf.RoundToInt(mScale * (float)mFinalFontSize);
			}
			return Mathf.RoundToInt((float)mFontSize * mScale);
		}
	}

	private bool shouldBeProcessed
	{
		get
		{
			return mShouldBeProcessed;
		}
		set
		{
			if (value)
			{
				mChanged = true;
				mShouldBeProcessed = true;
			}
			else
			{
				mShouldBeProcessed = false;
			}
		}
	}

	public override bool isAnchoredHorizontally
	{
		get
		{
			return base.isAnchoredHorizontally || mOverflow == Overflow.ResizeFreely;
		}
	}

	public override bool isAnchoredVertically
	{
		get
		{
			return base.isAnchoredVertically || mOverflow == Overflow.ResizeFreely || mOverflow == Overflow.ResizeHeight;
		}
	}

	public override Material material
	{
		get
		{
			if (mMat != null)
			{
				return mMat;
			}
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				return iNGUIFont.material;
			}
			if (mTrueTypeFont != null)
			{
				return mTrueTypeFont.material;
			}
			return null;
		}
		set
		{
			base.material = value;
		}
	}

	public override Texture mainTexture
	{
		get
		{
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				return iNGUIFont.texture;
			}
			if (mTrueTypeFont != null)
			{
				Material material = mTrueTypeFont.material;
				if (material != null)
				{
					return material.mainTexture;
				}
			}
			return null;
		}
		set
		{
			base.mainTexture = value;
		}
	}

	[Obsolete("Use UILabel.bitmapFont instead")]
	public UnityEngine.Object font
	{
		get
		{
			return bitmapFont as UnityEngine.Object;
		}
		set
		{
			bitmapFont = value as INGUIFont;
		}
	}

	public INGUIFont bitmapFont
	{
		get
		{
			return mFont as INGUIFont;
		}
		set
		{
			if (mFont as INGUIFont != value)
			{
				RemoveFromPanel();
				mFont = (UIFont)value;
				mTrueTypeFont = null;
				MarkAsChanged();
			}
		}
	}

	public INGUIAtlas atlas
	{
		get
		{
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				return iNGUIFont.atlas;
			}
			return null;
		}
		set
		{
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				iNGUIFont.atlas = value;
			}
		}
	}

	public Font trueTypeFont
	{
		get
		{
			if (mTrueTypeFont != null)
			{
				return mTrueTypeFont;
			}
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				return iNGUIFont.dynamicFont;
			}
			return null;
		}
		set
		{
			if (mTrueTypeFont != value)
			{
				SetActiveFont(null);
				RemoveFromPanel();
				mTrueTypeFont = value;
				shouldBeProcessed = true;
				mFont = null;
				SetActiveFont(value);
				ProcessAndRequest();
				if (mActiveTTF != null)
				{
					base.MarkAsChanged();
				}
				if (widgetAreStatic)
				{
					UpdateWidget();
				}
			}
		}
	}

	public UnityEngine.Object ambigiousFont
	{
		get
		{
			return (UnityEngine.Object)mFont ?? (UnityEngine.Object)mTrueTypeFont;
		}
		set
		{
			INGUIFont iNGUIFont = value as INGUIFont;
			if (iNGUIFont != null)
			{
				bitmapFont = iNGUIFont;
			}
			else
			{
				trueTypeFont = value as Font;
			}
		}
	}

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (mText == value)
			{
				return;
			}
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mText))
				{
					mText = string.Empty;
					MarkAsChanged();
					ProcessAndRequest();
					if (autoResizeBoxCollider)
					{
						ResizeCollider();
					}
					if (widgetAreStatic)
					{
						UpdateWidget();
					}
				}
			}
			else if (mText != value)
			{
				mText = value;
				MarkAsChanged();
				ProcessAndRequest();
				if (autoResizeBoxCollider)
				{
					ResizeCollider();
				}
				if (widgetAreStatic)
				{
					UpdateWidget();
				}
			}
		}
	}

	public int defaultFontSize
	{
		get
		{
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				return iNGUIFont.defaultSize;
			}
			if (trueTypeFont != null)
			{
				return mFontSize;
			}
			return 16;
		}
	}

	public int fontSize
	{
		get
		{
			return mFontSize;
		}
		set
		{
			value = Mathf.Clamp(value, 0, 256);
			if (mFontSize != value)
			{
				mFontSize = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public FontStyle fontStyle
	{
		get
		{
			return mFontStyle;
		}
		set
		{
			if (mFontStyle != value)
			{
				mFontStyle = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public NGUIText.Alignment alignment
	{
		get
		{
			return mAlignment;
		}
		set
		{
			if (mAlignment != value)
			{
				mAlignment = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public bool applyGradient
	{
		get
		{
			return mApplyGradient;
		}
		set
		{
			if (mApplyGradient != value)
			{
				mApplyGradient = value;
				MarkAsChanged();
			}
		}
	}

	public Color gradientTop
	{
		get
		{
			return mGradientTop;
		}
		set
		{
			if (mGradientTop != value)
			{
				mGradientTop = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public Color gradientBottom
	{
		get
		{
			return mGradientBottom;
		}
		set
		{
			if (mGradientBottom != value)
			{
				mGradientBottom = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public int spacingX
	{
		get
		{
			return mSpacingX;
		}
		set
		{
			if (mSpacingX != value)
			{
				mSpacingX = value;
				MarkAsChanged();
			}
		}
	}

	public int spacingY
	{
		get
		{
			return mSpacingY;
		}
		set
		{
			if (mSpacingY != value)
			{
				mSpacingY = value;
				MarkAsChanged();
			}
		}
	}

	public bool useFloatSpacing
	{
		get
		{
			return mUseFloatSpacing;
		}
		set
		{
			if (mUseFloatSpacing != value)
			{
				mUseFloatSpacing = value;
				shouldBeProcessed = true;
			}
		}
	}

	public float floatSpacingX
	{
		get
		{
			return mFloatSpacingX;
		}
		set
		{
			if (!Mathf.Approximately(mFloatSpacingX, value))
			{
				mFloatSpacingX = value;
				MarkAsChanged();
			}
		}
	}

	public float floatSpacingY
	{
		get
		{
			return mFloatSpacingY;
		}
		set
		{
			if (!Mathf.Approximately(mFloatSpacingY, value))
			{
				mFloatSpacingY = value;
				MarkAsChanged();
			}
		}
	}

	public float effectiveSpacingY
	{
		get
		{
			return (!mUseFloatSpacing) ? ((float)mSpacingY) : mFloatSpacingY;
		}
	}

	public float effectiveSpacingX
	{
		get
		{
			return (!mUseFloatSpacing) ? ((float)mSpacingX) : mFloatSpacingX;
		}
	}

	public bool overflowEllipsis
	{
		get
		{
			return mOverflowEllipsis;
		}
		set
		{
			if (mOverflowEllipsis != value)
			{
				mOverflowEllipsis = value;
				MarkAsChanged();
			}
		}
	}

	public int overflowWidth
	{
		get
		{
			return mOverflowWidth;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (mOverflowWidth != value)
			{
				mOverflowWidth = value;
				MarkAsChanged();
			}
		}
	}

	public int overflowHeight
	{
		get
		{
			return mOverflowHeight;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (mOverflowHeight != value)
			{
				mOverflowHeight = value;
				MarkAsChanged();
			}
		}
	}

	private bool keepCrisp
	{
		get
		{
			if (trueTypeFont != null && keepCrispWhenShrunk != 0)
			{
				return keepCrispWhenShrunk == Crispness.Always;
			}
			return false;
		}
	}

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				shouldBeProcessed = true;
			}
		}
	}

	public NGUIText.SymbolStyle symbolStyle
	{
		get
		{
			return mSymbols;
		}
		set
		{
			if (mSymbols != value)
			{
				mSymbols = value;
				shouldBeProcessed = true;
			}
		}
	}

	public Overflow overflowMethod
	{
		get
		{
			return mOverflow;
		}
		set
		{
			if (mOverflow != value)
			{
				mOverflow = value;
				shouldBeProcessed = true;
			}
		}
	}

	[Obsolete("Use 'width' instead")]
	public int lineWidth
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	[Obsolete("Use 'height' instead")]
	public int lineHeight
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	public bool multiLine
	{
		get
		{
			return mMaxLineCount != 1;
		}
		set
		{
			if (mMaxLineCount != 1 != value)
			{
				mMaxLineCount = ((!value) ? 1 : 0);
				shouldBeProcessed = true;
			}
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText(false, true);
			}
			return base.localCorners;
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText(false, true);
			}
			return base.worldCorners;
		}
	}

	public override Vector4 drawingDimensions
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText(false, true);
			}
			return base.drawingDimensions;
		}
	}

	public int maxLineCount
	{
		get
		{
			return mMaxLineCount;
		}
		set
		{
			if (mMaxLineCount != value)
			{
				mMaxLineCount = Mathf.Max(value, 0);
				shouldBeProcessed = true;
				if (overflowMethod == Overflow.ShrinkContent)
				{
					MakePixelPerfect();
				}
			}
		}
	}

	public Effect effectStyle
	{
		get
		{
			return mEffectStyle;
		}
		set
		{
			if (mEffectStyle != value)
			{
				mEffectStyle = value;
				shouldBeProcessed = true;
			}
		}
	}

	public Color effectColor
	{
		get
		{
			return mEffectColor;
		}
		set
		{
			if (mEffectColor != value)
			{
				mEffectColor = value;
				if (mEffectStyle != 0)
				{
					shouldBeProcessed = true;
				}
			}
		}
	}

	public Vector2 effectDistance
	{
		get
		{
			return mEffectDistance;
		}
		set
		{
			if (mEffectDistance != value)
			{
				mEffectDistance = value;
				shouldBeProcessed = true;
			}
		}
	}

	public int quadsPerCharacter
	{
		get
		{
			if (mEffectStyle == Effect.Shadow)
			{
				return 2;
			}
			if (mEffectStyle == Effect.Outline)
			{
				return 5;
			}
			if (mEffectStyle == Effect.Outline8)
			{
				return 9;
			}
			return 1;
		}
	}

	[Obsolete("Use 'overflowMethod == UILabel.Overflow.ShrinkContent' instead")]
	public bool shrinkToFit
	{
		get
		{
			return mOverflow == Overflow.ShrinkContent;
		}
		set
		{
			if (value)
			{
				overflowMethod = Overflow.ShrinkContent;
			}
		}
	}

	public string processedText
	{
		get
		{
			if (mLastWidth != mWidth || mLastHeight != mHeight)
			{
				mLastWidth = mWidth;
				mLastHeight = mHeight;
				mShouldBeProcessed = true;
			}
			if (shouldBeProcessed)
			{
				ProcessText(false, true);
			}
			return mProcessedText;
		}
	}

	public Vector2 printedSize
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText(false, true);
			}
			return mCalculatedSize;
		}
	}

	public override Vector2 localSize
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText(false, true);
			}
			return base.localSize;
		}
	}

	private bool isValid
	{
		get
		{
			return mFont != null || mTrueTypeFont != null;
		}
	}

	public Modifier modifier
	{
		get
		{
			return mModifier;
		}
		set
		{
			if (mModifier != value)
			{
				mModifier = value;
				MarkAsChanged();
				ProcessAndRequest();
			}
		}
	}

	public string printedText
	{
		get
		{
			if (!string.IsNullOrEmpty(mText))
			{
				if (mModifier == Modifier.None)
				{
					return mText;
				}
				if (mModifier == Modifier.ToLowercase)
				{
					return mText.ToLower();
				}
				if (mModifier == Modifier.ToUppercase)
				{
					return mText.ToUpper();
				}
				if (mModifier == Modifier.Custom && customModifier != null)
				{
					return customModifier(mText);
				}
			}
			return mText;
		}
	}

	private bool premultipliedAlphaShader
	{
		get
		{
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				return iNGUIFont.premultipliedAlphaShader;
			}
			return false;
		}
	}

	private bool packedFontShader
	{
		get
		{
			INGUIFont iNGUIFont = bitmapFont;
			if (iNGUIFont != null)
			{
				return iNGUIFont.packedFontShader;
			}
			return false;
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		mList.Add(this);
		SetActiveFont(trueTypeFont);
	}

	protected override void OnDisable()
	{
		SetActiveFont(null);
		mList.Remove(this);
		base.OnDisable();
	}

	protected void SetActiveFont(Font fnt)
	{
		if (!(mActiveTTF != fnt))
		{
			return;
		}
		Font font = mActiveTTF;
		int value;
		if (font != null && mFontUsage.TryGetValue(font, out value))
		{
			value = Mathf.Max(0, --value);
			if (value == 0)
			{
				mFontUsage.Remove(font);
			}
			else
			{
				mFontUsage[font] = value;
			}
		}
		mActiveTTF = fnt;
		font = fnt;
		if (font != null)
		{
			int num = 0;
			num = (mFontUsage[font] = num + 1);
		}
	}

	private static void OnFontChanged(Font font)
	{
		for (int i = 0; i < mList.size; i++)
		{
			UILabel uILabel = mList.buffer[i];
			if (!(uILabel != null))
			{
				continue;
			}
			Font font2 = uILabel.trueTypeFont;
			if (font2 == font)
			{
				font2.RequestCharactersInTexture(uILabel.mText, uILabel.mFinalFontSize, uILabel.mFontStyle);
				uILabel.MarkAsChanged();
				if (uILabel.panel == null)
				{
					uILabel.CreatePanel();
				}
				if (mTempDrawcalls == null)
				{
					mTempDrawcalls = new BetterList<UIDrawCall>();
				}
				if (uILabel.drawCall != null && !mTempDrawcalls.Contains(uILabel.drawCall))
				{
					mTempDrawcalls.Add(uILabel.drawCall);
				}
			}
		}
		if (mTempDrawcalls != null)
		{
			int j = 0;
			for (int size = mTempDrawcalls.size; j < size; j++)
			{
				UIDrawCall uIDrawCall = mTempDrawcalls.buffer[j];
				if (uIDrawCall.panel != null)
				{
					uIDrawCall.panel.FillDrawCall(uIDrawCall);
				}
			}
			mTempDrawcalls.Clear();
		}
		TimerManager.In(0.1f, delegate
		{
			string empty = string.Empty;
			for (int k = 0; k < mList.size; k++)
			{
				UILabel uILabel2 = mList.buffer[k];
				if (uILabel2 != null)
				{
					empty = uILabel2.text;
					uILabel2.text = "1#2";
					uILabel2.text = empty;
				}
			}
		});
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		if (shouldBeProcessed)
		{
			ProcessText(false, true);
		}
		return base.GetSides(relativeTo);
	}

	protected override void UpgradeFrom265()
	{
		ProcessText(true);
		if (mShrinkToFit)
		{
			overflowMethod = Overflow.ShrinkContent;
			mMaxLineCount = 0;
		}
		if (mMaxLineWidth != 0)
		{
			width = mMaxLineWidth;
			overflowMethod = ((mMaxLineCount > 0) ? Overflow.ResizeHeight : Overflow.ShrinkContent);
		}
		else
		{
			overflowMethod = Overflow.ResizeFreely;
		}
		if (mMaxLineHeight != 0)
		{
			height = mMaxLineHeight;
		}
		if (mFont != null)
		{
			int num = defaultFontSize;
			if (height < num)
			{
				height = num;
			}
			fontSize = num;
		}
		mMaxLineWidth = 0;
		mMaxLineHeight = 0;
		mShrinkToFit = false;
		NGUITools.UpdateWidgetCollider(gameObject, true);
	}

	protected override void OnAnchor()
	{
		if (mOverflow == Overflow.ResizeFreely)
		{
			if (isFullyAnchored)
			{
				mOverflow = Overflow.ShrinkContent;
			}
		}
		else if (mOverflow == Overflow.ResizeHeight && topAnchor.target != null && bottomAnchor.target != null)
		{
			mOverflow = Overflow.ShrinkContent;
		}
		base.OnAnchor();
	}

	private void ProcessAndRequest()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying && !NGUITools.GetActive(this)) return;
		if (!mAllowProcessing) return;
#endif
		if (ambigiousFont != null) ProcessText();
	}

#if UNITY_EDITOR
	// Used to ensure that we don't process font more than once inside OnValidate function below
	[System.NonSerialized] bool mAllowProcessing = true;
	[System.NonSerialized] bool mUsingTTF = true;

	/// <summary>
	/// Validate the properties.
	/// </summary>

	protected override void OnValidate()
	{
		base.OnValidate();

		if (NGUITools.GetActive(this))
		{
			Font ttf = mTrueTypeFont;
			UIFont fnt = mFont;

			// If the true type font was not used before, but now it is, clear the font reference
			if (!mUsingTTF && ttf != null) fnt = null;
			else if (mUsingTTF && fnt != null) ttf = null;

			mFont = null;
			mTrueTypeFont = null;
			mAllowProcessing = false;

#if DYNAMIC_FONT
			SetActiveFont(null);
#endif
			if (fnt != null)
			{
				bitmapFont = fnt;
				mUsingTTF = false;
			}
			else if (ttf != null)
			{
				trueTypeFont = ttf;
				mUsingTTF = true;
			}

			shouldBeProcessed = true;
			mAllowProcessing = true;
			ProcessAndRequest();
			if (autoResizeBoxCollider) ResizeCollider();
		}
	}
#endif

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!mTexRebuildAdded)
		{
			mTexRebuildAdded = true;
			Font.textureRebuilt += OnFontChanged;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (mLineWidth > 0f)
		{
			mMaxLineWidth = Mathf.RoundToInt(mLineWidth);
			mLineWidth = 0f;
		}
		if (!mMultiline)
		{
			mMaxLineCount = 1;
			mMultiline = true;
		}
		mPremultiply = material != null && material.shader != null && material.shader.name.Contains("Premultiplied");
		ProcessAndRequest();
	}

	public override void MarkAsChanged()
	{
		shouldBeProcessed = true;
		base.MarkAsChanged();
	}

	public void ProcessText(bool legacyMode = false, bool full = true)
	{
		if (!isValid)
		{
			return;
		}
		mChanged = true;
		shouldBeProcessed = false;
		float num = mDrawRegion.z - mDrawRegion.x;
		float num2 = mDrawRegion.w - mDrawRegion.y;
		NGUIText.rectWidth = ((!legacyMode) ? width : ((mMaxLineWidth == 0) ? 1000000 : mMaxLineWidth));
		NGUIText.rectHeight = ((!legacyMode) ? height : ((mMaxLineHeight == 0) ? 1000000 : mMaxLineHeight));
		NGUIText.regionWidth = ((num == 1f) ? NGUIText.rectWidth : Mathf.RoundToInt((float)NGUIText.rectWidth * num));
		NGUIText.regionHeight = ((num2 == 1f) ? NGUIText.rectHeight : Mathf.RoundToInt((float)NGUIText.rectHeight * num2));
		mFinalFontSize = Mathf.Abs((!legacyMode) ? defaultFontSize : Mathf.RoundToInt(cachedTransform.localScale.x));
		mScale = 1f;
		if (NGUIText.regionWidth < 1 || NGUIText.regionHeight < 0)
		{
			mProcessedText = string.Empty;
			return;
		}
		if (trueTypeFont != null && keepCrisp)
		{
			UIRoot uIRoot = root;
			if (uIRoot != null)
			{
				mDensity = ((!(uIRoot != null)) ? 1f : uIRoot.pixelSizeAdjustment);
			}
		}
		else
		{
			mDensity = 1f;
		}
		if (full)
		{
			UpdateNGUIText();
		}
		if (mOverflow == Overflow.ResizeFreely)
		{
			if (mOverflowWidth > 0)
			{
				NGUIText.rectWidth = mOverflowWidth;
				NGUIText.regionWidth = mOverflowWidth;
			}
			else
			{
				NGUIText.rectWidth = 1000000;
				NGUIText.regionWidth = 1000000;
			}
			if (mOverflowHeight > 0)
			{
				NGUIText.rectHeight = mOverflowHeight;
				NGUIText.regionHeight = mOverflowHeight;
			}
			else
			{
				NGUIText.rectHeight = 1000000;
				NGUIText.regionHeight = 1000000;
			}
		}
		else if (mOverflow == Overflow.ResizeFreely || mOverflow == Overflow.ResizeHeight)
		{
			NGUIText.rectHeight = 1000000;
			NGUIText.regionHeight = 1000000;
		}
		if (mFinalFontSize > 0)
		{
			bool flag = keepCrisp;
			int num3;
			for (num3 = mFinalFontSize; num3 > 0; num3--)
			{
				if (flag)
				{
					mFinalFontSize = num3;
					NGUIText.fontSize = mFinalFontSize;
				}
				else
				{
					mScale = (float)num3 / (float)mFinalFontSize;
					INGUIFont iNGUIFont = bitmapFont;
					if (iNGUIFont != null)
					{
						NGUIText.fontScale = (float)mFontSize / (float)iNGUIFont.defaultSize * mScale;
					}
					else
					{
						NGUIText.fontScale = mScale;
					}
				}
				NGUIText.Update(false);
				bool flag2 = NGUIText.WrapText(printedText, out mProcessedText, false, false, mOverflow == Overflow.ClampContent && mOverflowEllipsis);
				if (mOverflow == Overflow.ShrinkContent && !flag2)
				{
					if (--num3 > 1)
					{
						continue;
					}
					break;
				}
				if (mOverflow == Overflow.ResizeFreely)
				{
					mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
					if (!flag2 && mOverflowWidth > 0)
					{
						if (--num3 > 1)
						{
							continue;
						}
						break;
					}
					int num4 = Mathf.Max(minWidth, Mathf.RoundToInt(mCalculatedSize.x));
					if (num != 1f)
					{
						num4 = Mathf.RoundToInt((float)num4 / num);
					}
					int num5 = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
					if (num2 != 1f)
					{
						num5 = Mathf.RoundToInt((float)num5 / num2);
					}
					if ((num4 & 1) == 1)
					{
						num4++;
					}
					if ((num5 & 1) == 1)
					{
						num5++;
					}
					if (mWidth != num4 || mHeight != num5)
					{
						mWidth = num4;
						mHeight = num5;
						if (onChange != null)
						{
							onChange();
						}
					}
				}
				else if (mOverflow == Overflow.ResizeHeight)
				{
					mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
					int num6 = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
					if (num2 != 1f)
					{
						num6 = Mathf.RoundToInt((float)num6 / num2);
					}
					if ((num6 & 1) == 1)
					{
						num6++;
					}
					if (mHeight != num6)
					{
						mHeight = num6;
						if (onChange != null)
						{
							onChange();
						}
					}
				}
				else
				{
					mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
				}
				if (legacyMode)
				{
					width = Mathf.RoundToInt(mCalculatedSize.x);
					height = Mathf.RoundToInt(mCalculatedSize.y);
					cachedTransform.localScale = Vector3.one;
				}
				break;
			}
		}
		else
		{
			cachedTransform.localScale = Vector3.one;
			mProcessedText = string.Empty;
			mScale = 1f;
		}
		if (full)
		{
			NGUIText.bitmapFont = null;
			NGUIText.dynamicFont = null;
		}
	}

	public override void MakePixelPerfect()
	{
		if (ambigiousFont != null)
		{
			Vector3 localPosition = cachedTransform.localPosition;
			localPosition.x = Mathf.RoundToInt(localPosition.x);
			localPosition.y = Mathf.RoundToInt(localPosition.y);
			localPosition.z = Mathf.RoundToInt(localPosition.z);
			cachedTransform.localPosition = localPosition;
			cachedTransform.localScale = Vector3.one;
			if (mOverflow == Overflow.ResizeFreely)
			{
				AssumeNaturalSize();
				return;
			}
			int a = width;
			int a2 = height;
			Overflow overflow = mOverflow;
			if (overflow != Overflow.ResizeHeight)
			{
				mWidth = 100000;
			}
			mHeight = 100000;
			mOverflow = Overflow.ShrinkContent;
			ProcessText(false, true);
			mOverflow = overflow;
			int a3 = Mathf.RoundToInt(mCalculatedSize.x);
			int a4 = Mathf.RoundToInt(mCalculatedSize.y);
			a3 = Mathf.Max(a3, base.minWidth);
			a4 = Mathf.Max(a4, base.minHeight);
			if ((a3 & 1) == 1)
			{
				a3++;
			}
			if ((a4 & 1) == 1)
			{
				a4++;
			}
			mWidth = Mathf.Max(a, a3);
			mHeight = Mathf.Max(a2, a4);
			MarkAsChanged();
		}
		else
		{
			base.MakePixelPerfect();
		}
	}

	public void AssumeNaturalSize()
	{
		if (ambigiousFont != null)
		{
			mWidth = 100000;
			mHeight = 100000;
			ProcessText(false, true);
			mWidth = Mathf.RoundToInt(mCalculatedSize.x);
			mHeight = Mathf.RoundToInt(mCalculatedSize.y);
			if ((mWidth & 1) == 1)
			{
				mWidth++;
			}
			if ((mHeight & 1) == 1)
			{
				mHeight++;
			}
			MarkAsChanged();
		}
	}

	[Obsolete("Use UILabel.GetCharacterAtPosition instead")]
	public int GetCharacterIndex(Vector3 worldPos)
	{
		return GetCharacterIndexAtPosition(worldPos, false);
	}

	[Obsolete("Use UILabel.GetCharacterAtPosition instead")]
	public int GetCharacterIndex(Vector2 localPos)
	{
		return GetCharacterIndexAtPosition(localPos, false);
	}

	public int GetCharacterIndexAtPosition(Vector3 worldPos, bool precise)
	{
		Vector2 localPos = cachedTransform.InverseTransformPoint(worldPos);
		return GetCharacterIndexAtPosition(localPos, precise);
	}

	public int GetCharacterIndexAtPosition(Vector2 localPos, bool precise)
	{
		if (isValid)
		{
			string value = processedText;
			if (string.IsNullOrEmpty(value))
			{
				return 0;
			}
			UpdateNGUIText();
			if (precise)
			{
				NGUIText.PrintExactCharacterPositions(value, mTempVerts, mTempIndices);
			}
			else
			{
				NGUIText.PrintApproximateCharacterPositions(value, mTempVerts, mTempIndices);
			}
			if (mTempVerts.Count > 0)
			{
				ApplyOffset(mTempVerts, 0);
				int result = ((!precise) ? NGUIText.GetApproximateCharacterIndex(mTempVerts, mTempIndices, localPos) : NGUIText.GetExactCharacterIndex(mTempVerts, mTempIndices, localPos));
				mTempVerts.Clear();
				mTempIndices.Clear();
				NGUIText.bitmapFont = null;
				NGUIText.dynamicFont = null;
				return result;
			}
			NGUIText.bitmapFont = null;
			NGUIText.dynamicFont = null;
		}
		return 0;
	}

	public string GetWordAtPosition(Vector3 worldPos)
	{
		int characterIndexAtPosition = GetCharacterIndexAtPosition(worldPos, true);
		return GetWordAtCharacterIndex(characterIndexAtPosition);
	}

	public string GetWordAtPosition(Vector2 localPos)
	{
		int characterIndexAtPosition = GetCharacterIndexAtPosition(localPos, true);
		return GetWordAtCharacterIndex(characterIndexAtPosition);
	}

	public string GetWordAtCharacterIndex(int characterIndex)
	{
		string text = printedText;
		if (characterIndex != -1 && characterIndex < text.Length)
		{
			int num = text.LastIndexOfAny(new char[2] { ' ', '\n' }, characterIndex) + 1;
			int num2 = text.IndexOfAny(new char[4] { ' ', '\n', ',', '.' }, characterIndex);
			if (num2 == -1)
			{
				num2 = text.Length;
			}
			if (num != num2)
			{
				int num3 = num2 - num;
				if (num3 > 0)
				{
					string text2 = text.Substring(num, num3);
					return NGUIText.StripSymbols(text2);
				}
			}
		}
		return null;
	}

	public string GetUrlAtPosition(Vector3 worldPos)
	{
		return GetUrlAtCharacterIndex(GetCharacterIndexAtPosition(worldPos, true));
	}

	public string GetUrlAtPosition(Vector2 localPos)
	{
		return GetUrlAtCharacterIndex(GetCharacterIndexAtPosition(localPos, true));
	}

	public string GetUrlAtCharacterIndex(int characterIndex)
	{
		string text = printedText;
		if (characterIndex != -1 && characterIndex < text.Length - 6)
		{
			int num = ((text[characterIndex] != '[' || text[characterIndex + 1] != 'u' || text[characterIndex + 2] != 'r' || text[characterIndex + 3] != 'l' || text[characterIndex + 4] != '=') ? text.LastIndexOf("[url=", characterIndex) : characterIndex);
			if (num == -1)
			{
				return null;
			}
			num += 5;
			int num2 = text.IndexOf("]", num);
			if (num2 == -1)
			{
				return null;
			}
			int num3 = text.IndexOf("[/url]", num2);
			if (num3 == -1 || characterIndex <= num3)
			{
				return text.Substring(num, num2 - num);
			}
		}
		return null;
	}

	public int GetCharacterIndex(int currentIndex, KeyCode key)
	{
		if (isValid)
		{
			string text = processedText;
			if (string.IsNullOrEmpty(text))
			{
				return 0;
			}
			int num = defaultFontSize;
			UpdateNGUIText();
			NGUIText.PrintApproximateCharacterPositions(text, mTempVerts, mTempIndices);
			if (mTempVerts.Count > 0)
			{
				ApplyOffset(mTempVerts, 0);
				int i = 0;
				for (int count = mTempIndices.Count; i < count; i++)
				{
					if (mTempIndices[i] == currentIndex)
					{
						Vector2 pos = mTempVerts[i];
						switch (key)
						{
							case KeyCode.UpArrow:
								pos.y += (float)num + effectiveSpacingY;
								break;
							case KeyCode.DownArrow:
								pos.y -= (float)num + effectiveSpacingY;
								break;
							case KeyCode.Home:
								pos.x -= 1000f;
								break;
							case KeyCode.End:
								pos.x += 1000f;
								break;
						}
						int approximateCharacterIndex = NGUIText.GetApproximateCharacterIndex(mTempVerts, mTempIndices, pos);
						if (approximateCharacterIndex == currentIndex)
						{
							break;
						}
						mTempVerts.Clear();
						mTempIndices.Clear();
						return approximateCharacterIndex;
					}
				}
				mTempVerts.Clear();
				mTempIndices.Clear();
			}
			NGUIText.bitmapFont = null;
			NGUIText.dynamicFont = null;
			switch (key)
			{
				case KeyCode.UpArrow:
				case KeyCode.Home:
					return 0;
				case KeyCode.DownArrow:
				case KeyCode.End:
					return text.Length;
			}
		}
		return currentIndex;
	}

	public void PrintOverlay(int start, int end, UIGeometry caret, UIGeometry highlight, Color caretColor, Color highlightColor)
	{
		if (caret != null)
		{
			caret.Clear();
		}
		if (highlight != null)
		{
			highlight.Clear();
		}
		if (!isValid)
		{
			return;
		}
		string text = processedText;
		UpdateNGUIText();
		int count = caret.verts.Count;
		Vector2 item = new Vector2(0.5f, 0.5f);
		float num = finalAlpha;
		if (highlight != null && start != end)
		{
			int count2 = highlight.verts.Count;
			NGUIText.PrintCaretAndSelection(text, start, end, caret.verts, highlight.verts);
			if (highlight.verts.Count > count2)
			{
				ApplyOffset(highlight.verts, count2);
				Color item2 = new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightColor.a * num);
				int i = count2;
				for (int count3 = highlight.verts.Count; i < count3; i++)
				{
					highlight.uvs.Add(item);
					highlight.cols.Add(item2);
				}
			}
		}
		else
		{
			NGUIText.PrintCaretAndSelection(text, start, end, caret.verts, null);
		}
		ApplyOffset(caret.verts, count);
		Color item3 = new Color(caretColor.r, caretColor.g, caretColor.b, caretColor.a * num);
		int j = count;
		for (int count4 = caret.verts.Count; j < count4; j++)
		{
			caret.uvs.Add(item);
			caret.cols.Add(item3);
		}
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
	}

	public override void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		if (!isValid)
		{
			return;
		}
		int num = verts.Count;
		Color color = base.color;
		color.a = finalAlpha;
		if (premultipliedAlphaShader)
		{
			color = NGUITools.ApplyPMA(color);
		}
		string text = processedText;
		int count = verts.Count;
		UpdateNGUIText();
		NGUIText.tint = color;
		NGUIText.Print(text, verts, uvs, cols);
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
		Vector2 vector = ApplyOffset(verts, count);
		if (packedFontShader)
		{
			return;
		}
		if (effectStyle != 0)
		{
			int count2 = verts.Count;
			vector.x = mEffectDistance.x;
			vector.y = mEffectDistance.y;
			ApplyShadow(verts, uvs, cols, num, count2, vector.x, 0f - vector.y);
			if (effectStyle == Effect.Outline || effectStyle == Effect.Outline8)
			{
				num = count2;
				count2 = verts.Count;
				ApplyShadow(verts, uvs, cols, num, count2, 0f - vector.x, vector.y);
				num = count2;
				count2 = verts.Count;
				ApplyShadow(verts, uvs, cols, num, count2, vector.x, vector.y);
				num = count2;
				count2 = verts.Count;
				ApplyShadow(verts, uvs, cols, num, count2, 0f - vector.x, 0f - vector.y);
				if (effectStyle == Effect.Outline8)
				{
					num = count2;
					count2 = verts.Count;
					ApplyShadow(verts, uvs, cols, num, count2, 0f - vector.x, 0f);
					num = count2;
					count2 = verts.Count;
					ApplyShadow(verts, uvs, cols, num, count2, vector.x, 0f);
					num = count2;
					count2 = verts.Count;
					ApplyShadow(verts, uvs, cols, num, count2, 0f, vector.y);
					num = count2;
					count2 = verts.Count;
					ApplyShadow(verts, uvs, cols, num, count2, 0f, 0f - vector.y);
				}
			}
		}
		if (NGUIText.symbolStyle == NGUIText.SymbolStyle.NoOutline)
		{
			int i = 0;
			for (int count3 = cols.Count; i < count3; i++)
			{
				if (cols[i].r == -1f)
				{
					cols[i] = Color.white;
				}
			}
		}
		if (onPostFill != null)
		{
			onPostFill(this, num, verts, uvs, cols);
		}
	}

	public Vector2 ApplyOffset(List<Vector3> verts, int start)
	{
		Vector2 vector = pivotOffset;
		float f = Mathf.Lerp(0f, -mWidth, vector.x);
		float f2 = Mathf.Lerp(mHeight, 0f, vector.y) + Mathf.Lerp(mCalculatedSize.y - (float)mHeight, 0f, vector.y);
		f = Mathf.Round(f);
		f2 = Mathf.Round(f2);
		int i = start;
		for (int count = verts.Count; i < count; i++)
		{
			Vector3 value = verts[i];
			value.x += f;
			value.y += f2;
			verts[i] = value;
		}
		return new Vector2(f, f2);
	}

	public void ApplyShadow(List<Vector3> verts, List<Vector2> uvs, List<Color> cols, int start, int end, float x, float y)
	{
		Color color = mEffectColor;
		color.a *= finalAlpha;
		if (premultipliedAlphaShader)
		{
			color = NGUITools.ApplyPMA(color);
		}
		Color value = color;
		for (int i = start; i < end; i++)
		{
			verts.Add(verts[i]);
			uvs.Add(uvs[i]);
			cols.Add(cols[i]);
			Vector3 value2 = verts[i];
			value2.x += x;
			value2.y += y;
			verts[i] = value2;
			Color color2 = cols[i];
			if (color2.a == 1f)
			{
				cols[i] = value;
				continue;
			}
			Color value3 = color;
			value3.a = color2.a * color.a;
			cols[i] = value3;
		}
	}

	public int CalculateOffsetToFit(string text)
	{
		UpdateNGUIText();
		NGUIText.encoding = false;
		NGUIText.symbolStyle = NGUIText.SymbolStyle.None;
		int result = NGUIText.CalculateOffsetToFit(text);
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
		return result;
	}

	public void SetCurrentProgress()
	{
		if (UIProgressBar.current != null)
		{
			text = UIProgressBar.current.value.ToString("F");
		}
	}

	public void SetCurrentPercent()
	{
		if (UIProgressBar.current != null)
		{
			text = Mathf.RoundToInt(UIProgressBar.current.value * 100f) + "%";
		}
	}

	public void SetCurrentSelection()
	{
		if (UIPopupList.current != null)
		{
			text = ((!UIPopupList.current.isLocalized) ? UIPopupList.current.value : Localization.Get(UIPopupList.current.value));
		}
	}

	public bool Wrap(string text, out string final)
	{
		return Wrap(text, out final, 1000000);
	}

	public bool Wrap(string text, out string final, int height)
	{
		UpdateNGUIText();
		NGUIText.rectHeight = height;
		NGUIText.regionHeight = height;
		bool result = NGUIText.WrapText(text, out final);
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
		return result;
	}

	public void UpdateNGUIText()
	{
		Font font = trueTypeFont;
		bool flag = font != null;
		NGUIText.fontSize = mFinalFontSize;
		NGUIText.fontStyle = mFontStyle;
		NGUIText.rectWidth = mWidth;
		NGUIText.rectHeight = mHeight;
		NGUIText.regionWidth = Mathf.RoundToInt((float)mWidth * (mDrawRegion.z - mDrawRegion.x));
		NGUIText.regionHeight = Mathf.RoundToInt((float)mHeight * (mDrawRegion.w - mDrawRegion.y));
		NGUIText.gradient = mApplyGradient && !packedFontShader;
		NGUIText.gradientTop = mGradientTop;
		NGUIText.gradientBottom = mGradientBottom;
		NGUIText.encoding = mEncoding;
		NGUIText.premultiply = mPremultiply;
		NGUIText.symbolStyle = mSymbols;
		NGUIText.maxLines = mMaxLineCount;
		NGUIText.spacingX = effectiveSpacingX;
		NGUIText.spacingY = effectiveSpacingY;
		INGUIFont iNGUIFont = bitmapFont;
		if (flag)
		{
			NGUIText.fontScale = mScale;
		}
		else if (iNGUIFont != null)
		{
			NGUIText.fontScale = (float)mFontSize / (float)iNGUIFont.defaultSize * mScale;
		}
		else
		{
			NGUIText.fontScale = mScale;
		}
		if (iNGUIFont != null)
		{
			if (font != null)
			{
				NGUIText.dynamicFont = font;
				NGUIText.bitmapFont = null;
			}
			else
			{
				NGUIText.dynamicFont = null;
				NGUIText.bitmapFont = iNGUIFont;
			}
		}
		else
		{
			NGUIText.dynamicFont = font;
			NGUIText.bitmapFont = null;
		}
		if (flag && keepCrisp)
		{
			UIRoot uIRoot = root;
			if (uIRoot != null)
			{
				NGUIText.pixelDensity = ((!(uIRoot != null)) ? 1f : uIRoot.pixelSizeAdjustment);
			}
		}
		else
		{
			NGUIText.pixelDensity = 1f;
		}
		if (mDensity != NGUIText.pixelDensity)
		{
			ProcessText(false, false);
			NGUIText.rectWidth = mWidth;
			NGUIText.rectHeight = mHeight;
			NGUIText.regionWidth = Mathf.RoundToInt((float)mWidth * (mDrawRegion.z - mDrawRegion.x));
			NGUIText.regionHeight = Mathf.RoundToInt((float)mHeight * (mDrawRegion.w - mDrawRegion.y));
		}
		if (alignment == NGUIText.Alignment.Automatic)
		{
			switch (pivot)
			{
				case Pivot.TopLeft:
				case Pivot.Left:
				case Pivot.BottomLeft:
					NGUIText.alignment = NGUIText.Alignment.Left;
					break;
				case Pivot.TopRight:
				case Pivot.Right:
				case Pivot.BottomRight:
					NGUIText.alignment = NGUIText.Alignment.Right;
					break;
				default:
					NGUIText.alignment = NGUIText.Alignment.Center;
					break;
			}
		}
		else
		{
			NGUIText.alignment = alignment;
		}
		NGUIText.Update();
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused && mTrueTypeFont != null)
		{
			Invalidate(false);
		}
	}
}
