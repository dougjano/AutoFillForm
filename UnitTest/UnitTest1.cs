using System;
using System.Collections.Generic;
using AutoFillForm;
using AutoFillForm.Driver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethodAutomation()
        {
            FormFiller filler = new FormFiller();

            filler.ExecuteAutomation("https://login.mailchimp.com/", new List<Tuple<string, string>>() { new Tuple<string, string>("username", "dougjano2"), new Tuple<string, string>("password", "123") }, 0, false);
            filler.ExecuteAutomation("https://www.google.com/maps/", new List<Tuple<string, string>>() { new Tuple<string, string>("searchboxinput", "pastor mamelio") }, 0, false);
        }

        [TestMethod]
        public void TestMethodAutomationOnSamePage()
        {
            FormFiller filler = new FormFiller();

            filler.ExecuteAutomation("https://login.mailchimp.com/", new List<Tuple<string, string>>() { new Tuple<string, string>("username", "dougjano2"), new Tuple<string, string>("password", "123") }, 0, false);
            filler.ExecuteAutomation("https://www.google.com/maps/", new List<Tuple<string, string>>() { new Tuple<string, string>("searchboxinput", "pastor mamelio") }, 0, false, "mailchimp");
        }

        [TestMethod]
        public void TestMethodExecuteSite()
        {
            FormFiller filler = new FormFiller();

            filler.ExecuteWebSite("https://login.mailchimp.com/", false);
            filler.ExecuteWebSite("https://www.google.com/maps/", false, "mailchimp");
        }
    }
}
