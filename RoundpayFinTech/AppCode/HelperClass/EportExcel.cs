using OfficeOpenXml;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Data;

namespace RoundpayFinTech
{
    public class EportExcel
    {
        private EportExcel()
        {

        }
        private static Lazy<EportExcel> instance = new Lazy<EportExcel>(() => new EportExcel());
        public static EportExcel o => instance.Value;

        public byte[] GetFile(DataTable dataTable,string[] removableCol)
        {
            
            foreach (string str in removableCol)
            {
                if (dataTable.Columns.Contains(str))
                {
                    dataTable.Columns.Remove(str);
                }
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("sheet1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray()
                };
                return exportToExcel.Contents;
            }
        }
    }
}
