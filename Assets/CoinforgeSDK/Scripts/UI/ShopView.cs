using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CoinforgeSDK.UI;
using CoinforgeSDK;
using UnityEngine.Purchasing;

namespace CoinforgeSDK.UI {
    public class ShopView : UIView {

        [SerializeField] private Text ShopTitleText;
        [SerializeField] private UIList productsList;
        [SerializeField] private Image logo;
        [SerializeField] private AspectRatioFitter logoAspectRatioFitter;

        public override void ViewWillAppear(UIView sourceView = null) {
            base.ViewWillAppear(sourceView);

            ShopTitleText.text = $"Buy " + Coinforge.Instance.CurrencyConfig.DisplayNamePlural;
            productsList.Populate(new ArrayList(CoinforgeIAP.Instance.products));

            Sprite logoSprite = CoinforgeInit.Instance.CurrencyConfig.CurrencyLogo;
            float aspectRatio = (float) logoSprite.rect.width / (float) logoSprite.rect.height;
            logoAspectRatioFitter.aspectRatio = aspectRatio;
            logo.sprite = logoSprite;

        }



        public void ProductButtonTapped(UIProductButton productButton) {

            ModalView.instance.ShowActivity();
            CoinforgeIAP.Instance.BuyProduct(productButton.product, PurchaseSucessfullDelegate, PurchaseFailedDelegate);

        }

        private void PurchaseFailedDelegate(string error) {
            ModalView.instance.ShowAlert("Purchase Error", error, new string[] {"OK"}, null);
        }

        private void PurchaseSucessfullDelegate(Product product, string txid) {

            //refresh balance
            Coinforge.Instance.GetAccountBalance(delegate(User.Account.Balance newBalance) {
                ModalView.instance.HideActivity();

                int quantity = CoinforgeIAP.Instance.ParseCoinsQuantity(product, CoinforgeInit.Instance.CurrencyConfig);
                
                SpendRewardView.Instance.Present(quantity);
                
            }, delegate(string balanceError) {
                ModalView.instance.ShowAlert("Balance Error", balanceError, new string[] {"OK"}, null);
            });
        }
    }
}