using FluxDayAutomation.PageObjects;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using FluxDayAutomation.Drivers;

namespace FluxDayAutomation.UITests
{
    [TestFixture]
    [Category("ApproveOKR")]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V1)]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V2)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V1)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V2)]
    public class ApproveOKRTest
    {
        IWebDriver driver;

        private const string MANAGER_EMAIL = "admin@fluxday.io";
        private const string MANAGER_USERNAME = "Admin User";
        private const string MANAGER_PASSWORD = "password";
        private const string APPLICATION_URL = "https://app.fluxday.io/users/sign_in";
        private const string TESTOKR_NAME = "Temporary OKR";
        private const string TESTOKR_OBJECTIVENAME = "Some name";
        private const string TESTOKR_V2 = "Some more data";
        private const int EXPLICIT_WAIT_SECONDS = 10;
        private const int IMPLICIT_WAIT_TIMEOUT = 10;
        private const string TEARDOWN_EXCEPTION_TEXT = "TearDown: OKR was not deleted!";

        [SetUp]
        public void SetupEnvironment()
        {
            // Configuring timeout and navigating to Application
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(IMPLICIT_WAIT_TIMEOUT);
            driver.Navigate().GoToUrl(APPLICATION_URL);
            
            // Preconditions: login as manager and create OKR
            var loginObject = new LoginPageObject(driver);
            loginObject.UserEmailTextBox.SendKeys(MANAGER_EMAIL);
            loginObject.UserPasswordTextBox.SendKeys(MANAGER_PASSWORD);
            loginObject.LoginButton.Click();
            var sidebarMenu = new SideBarMenuPageObject(driver);
            sidebarMenu.OKRItem.Click();
            var okrListPageObject = new OKRListPageObject(driver);
            okrListPageObject.UsersComboBox.SelectByText(MANAGER_USERNAME);
            okrListPageObject.NewOKRButton.Click();

            // OKR fillout
            var newOKRPageObject = new SetOKRPageObject(driver);
            newOKRPageObject.OKRNameField.SendKeys(TESTOKR_NAME);
            newOKRPageObject.ObjectivesList[0].ObjectiveNameField.SendKeys(TESTOKR_OBJECTIVENAME);
            newOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField.SendKeys(TESTOKR_V2);
            newOKRPageObject.ObjectivesList[0].KeyResultsList[1].KeyResultField.SendKeys(TESTOKR_V2);
            newOKRPageObject.SaveButton.Click();
        }

        public ApproveOKRTest(string browser, string version)
        {
            this.driver = SelenoidDrivers.CreateDriver(browser, version);
        }

        [Test]
        public void ApproveOKR()
        {
            var approveOKRPageObj = new ApproveOKRPageObject(driver);
            approveOKRPageObj.ApproveButton.Click();
            Assert.IsTrue(approveOKRPageObj.ApprovedOKRLabel.Displayed);
        }

        [TearDown]
        public void CleanEnvironment()
        {
            var okrListPageObject = new OKRListPageObject(driver);
            okrListPageObject.UsersComboBox.SelectByText(MANAGER_USERNAME);

            var approveOKRPageObject = new ApproveOKRPageObject(driver);
            var lastCreatedOKR = okrListPageObject.OKRsList.Last();

            // Last created OKR is placed to the bottom of OKRs list
            if (lastCreatedOKR.TitleLabel.Text == TESTOKR_NAME)
            {
                approveOKRPageObject.SettingsButton.Click();
                approveOKRPageObject.SettingsMenu.DeleteMenuItem.Click();
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(EXPLICIT_WAIT_SECONDS));
                wait.Until(ExpectedConditions.AlertIsPresent());
                driver.SwitchTo().Alert().Accept();
            }
            else
            {
                throw new Exception(TEARDOWN_EXCEPTION_TEXT);
            }

            driver.Quit();
        }
    }
}