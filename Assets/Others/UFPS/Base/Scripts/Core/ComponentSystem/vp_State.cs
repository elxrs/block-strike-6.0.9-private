using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class vp_State
{
	public vp_StateManager StateManager;

	public string TypeName;

	public string Name;

	public TextAsset TextAsset;

	public vp_ComponentPreset Preset;

	public List<int> StatesToBlock;

	[NonSerialized]
	protected bool m_Enabled;

	[NonSerialized]
	protected List<vp_State> m_CurrentlyBlockedBy;

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			m_Enabled = value;
			if (Application.isPlaying && StateManager != null)
			{
				if (m_Enabled)
				{
					StateManager.ImposeBlockingList(this);
				}
				else
				{
					StateManager.RelaxBlockingList(this);
				}
			}
		}
	}

	public bool Blocked
	{
		get
		{
			return CurrentlyBlockedBy.Count > 0;
		}
	}

	public int BlockCount
	{
		get
		{
			return CurrentlyBlockedBy.Count;
		}
	}

	protected List<vp_State> CurrentlyBlockedBy
	{
		get
		{
			if (m_CurrentlyBlockedBy == null)
			{
				m_CurrentlyBlockedBy = new List<vp_State>();
			}
			return m_CurrentlyBlockedBy;
		}
	}

	public vp_State(string typeName, string name = "Untitled", string path = null, TextAsset asset = null)
	{
		TypeName = typeName;
		Name = name;
		TextAsset = asset;
	}

	public void AddBlocker(vp_State blocker)
	{
		if (!CurrentlyBlockedBy.Contains(blocker))
		{
			CurrentlyBlockedBy.Add(blocker);
		}
	}

	public void RemoveBlocker(vp_State blocker)
	{
		if (CurrentlyBlockedBy.Contains(blocker))
		{
			CurrentlyBlockedBy.Remove(blocker);
		}
	}
}
