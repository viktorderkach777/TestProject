using System;
using NUnit.Framework;
using OpenQA.Selenium;
using FluxDayAutomation.Drivers;
using FluxDayAutomation.PageObjects;
using System.Threading;
using System.Linq;

namespace FluxDayAutomation.UITests
{
    [TestFixture]
    [Category("DeleteOKR")]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V1)]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V2)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V1)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V2)]

    public class DeleteOKRTest
    {
        private const string APPLICATION_URL = "https://app.fluxday.io";
        private const int IMPLICIT_WAIT_TIMEOUT = 10;
        private const string MANAGER_USER_NAME = "Admin User";
        private const string MANAGER_EMAIL = "admin@fluxday.io";
        private const string MANAGER_PASSWORD = "password";
        private const string TEST_OKR_NAME = "Delete OKR Test: Temporary OKR#";
        private const string TEST_OKR_OBJECTIVE_TEXT = "Test Objective";
        private const string TEST_OKR_KEYRESULT_TEXT = "Test Key Result";
        private const string ASSERT_TEXT = "Assert: Found OKR, created in this test. Please delete it manually.";
        private const string OKR_WAS_NOT_CREATED_TEXT = "Error: can\'t find OKR which was created in pre-conditions setup!";
        
        // Thread.Sleep is called while running on Chrome to avoid exceptions. Chrome ignores WebDriver's implicit waiters
        private const int CHROME_THREAD_SLEEP_TIME = 200;

        private IWebDriver driver;
        private OKRListPageObject okrListPageObject;
        private string testOKRName, currentBrowser;

        public DeleteOKRTest(string browser, string version)
        {
            driver = SelenoidDrivers.CreateDriver(browser, version);
            currentBrowser = browser;
        }

        [SetUp]
        public void Initialization()
        {
            // Configuring Implicit Timeout and navigating to Application
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(IMPLICIT_WAIT_TIMEOUT);
            driver.Navigate().GoToUrl(APPLICATION_URL);

            // Pre-condition: Logging in as manager
            var loginPageObject = new LoginPageObject(driver);
            SetInputField(loginPageObject.UserEmailTextBox, MANAGER_EMAIL);
            SetInputField(loginPageObject.UserPasswordTextBox, MANAGER_PASSWORD);
            loginPageObject.LoginButton.Click();

            // Navigating to OKRs list page
            var sidebarMenuPageObject = new SideBarMenuPageObject(driver);
            sidebarMenuPageObject.OKRItem.Click();

            // Pre-condition: creating temporary OKR for Admin User
            okrListPageObject = new OKRListPageObject(driver);
            okrListPageObject.UsersComboBox.SelectByText(MANAGER_USER_NAME);
            okrListPageObject.NewOKRButton.Click();

            var newOKRPageObject = new SetOKRPageObject(driver);
            testOKRName = TEST_OKR_NAME + new Random().Next(0, 999).ToString();
            SetInputField(newOKRPageObject.OKRNameField, testOKRName);
            SetInputField(newOKRPageObject.ObjectivesList[0].ObjectiveNameField, TEST_OKR_OBJECTIVE_TEXT);
            newOKRPageObject.ObjectivesList[0].KeyResultsList[0].RemoveKeyResultLink.Click();  // Removing unnecessary Key Result
            SetInputField(newOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField, TEST_OKR_KEYRESULT_TEXT);
            newOKRPageObject.SaveButton.Click();
        }

        [Test]
        public void DeleteOKRTestCase()
        {
            var approveOKRPageObject = new ApproveOKRPageObject(driver);
            
            okrListPageObject.UsersComboBox.SelectByText(MANAGER_USER_NAME);
                  
            // OKR created in pre-conditions setup gets last position in OKRs list for user
            if (okrListPageObject.OKRsList.Last().TitleLabel.Text == testOKRName)
            {
                okrListPageObject.OKRsList.Last().TitleLabel.Click();

                // Thread.Sleep is called while running on Chrome to avoid exceptions. Chrome ignores WebDriver's implicit waiters
                if (currentBrowser == SelenoidDrivers.CHROME)
                {
                    Thread.Sleep(CHROME_THREAD_SLEEP_TIME);
                }
                approveOKRPageObject.SettingsButton.Click();

                // Thread.Sleep is called while running on Chrome to avoid exceptions. Chrome ignores WebDriver's implicit waiters
                if (currentBrowser == SelenoidDrivers.CHROME)
                {
                    Thread.Sleep(CHROME_THREAD_SLEEP_TIME);
                }
                approveOKRPageObject.SettingsMenu.DeleteMenuItem.Click();
                driver.SwitchTo().Alert().Accept();
            }
            else
            {
                throw new Exception(OKR_WAS_NOT_CREATED_TEXT);
            }
            
            // Checking results
            driver.Navigate().GoToUrl(APPLICATION_URL + "/okrs");
            okrListPageObject.UsersComboBox.SelectByText(MANAGER_USER_NAME);

            foreach (var okr in okrListPageObject.OKRsList)
            {
                Assert.AreNotEqual(okr.TitleLabel.Text, testOKRName, ASSERT_TEXT);
            }
        }

        [TearDown]
        public void CleanUp()
        {
            driver.Quit();
        }

        private void SetInputField(IWebElement field, string text)
        {
            field.Clear();
            field.SendKeys(text);
        }
    }
}
