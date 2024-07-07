using UnityEngine;

public class mPlayerData : MonoBehaviour
{
	public UILabel PlayerNameLabel;

	public UILabel PlayerLevelLabel;

	public UIProgressBar PlayerXP;

	public UILabel MoneyLabel;

	public UILabel GoldLabel;

	public UITexture AvatarTexture;

	private void Start()
	{
		EventManager.AddListener("AccountUpdate", AccountUpdate);
		EventManager.AddListener("AvatarUpdate", AvatarUpdate);
		AccountUpdate();
		AvatarUpdate();
	}

	private void AccountUpdate()
	{
		if (string.IsNullOrEmpty(AccountManager.instance.Data.Clan.ToString()))
		{
			PlayerNameLabel.text = AccountManager.instance.Data.AccountName;
		}
		else
		{
			PlayerNameLabel.text = string.Concat(AccountManager.instance.Data.AccountName, " - ", AccountManager.instance.Data.Clan);
		}
		PlayerLevelLabel.text = Localization.Get("Level") + " - " + AccountManager.GetLevel();
		PlayerXP.value = (float)AccountManager.GetXP() / (float)AccountManager.GetMaxXP();
		GoldLabel.text = AccountManager.GetGold().ToString("n0");
		MoneyLabel.text = AccountManager.GetMoney().ToString("n0");
	}

	private void AvatarUpdate()
	{
		AvatarTexture.mainTexture = AccountManager.instance.Data.Avatar;
	}
}
