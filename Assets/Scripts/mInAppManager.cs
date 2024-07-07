using UnityEngine;

public class mInAppManager : MonoBehaviour
{
	private GameCurrency Currency;

	private bool isRewardedVideo;

	public void OnRewardedVideo(int currency)
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
		}
		else
		{
			if (isRewardedVideo)
			{
				return;
			}
			Currency = (GameCurrency)currency;
			UIToast.Show(Localization.Get("Please wait") + "...");
			isRewardedVideo = true;
			TimerManager.In(0.5f, delegate
			{
				RewardedVideoComplete();
			});
		}
	}

	private void RewardedVideoComplete()
	{
		TimerManager.In(0.3f, delegate
		{
			AccountManager.Rewarded(Currency, delegate
			{
				if (Currency == GameCurrency.Money)
				{
					UIToast.Show("+50 BS Silver");
				}
				else
				{
					UIToast.Show("+1 BS Coins");
				}
				EventManager.Dispatch("AccountUpdate");
				isRewardedVideo = false;
			}, delegate(string e)
			{
				UIToast.Show(e);
				isRewardedVideo = false;
			});
		});
	}
}
