using System;
using System.Collections.Generic;
using System.Linq; 

using Microsoft.Office.Interop.Excel;

namespace Claims.Common.Logging.Reporting
{
    public class ReportBL
    {
        private ReportingDAL _dal = null;
        public ReportBL(string conn)
        {
            _dal = new ReportingDAL(conn);
        }

        public void ReportToExcel(string excelPath, DateTime start, DateTime end)
        {
            var logs = GetYesterdayLogSummary(start, end);


            Microsoft.Office.Interop.Excel.Application a = new Microsoft.Office.Interop.Excel.Application();

            Workbook wb = a.Workbooks.Open(excelPath, Type.Missing, false,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            _Worksheet ws = (_Worksheet)wb.Sheets[1];

            int excelRowIndexer = 0;
            int total = 0;
            string msg = null;
            string app = null;
            Range rng = null;
            #region by box

            IEnumerable<string> boxes = logs.Select(c => c.appPath.Split('\\')[0]).Distinct();
            var grpByBox = logs.Where(l => !string.IsNullOrWhiteSpace(l.msgText)).GroupBy(l => new { box = l.appPath.Split('\\')[0], msg = l.msgText, app = l.appName });

            excelRowIndexer = 0;
            total = 0;
            msg = null;
            app = null;

            //rng = ws.get_Range(" ", " ");

            foreach (var grp in grpByBox)
            {
                Console.WriteLine(grp.Key);
            }

            #endregion

            #region by error message

            var msgs = logs.Select(c => c.msgText).Distinct();
            var grpByMsg = logs.Where(c=>!string.IsNullOrWhiteSpace(c.msgText)).GroupBy(l => new { msg = l.msgText, app = l.appName, stack = l.stackText }).OrderByDescending(c => c.Count());

            rng = ws.get_Range("B6", "E15");

            var rng2 = ws.get_Range("V6", "W15");

            excelRowIndexer = 1;
            total = 0;
            msg = null;
            app = null;
            string stackTxt = null;

            foreach (var grp in grpByMsg)
            {
                if (excelRowIndexer > 10) break;

                total = grp.Count();
                msg = grp.Key.msg;
                app = grp.Key.app;
                stackTxt = grp.Key.stack;

                rng[excelRowIndexer, 2] = msg;
                rng[excelRowIndexer, 3] = app;
                rng[excelRowIndexer, 4] = total;

                rng2[excelRowIndexer, 2] = stackTxt;

                excelRowIndexer++;
            }

            #endregion

            #region by app

            var apps = logs.Select(c => c.appName).Distinct();
            var grpByApp = logs.Where(c => !string.IsNullOrEmpty(c.msgText)).GroupBy(l => new { app = l.appName });

            excelRowIndexer = 1;
            total = 0;
            msg = null;
            app = null;

            string c1 = "O6";
            string c2 = "P" + (6 + grpByApp.Count() + 1).ToString();

            rng = ws.get_Range(c1,c2 );

            foreach (var grp in grpByApp)
            {
                total = grp.Count(); 
                app = grp.Key.app;

                rng[excelRowIndexer, 1] = app;
                rng[excelRowIndexer, 2] = total;

                excelRowIndexer++;
            }
            #endregion

            a.DisplayAlerts = false;
            wb.Save();
            wb.Close(true,null,null);
            a.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(a);
        }

        public List<LogHolder> GetYesterdayLogSummary(DateTime start, DateTime end)
        {
            return _dal.GetReportsForTimeSpan(start, end);
        }
    }
}
