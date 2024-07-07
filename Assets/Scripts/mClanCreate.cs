using System;
using UnityEngine;
using FreeJSON;

[Serializable]
public class mClanCreate
{
	public GameObject panel;

	public UIInput tag;

	public UIInput name;

	public void OnSubmit()
	{
		UIInput current = UIInput.current;
		if (current == tag)
		{
			tag.value = mChangeName.UpdateSymbols(tag.value, true);
			if (tag.value.Length > 4 || tag.value.Length < 1)
			{
				tag.value = Utils.NameGenerator(3);
			}
		}
		if (current == name)
		{
			name.value = mChangeName.UpdateSymbols(name.value, true);
			if (name.value.Length > 15 || name.value.Length < 2)
			{
				name.value = Utils.NameGenerator(UnityEngine.Random.Range(5, 12));
			}
		}
	}

	public void CreateClan(Action complete, Action<string> error)
	{
		if (AccountManager.GetGold() < 250)
		{
			UIToast.Show(Localization.Get("Not enough money"));
			return;
		}
        if (tag.value.ToUpper() == "DEV")
        {
            return;
        }
        mPopUp.ShowText(Localization.Get("Please wait", true) + "...");
        Firebase check = new Firebase();
        check.Auth = AccountManager.AccountToken;
        check.Child("Clans").Child(tag.value.ToUpper()).GetValue(delegate (string result)
        {
            if (result == "null")
            {
                tag.value = mChangeName.UpdateSymbols(tag.value.ToUpper(), true);
                name.value = mChangeName.UpdateSymbols(name.value, true);
                if (tag.value.Length > 4 || tag.value.Length < 2)
                {
                    return;
                }
                if (name.value.Length > 12 || tag.value.Length < 2)
                {
                    return;
                }
                AccountManager.SetClan(tag.value.ToUpper());
                AccountManager.SetClanFirebase(tag.value.ToUpper());
                AccountManager.UpdateGold(AccountManager.GetGold() - 250, null, null);
                AccountManager.SetGold(AccountManager.GetGold() - 250);

                JsonObject players = new JsonObject();
                players.Add(AccountManager.instance.Data.ID.ToString(), 0);

                JsonObject jsonObject = new JsonObject();
                jsonObject.Add("a", AccountManager.instance.Data.ID.ToString());
                jsonObject.Add("n", name.value);
                jsonObject.Add("t", tag.value.ToUpper());
                jsonObject.Add("p", players);

                Firebase firebase = new Firebase();
                firebase.Auth = AccountManager.AccountToken;
                firebase.Child("Clans").Child(AccountManager.GetClan()).SetValue(jsonObject.ToString());

                EventManager.Dispatch("AccountUpdate");
                mPopUp.HideAll();
                mPanelManager.Hide();
                mPanelManager.Show("Menu", true);
            }
            else
            {
                mPopUp.HideAll();
                mPanelManager.Hide();
                mPanelManager.Show("Menu", true);
                error("Tag already taken");
            }
        }, delegate (string fail)
        {
            UIToast.Show("Create Clan Error: " + fail);
        });
    }
}
