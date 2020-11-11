using PagedList;
using Store.Areas.Admin.Models.ViewModels.Shop;
using Store.Models.Data;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //annoucing model type List
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //initialization model data
                categoryVMList = db.Categories.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }
                //return List in view
                return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //announce string value ID
            string id;

            using (Db db = new Db())
            {

                //check name category on unique
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //initialization model DTO
                CategoryDTO dto = new CategoryDTO();

                //add data in model
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //save
                db.Categories.Add(dto);
                db.SaveChanges();

                //get ID for return in View
                id = dto.Id.ToString();

            }

            //return ID in view 
            return id;
        }

        //ctreate method sorting
        //POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //begin counter
                int count = 1;

                //initializationmodel data
                CategoryDTO dto;

                //instal sorting for each page
                foreach (var catID in id)
                {
                    dto = db.Categories.Find(catID);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }

        //GET: method delete category
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //get model category
                CategoryDTO dto = db.Categories.Find(id);

                //delete category
                db.Categories.Remove(dto);

                //save in base
                db.SaveChanges();
            }

            //add message about success deletion category
            TempData["SM"] = "You have delete a category!";

            //redirect user
            return RedirectToAction("Categories");
        }

        //POST: method rename category
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //check name on unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //get model DTO
                CategoryDTO dto = db.Categories.Find(id);

                //edit model DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //save changing
                db.SaveChanges();
            }
            //return word
            return "ok";
        }

        //GET: create method add product
        [HttpGet]
        public ActionResult AddProduct()
        {
            //announce model data
            ProductVM model = new ProductVM();

            //add list categories from base in model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }

            //return model in view
            return View(model);

        }

        //POST: create method add product
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //check model on validity
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //check name product on unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            //announce variable ProductID
            int id;

            //initialization and save model on base ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            //add messagein TempDate
            TempData["SM"] = "You have added a product!";

            #region Upload Image

            // create needed link diretory
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            // check is directories (if not, create)
            if (!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }
            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }
            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }
            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }
            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            // check is file download
            if (file != null && file.ContentLength > 0)
            {
                //get extension file
                string ext = file.ContentType.ToLower();

                //check extension file
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension!");
                        return View(model);
                    }
                }

                //announce variable with name image
                string imageName = file.FileName;

                //save name image in model DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //assign path to original and reduce image
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                //save original image
                file.SaveAs(path);

                //create and save reduce copy
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
            }
                #endregion

                //readirect user
                return RedirectToAction("AddProduct");
        }

        //Get: create method list of products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //announcing ProductVM type List
            List<ProductVM> listOfProductVM;

            //set number of page
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //initialization list and fill data
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                //fill category data
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //set selected category
                ViewBag.SelectedCat = catId.ToString();
            }
            //set paging navigation
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

                //return view with data
            return View(listOfProductVM);
        }

        //Get: create method edit products
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //announcing model ProductVM
            ProductVM model;

            using (Db db = new Db())
            {
                //get product 
                ProductDTO dto = db.Products.Find(id);

                //check is available product
                if (dto == null)
                {
                    return Content("That product is not exist!");
                }

                //initialization model data
                model = new ProductVM(dto);

                //create list categories
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //get all images from gallery
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fileName => Path.GetFileName(fileName));
            }

                //return model in view
                return View(model);
        }

        //Post: create method edit products
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //get id product
            int id = model.Id;

            //fill list categories and images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(),"Id", "Name");
            }

            model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fileName => Path.GetFileName(fileName));

            //check model on validity
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //check name product on unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            //updqate product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            //message TempData
            TempData["SM"] = "You have edited the product!";

            //logic processing images
            #region Image Upload

            //check download file
            if (file != null && file.ContentLength > 0)
            {
                //get extension file
                string ext = file.ContentType.ToLower();

                //check extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension!");
                        return View(model);
                    }
                }


                //set path download
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete existing files and directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //save name of image
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //save original and preview version
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                //save original image
                file.SaveAs(path);

                //create and save reduce copy
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);

            }
            #endregion

                //redirect user
                return RedirectToAction("EditProduct");
        }

        //Post: create method delete product
        public ActionResult DeleteProduct(int id)
        {
            //delete product from data base
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            //delete directory product (image)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);
            }

            //redirect user
            return RedirectToAction("Products");
        }

        //Post: create method add images in gallery
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //sort through all get files
            foreach (string fileName in Request.Files)
            {
                //initialization files
                HttpPostedFileBase file = Request.Files[fileName];

                //check on NULL
                if (file != null && file.ContentLength > 0)
                {
                    //set path to directory
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //set path images
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    //save original image and reduce copy
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200,200).Crop(1,1);
                    img.Save(path2);
                }
            }
        }

        //Post: create method delete images from gallery
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);
        
            if (System.IO.File.Exists(fullPath1))
            {
                System.IO.File.Delete(fullPath1);
            }

            if (System.IO.File.Exists(fullPath2))
            {
                System.IO.File.Delete(fullPath2);
            }
        }

        //Create metod output all goods for administrator
        //POST: Admin/Shop/Orders
        public ActionResult Orders()
        {
            //initialization model OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                //initialization model OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //one for one all data model OrderVM
                foreach (var order in orders)
                {
                    //initialization dictionary goods
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    //announce var total summ
                    decimal total = 0m;

                    //initialization list OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList =
                        db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //get name user
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string username = user.Username;

                    //one for one list goods from OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //get good
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //get price good's
                        decimal price = product.Price;

                        //get name good
                        string productName = product.Name;

                        //add good in dictionary
                        productAndQty.Add(productName, orderDetails.Quantity);

                        //get total price goods
                        total += orderDetails.Quantity * price;
                    }
                    //add data in model OrdersForAdminVM
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
                //return view with model OrdersForAdminVM
                return View(ordersForAdmin);
        }

    }
}