using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CoinforgeSDK.UI;
using CoinforgeSDK;
using UnityEngine.Purchasing;
using DG.Tweening;


namespace CoinforgeSDK.UI {


	public class AuthorizeView : UIView {

		public Image brandLogo;

		public GameObject SignUpButton;
		public GameObject LoginButton;
		public GameObject PlayAsGuestButton;


		public override void ViewAppeared() {
			base.ViewAppeared();

			CoinforgeInit.Instance.Init(delegate {
				Present(delegate {

					//show loading
					ModalView.instance.ShowActivity();


					Session session = new Session();
					
					if (!session.IsAuthorized) {
						//first session
						ShowButtons();
						ModalView.instance.HideActivity();
					}
					else {
						//not first session
						if (session.IsGuestSession) {
							//following session with guest mode display dialog
							ShowButtons();
							ModalView.instance.HideActivity();
						}
						else {
							//email user
							Coinforge.Instance.Authorize(AuthorizationSuccess, delegate(string error) {
								ShowButtons();
								AuthorizationFailed(error);
							});
						}
					}

				});
			});
		}



		private void ResetView() {
			brandLogo.color = new Color(1f, 1f, 1f, 0);
			HideButtons();
		}



		private void Present(TweenCallback OnPresented) {

			ResetView();

			//fade brand
			Sequence sequence = DOTween.Sequence();
			sequence.Append(brandLogo.DOFade(1f, 1f)).SetEase(Ease.Linear);
			sequence.AppendCallback(OnPresented);
		}


		private void ShowButtons() {
			SignUpButton.SetActive(true);
			LoginButton.SetActive(true);
			PlayAsGuestButton.SetActive(true);
		}


		private void HideButtons() {
			SignUpButton.SetActive(false);
			LoginButton.SetActive(false);
			PlayAsGuestButton.SetActive(false);
		}



		private void OnViewPresented() {

		}




		//TODO add retrying coroutine to all error UX
		private void AuthorizationFailed(string error) {

			if (error == "User canceled") {
				ModalView.instance.HideActivity();
				return;
			}

			Debug.LogError("AuthorizationFailed: " + error);
			ModalView.instance.ShowAlert("Authorization failed", error, new string[] {"Try again"}, null);
		}


		//todo move this outside the view
		private void AuthorizationSuccess() {

			Debug.Log("AuthorizationSuccess");
			ModalView.instance.ShowActivity();


			//pull user details
			Coinforge.Instance.GetUserDetails(delegate(User user) { Coinforge.Instance.GetAccounts(delegate(List<User.Account> accounts) { Coinforge.Instance.GetAccountBalance(delegate(User.Account.Balance balance) { CoinforgeIAP.Instance.Initialize(Coinforge.Instance.CurrencyConfig.IAPProductIds, delegate(Product[] products) { CoinforgeInit.Instance.LoadMainScene(); }, delegate(InitializationFailureReason reason) { ModalView.instance.ShowAlert("Unable to load products", reason.ToString(), new string[] {"Try again"}, null); }); }, delegate(string getBalanceError) { ModalView.instance.ShowAlert("Coinforge get balance error", getBalanceError, new string[] {"Try again"}, null); }); }, delegate(string getAccountsError) { ModalView.instance.ShowAlert("Coinforge get user accounts error", getAccountsError, new string[] {"Try again"}, null); }); }, delegate(string getUserDetailsError) { ModalView.instance.ShowAlert("Coinforge user details error", getUserDetailsError, new string[] {"Try again"}, null); });

		}


		public void ButtonPlayAsGuestTapped() {
			ModalView.instance.ShowActivity();
			Coinforge.Instance.AuthorizeGuest(AuthorizationSuccess, AuthorizationFailed);
		}


		public void ButtonSignUpTapped() {

			Session session = new Session();

			ModalView.instance.ShowActivity();

			Coinforge.Instance.AuthorizeGuest(delegate { Coinforge.Instance.SignUp(AuthorizationSuccess, AuthorizationFailed); }, AuthorizationFailed);


		}


		public void ButtonLoginTapped() {
			Coinforge.Instance.Authorize(AuthorizationSuccess, AuthorizationFailed);
		}

	}
}