using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using RegistrationAndLogin.Models;

namespace RegistrationAndLogin.Controllers
{
    public class UserController : Controller
    {
       //Registration Action

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }


        //Registration POST Action

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";
            //
            //Model Validation
            if(ModelState.IsValid)
            {
                //Email is already exist

                var isExist = IsEmailExist(user.EnailID);
                if(isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);

                }
                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid();

                #endregion

                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); ;
                #endregion
                user.IsEmailVerified = false;

                #region Save to Datebase
                using (MyDatabaseEntities2 dc = new MyDatabaseEntities2())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //send email to user
                    SendVerificationLink(user.EnailID, user.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link has been sent to your email id :" + user.EnailID;
                    Status = true; 
                }
                #endregion

            }
            else
            {
                message = "invalid request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;

            //send email to user

            return View(user);
        }

        //Verify Email

        //Verify Email link


        //Login

        //Login POST


        //Logout

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using(MyDatabaseEntities2 dc = new MyDatabaseEntities2())
            {
                var v = dc.Users.Where(a => a.EnailID == emailID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLink(string emailID,string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("mihais.photography@gmail.com", "MihaiS Photography");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "liceminescu2"; // Replace with actual password
            string subject = "Your account is successfully created!";

            string body = "<br/><br/>We are excited to tell you that your MihaiS Photography account is" +
                " successfully created. Please click on the below link to verify your account" +
                " <br/><br/><a href='" + link + "'>" + link + "</a> ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

    }
   
}