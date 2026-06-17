using System;
using System.IO;
using System.Linq;

namespace test.DatabaseContext
{
    public partial class Product
    {
        public decimal FinalPrice
        {
            get
            {
                return Price - Price * Discount / 100;
            }
        }

        public bool HasDiscount
        {
            get
            {
                return Discount > 0;
            }
        }

        public bool BigDiscount
        {
            get
            {
                return Discount > 15;
            }
        }

        public bool OutOfStock
        {
            get
            {
                return Amount == 0;
            }
        }

        public string FullImagePath
        {
            get
            {
                string productsFolder = Path.GetFullPath(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"..\..\Materials\Products"));

                string imageName = string.IsNullOrWhiteSpace(Photo)
                    ? "picture.png"
                    : Photo.Trim();

                string imagePath = Path.Combine(
                    productsFolder,
                    imageName);

                if (!File.Exists(imagePath))
                {
                    return Path.Combine(
                        productsFolder,
                        "picture.png");
                }

                return imagePath;
            }
        }
    }

    public partial class Order
    {
        public string ProductArticle
        {
            get
            {
                var articles = Product_Order
                    .Where(x =>
                        x.Product != null
                        && !string.IsNullOrWhiteSpace(
                            x.Product.Article))
                    .Select(x => x.Product.Article)
                    .ToList();

                if (articles.Count == 0)
                {
                    return "Товар не указан";
                }

                return string.Join(", ", articles);
            }
        }
    }

    public partial class Address
    {
        public string StreetName
        {
            get
            {
                if (Streets == null)
                {
                    return "";
                }

                return Streets.Name;
            }
        }

        public string CityName
        {
            get
            {
                if (Streets == null || Streets.City == null)
                {
                    return "";
                }

                return Streets.City.Name;
            }
        }
    }
}