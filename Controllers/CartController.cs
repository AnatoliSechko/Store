using Store.Models.Data;
using Store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //announce list of CartVM
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //check is basket empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            //doing summ and write in ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //return list in view
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //announce model CartVM
            CartVM model = new CartVM();

            //announce var quantity
            int qty = 0;

            //announce var price
            decimal price = 0m;

            //check session of busket
            if (Session["cart"] != null)
            {

                //get total quantity products and price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {
                //or install quantity and price in 0
                model.Quantity = 0;
                model.Price = 0m;
            }
            //return part view with model
            return PartialView("_CartPartial", model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //announce list, param type CartVM 
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //announce model CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //get product 
                ProductDTO product = db.Products.Find(id);

                //check, is the product already in busket
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //if not, to add new product in busket
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }

                //if yes, add count product in busket +1
                else
                {
                    productInCart.Quantity++;
                }
            }

            //get total count, price and add data in model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //save condition busket in session
            Session["cart"] = cart;

            //return part view with model
            return PartialView("_AddToCartPartial", model);
        }

        //GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //announc list cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get model CartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //add count
                model.Quantity++;

                //save required data
                var result = new { qty = model.Quantity, price = model.Price };

                //return JSON answer with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //GET: /cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            //announc list cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get model CartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //reduce count
                if (model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                //save required data
                var result = new { qty = model.Quantity, price = model.Price };

                //return JSON answer with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct(int productId)
        {
            //announc list cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get model CartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            //get list goods in busket
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //return partial view  with list
            return PartialView(cart);
        }

        //POST: /cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //get list goods in busket
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //get name user
            string userName = User.Identity.Name;

            //announce variable for orderId
            int orderId = 0;

            using (Db db = new Db())
            {
                //announce model OrderDTO
                OrderDTO orderDto = new OrderDTO();

                //get Id user
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                //fil model OrderDTO data nad save
                orderDto.UserId = userId;
                orderDto.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDto);
                db.SaveChanges();

                //get orderId
                orderId = orderDto.OrderId;

                //announce model OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // add in model data
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }
            //sent letter about order on mail administrator
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("062a6ec026cdf6", "2204f3a3f882e2"),
                EnableSsl = true
            };
            client.Send("shop@example.com", "admin@example.com", "New Order", $"You have a new order. Order number: {orderId}");

            //equal zero session
            Session["cart"] = null;
        }
    }
}