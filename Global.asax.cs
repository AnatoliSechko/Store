using Store.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Store
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //create metod treatment requests authentification
        protected void Application_AuthenticateRequest()
        {
            //check if user autorization
            if (User == null)
                return;

            //get name user
            string userName = Context.User.Identity.Name;

            //announce massive roles
            string[] roles = null;

            using (Db db = new Db())
            {
                //fill massive roles
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                if (dto == null)
                    return;

                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }

            //create object interface IPrincipal
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            //announce and initialization data Context.User
            Context.User = newUserObj;
        }
    }
}
