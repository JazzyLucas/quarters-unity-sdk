using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK;
using QuartersSDK.UI;
using UnityEngine.EventSystems;

public class ModalView : UIView {
 
	public static ModalView instance;
	public delegate void AlertButtonTappedDelegate(string button);
	public AlertButtonTappedDelegate alertButtonDelegate;

	public Text title;
	public Text message;
	public Button buttonPrototype;
	public RectTransform alertRect;
	public Image activityIcon;
	public GameObject activityView;

    public AudioSource clickSFX;

    private List<Button> buttons = new List<Button>();

	private CanvasGroup alertViewCanvasGroup {
		get {
			return alertRect.GetComponent<CanvasGroup>();
		}
	}


	public override void Awake () {
		base.Awake ();
		buttonPrototype.gameObject.SetActive(false);
		instance = this;

		alertViewCanvasGroup.alpha = 0;
		alertViewCanvasGroup.interactable = false;
		alertViewCanvasGroup.blocksRaycasts = false;
		alertRect.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		ViewWillDissappear();
		SetVisible(false);
		ViewDisappeared();
	}

    public bool IsActive() {
        return camera.enabled;
    }



    public void ShowNoConnectionAlert(string[] buttonNames, AlertButtonTappedDelegate tappedDelegate) {
        ModalView.instance.ShowAlert("Connection Error", "We can’t seem to connect you to the game. Please check your connection and try again.", buttonNames, tappedDelegate);
    }


	public void ShowAlert(string title, string message, string[] buttonNames, AlertButtonTappedDelegate alertButtonDelegate) {
		this.alertButtonDelegate = alertButtonDelegate;

		alertRect.localPosition = Vector3.zero;

		activityView.SetActive(false);
		Debug.Log("Show alert: " + title);

		Clean();

		this.title.text = title;
		this.message.text = message;

		for (int i=0; i<buttonNames.Length; i++) {
			Button copy = Instantiate<Button>(buttonPrototype);
			copy.transform.SetParent(buttonPrototype.transform.parent, false);
			copy.GetComponentInChildren<Text>().text = buttonNames[i];
			copy.gameObject.SetActive(true);


			this.buttons.Add(copy);
		}


			
		alertViewCanvasGroup.alpha = 1f;
		alertViewCanvasGroup.interactable = true;
		alertViewCanvasGroup.blocksRaycasts = true;
  //
  //       alertRect.DOKill();
		// alertRect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
		//
		alertRect.transform.localScale = Vector3.one;
			
		ViewWillAppear();
		SetVisible(true);
		ViewAppeared();

		StartCoroutine(Fit());

	}


	public void Hide(System.Action OnAnimationFinished = null) {
		
		alertRect.transform.localScale = Vector3.zero;
		
		alertViewCanvasGroup.alpha = 0;
		alertViewCanvasGroup.interactable = false;
		alertViewCanvasGroup.blocksRaycasts = false;
		ViewWillDissappear();
		SetVisible(false);
		ViewDisappeared();
		if (OnAnimationFinished != null) OnAnimationFinished();
		alertButtonDelegate = null;
		
	}


	private IEnumerator Fit() {

		yield return new WaitForEndOfFrame();
		//scale background up to accommodate
		RectTransform messageRect = (RectTransform)this.message.transform;
		alertRect.sizeDelta = new Vector2(alertRect.sizeDelta.x, messageRect.rect.height + 235f);

	}



	void Clean() {
		foreach (Button button in buttons) Destroy(button.gameObject);
		this.buttons = new List<Button>();
	}




	public void ButtonTapped(Button button) {

        clickSFX.Play();

        string buttonText = button.GetComponentInChildren<Text>().text;

		Hide(delegate() {
            if (alertButtonDelegate != null) alertButtonDelegate(buttonText);
		});


	}



	public void ShowActivity() {

		activityIcon.sprite = Quarters.Instance.CurrencyConfig.CurrencyLogo;

		ViewWillAppear();
		SetVisible(true);
		ViewAppeared();
		activityView.SetActive(true);
	}
		

	public void HideActivity() {

		activityView.SetActive(false);
		ViewWillDissappear();
		SetVisible(false);
		ViewDisappeared();
		
	}
}