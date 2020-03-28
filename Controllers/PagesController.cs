using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            //get/install short title(slug)
            if (page == "")
                page = "home";

            //announce model and class DTO
            PageVM model;
            PagesDTO dto;

            //check is available page
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });
            }

            //get DTO page
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //instal title page
            ViewBag.PageTitle = dto.Title;

                //check side bar
                if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            //fill model data
            model = new PageVM(dto);

                //return view with model
                return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //initialization list PageVM
            List<PageVM> pageVMList;

            //get all pages excepc home
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x)).ToList();
            }
            //return part view with list data
            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //announce model
            SidebarVM model;

            //initialization model data
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                model = new SidebarVM(dto);
            }

            //return model in partial view
            return PartialView("_SidebarPartial", model);
        }
    }
}