using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace FluxDayAutomation.PageObjects
{
    public class DeleteTaskPageObject
    {
        private const string SETTINGS_ICON_XPATH = "//*[@id=\"pane3\"]/div[2]/div/div[1]/div[1]/div/a[2]";
        private const string DROPDOWN_LIST_XPATH = "//*[@id=\"pane3\"]/div[2]/div/div[1]/div[1]/div";
        private const string EXECUTE_SCRIPT = "return (document.readyState == 'complete' && jQuery.active == 0)";
        private const int EXPLICIT_WAIT_SECONDS = 10;
        private IWebDriver driver;

        public DeleteTaskPageObject(IWebDriver driver)
        {
            this.driver = driver;
        }

        public IWebElement SettingsIcon
        {
            get
            {
                // Waiting until JS and jQuery load is completed
                var javaScriptExecutor = driver as IJavaScriptExecutor;

                if (javaScriptExecutor != null)
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(EXPLICIT_WAIT_SECONDS));
                    wait.Until(driver => (bool)javaScriptExecutor.ExecuteScript(EXECUTE_SCRIPT));
                }

                return driver.FindElement(By.XPath(SETTINGS_ICON_XPATH));
            }
        }

        public DeleteTaskMenuPageObject DropdownList
        {
            get
            {
                return new DeleteTaskMenuPageObject(driver.FindElement(By.XPath(DROPDOWN_LIST_XPATH)));
            }
        }
    }
}