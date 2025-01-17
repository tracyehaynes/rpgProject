using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Shops
{
    public class Shopper : MonoBehaviour
    {

        Shop activeShop = null;

        public event Action activeShopChanged;


        public void SetActiveShop (Shop shop)
        {
            if (activeShop != null)
            {
                activeShop.SetShopper (null);
            }

            activeShop = shop;

            if (activeShop != null)
            {
                activeShop.SetShopper(this);
            }

            if (activeShopChanged != null)
            {
                activeShopChanged ();
            }
        }

        public Shop GetActiveShop ()
        {
            return activeShop;
        }

        public void Quit ()
        {
            activeShop = null;
            if (activeShopChanged != null)
            {
                activeShopChanged ();
            }
        }
    }
}