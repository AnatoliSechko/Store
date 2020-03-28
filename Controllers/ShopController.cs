using Store.Models.Data;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //announce model type List<> CategoryVN
            List<CategoryVM> categoryVMList;

            //initialization model data
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x))
                    .ToList();
            }

            //return partial view with model
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        // GET: Shop/Category/name
        public ActionResult Category(string name)
        {
            //announce List
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //get id category
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                //initialization list data
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x))
                    .ToList();

                //get name category
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //check on NULL
                if (productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }

                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }
            //return view with model
            return View(productVMList);
        }

        // GET: Shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //anounce models DTO and VM
            ProductDTO dto;
            ProductVM model;

            //initialization ID producta
            int id = 0;

            using (Db db = new Db())
            {
                //check is available product
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                //initialization model DTO data
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //get ID
                id = dto.Id;

                //initialization model VM data
                model = new ProductVM(dto);
            }
            //get image from galery
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
            .Select(fn => Path.GetFileName(fn));

                //return model in view
                return View("ProductDetails", model);
        }
    }
}