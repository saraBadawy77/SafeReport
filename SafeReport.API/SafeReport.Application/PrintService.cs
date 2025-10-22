using iTextSharp.text;
using iTextSharp.text.pdf;
using SafeReport.Core.Models;
using System.Globalization;

namespace SafeReport.Application
{
    public static class PrintService
    {
        public static byte[] GenerateReportPdf(Report report, IncidentType incidentType)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 36, 36, 36, 36);
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            BaseFont baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font boldFont = new Font(baseFont, 16, Font.BOLD, BaseColor.Black);


            PdfPTable table = new PdfPTable(5)
            {
                WidthPercentage = 100,
                RunDirection = currentCulture == "ar" ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };
            table.SetWidths(new float[] { 2, 2, 2, 3, 2 });


            AddHeaderCell(table, currentCulture == "ar" ? "اسم التقرير" : "Report Name", baseFont);
            AddHeaderCell(table, currentCulture == "ar" ? "الحادث" : "Incident", baseFont);
            AddHeaderCell(table, currentCulture == "ar" ? "نوع الحادث" : "Incident Type", baseFont);
            AddHeaderCell(table, currentCulture == "ar" ? "تاريخ الإنشاء" : "Created Date", baseFont);
            AddHeaderCell(table, currentCulture == "ar" ? "العنوان" : "Address", baseFont);

            string description = currentCulture == "ar" ? (report.DescriptionAr ?? "N/A") : (report.Description ?? "N/A");
            string incidentName = currentCulture == "ar" ? (report.Incident?.NameAr ?? "N/A") : (report.Incident?.NameEn ?? "N/A");
            string incidentTypeName = currentCulture == "ar" ? (incidentType?.NameAr ?? "N/A") : (incidentType?.NameEn ?? "N/A");
            string address = report.Address ?? "N/A";

            AddCell(table, description, baseFont);
            AddCell(table, incidentName, baseFont);
            AddCell(table, incidentTypeName, baseFont);
            AddCell(table, report.CreatedDate.ToString("yyyy-MM-dd HH:mm"), baseFont);
            AddCell(table, address, baseFont);

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }

        private static void AddHeaderCell(PdfPTable table, string text, BaseFont baseFont)
        {
            var font = new Font(baseFont, 12, Font.BOLD, BaseColor.White);
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = new BaseColor(0, 102, 204),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            };
            table.AddCell(cell);
        }

        private static void AddCell(PdfPTable table, string text, BaseFont baseFont)
        {
            var font = new Font(baseFont, 11);
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 6
            };
            table.AddCell(cell);
        }
    }
}
