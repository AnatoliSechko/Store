using Store.Models.Data;
using Store.Models.ViewModels.Account;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //check model on validation
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            //check confirm passord
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match!");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //check name on unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Username {model.Username} is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //create example classes UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                //Add data in model
                db.Users.Add(userDTO);

                //save data
                db.SaveChanges();

                //add role user
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }
            //note message in TempData
            TempData["SM"] = "You are now registered and can login.";

            //redirect user
            return RedirectToAction("Login");

        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //confirm, user does not autorization
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");

            //return view
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //check model on validation
            if (!ModelState.IsValid)
                return View(model);

            //check user on validation
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                    isValid = true;

                if (!isValid)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }

        }

        // GET: /account/logout
        [Authorize]
        public ActionResult logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            //get name user
            string userName = User.Identity.Name;

            //announce model 
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //fill model data from context (DTO)
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            //return part view with model
            return PartialView(model);
        }

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //get name user
            string userName = User.Identity.Name;

            //announce model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //initialization model data
                model = new UserProfileVM(dto);

            }
            //returm model in view
            return View("UserProfile", model);
        }

        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;
            //check model on validation
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //check password (if user change it)
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //get user name
                string userName = User.Identity.Name;

                //check, if change name user
                if (userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;
                }

                //check name ou unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", $"UserName {model.Username} already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //change model context data
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //save changes
                db.SaveChanges();
            }

            //install message in TempData
            TempData["SM"] = "You have edited your profile!";

            if (!userNameIsChanged)
                //return view with model
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");
        }

        //GET: /account/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //initialization model OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //get ID user
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = user.Id;

                //initialization model OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM()).ToList();

                //one for one list goods in OrderVM
                foreach (var order in orders)
                {
                    //initialization dictionary goods
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //announce var total summ
                    decimal total = 0m;

                    //initialization model OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = 
                        db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //one for one list OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        //get good
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //get price good
                        decimal price = product.Price;

                        //get name good
                        string productName = product.Name;

                        //add good in dictionary
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        //get total price good
                        total += orderDetails.Quantity * price;

                    }
                    //add gotten data in model OrdersForUserVM
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                       OrderNumber = order.OrderId,
                       Total = total,
                       ProductsAndQty = productsAndQty,
                       CreatedAt = order.CreatedAt
                    });
                }
            }
            //return view with model OrdersForUserVM
            return View(ordersForUser);
        }
    }
}