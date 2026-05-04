using ShopShoe.Db;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShopShoe.Helpers
{
    public class ImageHelper
    {
        private ShopShoeDbEntities _db = new ShopShoeDbEntities();
        public void DeleteOldImages()
        {
            var oldFiles = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res"));
            
            var images = _db.Product.Select(p => p.Photo).ToList();
            foreach(var image in oldFiles)
            {
                if (!images.Contains("res/" + Path.GetFileName(image)) && Path.GetFileName(image) != "picture.png")
                {
                    if (File.Exists(image))
                    {
                        try
                        {
                            File.Delete(image);
                        }
                        catch (Exception ex)
                        {
                        }
                        
                    }
                        

                }

            }
        }
    }
}
