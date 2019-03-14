using System;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using FluxDayAutomation.PageObjects;
using FluxDayAutomation.Drivers;

namespace FluxDayAutomation.UITests
{
    [TestFixture]
    [Category("EditOKR")]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V1)]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V2)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V1)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V2)]
    public class EditOKRTestSuit
    {
        private const string APPLICATION_URL = "https://app.fluxday.io";
        private const string OKRS_PAGE_LINK = APPLICATION_URL + "/okrs";
        private const int IMPLICIT_WAIT_TIMEOUT = 10;
        private const int PAGE_LOAD_TEST_TIMEOUT = 10;
        private const string MANAGER_EMAIL = "admin@fluxday.io";
        private const string MANAGER_PASSWORD = "password";
        private const string TEST_USER_NAME = "Team Lead";
        private const string TEST_OKR_DEFAULT_NAME = "Edit OKR Test: OKR#";
        private const string TEST_OKR_DEFAULT_OBJECTIVE = "Test OKR Objective";
        private const string TEST_OKR_DEFAULT_KEY_RESULT_1 = "Test Key Result 1";
        private const string TEST_OKR_DEFAULT_KEY_RESULT_2 = "Test Key Result 2";
        private const string TEST_OKR_CHANGED_TITLE = "Change Name Test";
        private const string TEST_OKR_CHANGED_OBJECTIVE = "Edited for test";
        private const string TEST_OKR_CHANGED_KEY_RESULT_1 = "Key result is edited for test";
        private const string OKR_NOT_FOUND_EXCEPTION_TEXT = "Can\'t find OKR which is used for this test!";
        private const string OBJECTIVE_WAS_NOT_EDITED_TEXT = "Objective was not edited!";
        private const string KEY_RESULT_WAS_NOT_EDITED_TEXT = "Key Result was not edited!";
        private const string OBJECTIVE_WAS_NOT_DELETED_TEXT = "Objective was not deleted!";
        private const string KEY_RESULT_WAS_NOT_DELETED_TEXT = "Key Result was not deleted!";
        private const string CANCEL_LINT_TEST_FAIL_TEXT = "Cancel Lint Test failed or browser redirection error";
        private const string PAGE_WAS_NOT_LOADED_TEXT = "Page was not fully loaded! (Button \"Add Objective\" is not accessible)";
        private const string TEAR_DOWN_EXCEPTION_TEXT = "OKR, created in this test was not deleted! Please delete it manually";

        private IWebDriver driver;
        private OKRListPageObject okrListPageObject;
        private ApproveOKRPageObject approveOKRPageObject;
        private SetOKRPageObject editOKRPageObject;

        private string currentTestOKRName, currentTestOKRLink;
        private string currentBrowser;

        public EditOKRTestSuit(string browser, string version)
        {
            driver = SelenoidDrivers.CreateDriver(browser, version);
            currentBrowser = browser;
        }

        [OneTimeSetUp]
        public void Initialization()
        {
            // Configuring Implicit Timeout and navigating to Application
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(IMPLICIT_WAIT_TIMEOUT);
            driver.Navigate().GoToUrl(APPLICATION_URL);

            //Initializing globals
            approveOKRPageObject = new ApproveOKRPageObject(driver);
            editOKRPageObject = new SetOKRPageObject(driver);
            okrListPageObject = new OKRListPageObject(driver);

            // Pre-condition: Logging in as manager
            var loginPageObject = new LoginPageObject(driver);
            loginPageObject.UserEmailTextBox.Clear();
            loginPageObject.UserEmailTextBox.SendKeys(MANAGER_EMAIL);
            loginPageObject.UserPasswordTextBox.Clear();
            loginPageObject.UserPasswordTextBox.SendKeys(MANAGER_PASSWORD);
            loginPageObject.LoginButton.Click();

            // Navigating to OKRs list page and selecting Team Lead User
            var sidebarMenuPageObject = new SideBarMenuPageObject(driver);
            sidebarMenuPageObject.OKRItem.Click();
        }

        [SetUp]
        public void CreateNewOKRForTest()
        {
            SelectTestUser();
            okrListPageObject.NewOKRButton.Click();
            currentTestOKRName = TEST_OKR_DEFAULT_NAME + new Random().Next(0, 999).ToString(); // Generating random name for OKR
            SetInputField(editOKRPageObject.OKRNameField, currentTestOKRName);
            SetInputField(editOKRPageObject.ObjectivesList[0].ObjectiveNameField, TEST_OKR_DEFAULT_OBJECTIVE);
            SetInputField(editOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField, TEST_OKR_DEFAULT_KEY_RESULT_1);
            SetInputField(editOKRPageObject.ObjectivesList[0].KeyResultsList[1].KeyResultField, TEST_OKR_DEFAULT_KEY_RESULT_2);
            editOKRPageObject.SaveButton.Click();
        }

        [Test]
        public void EditOKRPageLoadTest()
        {
            EnterEditMode();
            
            // Waiting until last element on Page Object will be available for clicking
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(PAGE_LOAD_TEST_TIMEOUT));
            Assert.DoesNotThrow(() => wait.Until(ExpectedConditions.ElementToBeClickable(editOKRPageObject.AddObjectiveButton)), PAGE_WAS_NOT_LOADED_TEXT);
        }

        [Test]
        public void EditOKRNameTest()
        {
            EnterEditMode();
            SetInputField(editOKRPageObject.OKRNameField, TEST_OKR_CHANGED_TITLE);
            editOKRPageObject.SaveButton.Click();

            // Checking results
            EnterEditMode();
            Assert.AreEqual(editOKRPageObject.OKRNameField.GetAttribute("value"), TEST_OKR_CHANGED_TITLE);
        }

        [Test]
        public void EditOKRObjectiveTest()
        {
            EnterEditMode();
            SetInputField(editOKRPageObject.ObjectivesList[0].ObjectiveNameField, TEST_OKR_CHANGED_OBJECTIVE);
            editOKRPageObject.SaveButton.Click();

            // Checking results
            EnterEditMode();
            Assert.AreEqual(editOKRPageObject.ObjectivesList[0].ObjectiveNameField.GetAttribute("value"), TEST_OKR_CHANGED_OBJECTIVE, OBJECTIVE_WAS_NOT_EDITED_TEXT);
        }

        [Test]
        public void EditOKRKeyResultTest()
        {
            EnterEditMode();
            SetInputField(editOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField, TEST_OKR_CHANGED_KEY_RESULT_1);
            editOKRPageObject.SaveButton.Click();

            // Checking results
            EnterEditMode();
            Assert.AreEqual(editOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField.GetAttribute("value"), TEST_OKR_CHANGED_KEY_RESULT_1, KEY_RESULT_WAS_NOT_DELETED_TEXT);
        }

        [Test]
        public void DeleteOKRObjectiveTest()
        {
            EnterEditMode();
            var objectiveToDelete = editOKRPageObject.ObjectivesList[0].ObjectiveNameField.Text;
            editOKRPageObject.ObjectivesList[0].RemoveObjectLink.Click();
            editOKRPageObject.SaveButton.Click();

            // Checking results
            EnterEditMode();
            Assert.IsTrue(editOKRPageObject.ObjectivesList.All(i => !i.ObjectiveNameField.GetAttribute("value").Equals(objectiveToDelete)), OBJECTIVE_WAS_NOT_DELETED_TEXT);
        }

        [Test]
        public void DeleteOKRKeyResultTest()
        {
            EnterEditMode();
            var keyResultToDelete = editOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField.Text;
            editOKRPageObject.ObjectivesList[0].KeyResultsList[0].RemoveKeyResultLink.Click();
            editOKRPageObject.SaveButton.Click();

            // Checking results
            EnterEditMode();
            Assert.IsTrue(editOKRPageObject.ObjectivesList[0].KeyResultsList.All(i => !i.KeyResultField.GetAttribute("value").Equals(keyResultToDelete)), KEY_RESULT_WAS_NOT_DELETED_TEXT);
        }

        [Test]
        public void CancelLinkTest()
        {
            EnterEditMode();
            editOKRPageObject.CancelButton.Click();

            Assert.IsTrue(driver.Url.Contains(OKRS_PAGE_LINK), CANCEL_LINT_TEST_FAIL_TEXT);
        }

        [TearDown]
        public void DeleteTestOKR()
        {
            // Force navigating to last created OKR and removing it
            driver.Navigate().GoToUrl(currentTestOKRLink);
            approveOKRPageObject.SettingsButton.Click();
            approveOKRPageObject.SettingsMenu.DeleteMenuItem.Click();
            driver.SwitchTo().Alert().Accept();
            
            // Checking results of TearDown and throwing Exception if OKR was not removed
            NavigateAndSelectUser();
            if (okrListPageObject.OKRsList.Count > 0 && (okrListPageObject.OKRsList.Last().TitleLabel.Text == currentTestOKRName || okrListPageObject.OKRsList.Last().TitleLabel.Text == TEST_OKR_CHANGED_TITLE))
            {
                throw new Exception(TEAR_DOWN_EXCEPTION_TEXT);
            }
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            driver.Quit();
        }

        // This method finds OKR which must be edited and navigates to Edit OKR Page
        public void EnterEditMode()
        {
            SelectTestUser();
            
            // Last created OKR gets last position in OKRs list for selected user
            if (okrListPageObject.OKRsList.Last().TitleLabel.Text == currentTestOKRName || okrListPageObject.OKRsList.Last().TitleLabel.Text == TEST_OKR_CHANGED_TITLE)
            {
                currentTestOKRLink = okrListPageObject.OKRsList.Last().OKRCardLink.GetAttribute("href");

                // Force navigating to OKR's URL and entering Edit Mode
                driver.Navigate().GoToUrl(currentTestOKRLink);
                approveOKRPageObject.SettingsButton.Click();
                approveOKRPageObject.SettingsMenu.EditMenuItem.Click();
                return; 
            }
            throw new Exception(OKR_NOT_FOUND_EXCEPTION_TEXT);
        }

        // This method is used to set an input field
        private void SetInputField(IWebElement field, string text)
        {
            field.Clear();
            field.SendKeys(text);
        }

        // This method can be replaced with SelectElement.SelectByText(TEST_USER_NAME) but it doesn't work in last versions of Firefox
        private void SelectTestUser()
        {
            okrListPageObject.UserListContainer.Click();
            okrListPageObject.UserListSearchBox.Clear();
            okrListPageObject.UserListSearchBox.SendKeys(TEST_USER_NAME);
            okrListPageObject.UserListSearchBox.SendKeys(Keys.Enter);
        }

        // This method is called after removing OKR to avoid Fluxday's redirrection issues
        private void NavigateAndSelectUser()
        {
            driver.Navigate().GoToUrl(OKRS_PAGE_LINK);
            SelectTestUser();
        }
    }
}