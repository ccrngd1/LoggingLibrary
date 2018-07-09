using System;
using System.Collections.Generic;

using BaseLogging.Reporting;

using Microsoft.VisualStudio.TestTools.UnitTesting; 


namespace BaseLogging.UnitTests
{
    [TestClass]
    public class ReportingIntegrationTest
    {
        private string conn = @"Data Source=louprsql063a.zcloud.com;Initial Catalog=ClaimsAuditWarehouse;Persist Security Info=True;Integrated Security=SSPI;Connection Timeout=360";
        private string testXLS = @"C:\Users\nick.lawson\Desktop\ErrorLogReportSample - Copy.xlsx";

#if DEBUG
        [TestMethod]
#endif
        public void GetReportData()
        {
            var dal = new ReportingDAL(conn);

            List<LogHolder> logs = dal.GetReportsForTimeSpan(DateTime.Today.AddDays(-4), DateTime.Today.AddDays(1));

            Assert.IsNotNull(logs);
            Assert.IsTrue(logs.Count>0);
        }

#if DEBUG
        [TestMethod]
#endif 
        public void ExcelTest()
        {
            try
            {
                var bl = new ReportBL(conn);

                bl.ReportToExcel(testXLS, DateTime.Today.AddDays(-4), DateTime.Today.AddDays(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
