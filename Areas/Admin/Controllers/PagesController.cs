using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Pages
        public ActionResult Index()
        {
            //Declare listfor view PageVM
            List<PageVM> pageList;

            //initialization list DB
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
           
                //return list view  
                return View(pageList);
        }

        // GET: Pages/AddPages
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Pages/AddPages
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Check model on validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {

                //ad short description slag
                string slug;

                //initialization class PageDTO
                PagesDTO dto = new PagesDTO();

                //assignment head model
                dto.Title = model.Title.ToUpper();

                //check has short description, if not assignment
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //confirm, that head and short description - unique
                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                //assignment remaining model value
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //save model to data base
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            //transmitt message through TempData
            TempData["SM"] = "You have added a new page!";

            //redirect user on method INDEX
            return RedirectToAction("Index");
        }

        // GET: Pages/EditPages
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //assignment model PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //get page
                PagesDTO dto = db.Pages.Find(id);

                //check if available page
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }
                //initialization model data
                model = new PageVM(dto);
            }
                //return model in view
                return View(model);
        }

        // POST: Pages/EditPages
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //check model on validity
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //get id page
                int id = model.Id;

                //assignment var short head
                string slug = "home";

                //get page on id
                PagesDTO dto = db.Pages.Find(id);

                //set name from sent model in DTO
                dto.Title = model.Title;

                //check short head and set it if need
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //check slug and title on unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                //write other value in class DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //save changing in base
                db.SaveChanges();
            }
            //messagein TempData
            TempData["SM"] = "You have edited page.";

            //redirect user
            return RedirectToAction("EditPage");
        }

        // GET: Pages/PageDetailsID
        public ActionResult PageDetails(int id)
        {
            //assignment  model PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //get page
                PagesDTO dto = db.Pages.Find(id);

                //confirm that page available
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                //set model info from base
                model = new PageVM(dto);
            }
                //return model in view
                return View(model);
        }

        //GET: method delete page
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //get page
                PagesDTO dto = db.Pages.Find(id);

                //delete page
                db.Pages.Remove(dto);

                //save in base
                db.SaveChanges();
            }

            //add message about success deletion page
            TempData["SM"] = "You have delete page!";

            //redirect user
            return RedirectToAction("Index");
        }

        //ctreate method sorting
        //POST: Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int [] id)
        {
            using (Db db = new Db())
            {
                //begin counter
                int count = 1;

                //initializationmodel data
                PagesDTO dto;

            //instal sorting for each page
            foreach (var pageID in id)
                {
                    dto = db.Pages.Find(pageID);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }

        // GET: Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //annoucing model
            SidebarVM model;
            using (Db db = new Db())
            {
                //get data from DTO
                SidebarDTO dto = db.Sidebars.Find(1);//badcode

                //fill model data
                model = new SidebarVM(dto);
            }
                //return view with model
                return View(model);
        }

        //POST: Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //get data from DTO
                SidebarDTO dto = db.Sidebars.Find(1);//BadCode

                //assign data in body, property Body
                dto.Body = model.Body;

                //save
                db.SaveChanges();
            }

            //message about success TempData
            TempData["SM"] = "You have edited the sidebar";

            //redirect user
            return RedirectToAction("EditSidebar");
        }
    }
}