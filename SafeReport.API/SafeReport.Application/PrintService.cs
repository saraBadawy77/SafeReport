using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using SafeReport.Core.Models;
using System.Globalization;

namespace SafeReport.Application
{

    public static class PrintService
    {
        public static byte[] GenerateReportPdf(Report report, IncidentType incidentType, IWebHostEnvironment env)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 36, 36, 36, 36);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();
            bool isArabic = currentCulture == "ar";
            // === Fonts ===
            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font titleFont = new Font(baseFont, 14, Font.BOLD, BaseColor.WHITE);
            Font labelFont = new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK);
            Font valueFont = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);

            // ===== Background Logo =====
            string logoPath = Path.Combine(env.WebRootPath, "Logo", "logo.png");

            if (File.Exists(logoPath))
            {
                var logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(550, 550);
                logo.GrayFill = 5.20f;


                float x = (PageSize.A4.Width - logo.ScaledWidth) / 2;
                float y = (PageSize.A4.Height - logo.ScaledHeight) / 2;

                logo.SetAbsolutePosition(x, y);

                PdfContentByte under = writer.DirectContentUnder;


                PdfGState gstate = new PdfGState();
                gstate.FillOpacity = 0.5f;
                gstate.StrokeOpacity = 0.5f;

                under.SaveState();
                under.SetGState(gstate);
                under.AddImage(logo);
                under.RestoreState();
            }


            // === Title Header ===
            PdfPTable headerTable = new PdfPTable(1)
            {
                WidthPercentage = 100,
                RunDirection = isArabic ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };

            string titleText = isArabic ? "معلومات البلاغ" : "Report Details";
            PdfPCell headerCell = new PdfPCell(new Phrase(titleText, titleFont))
            {
                BackgroundColor = new BaseColor(46, 139, 87),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 10
            };
            headerTable.AddCell(headerCell);
            document.Add(headerTable);
            document.Add(new Paragraph("\n"));


            // ===== Report Details Table =====
            PdfPTable detailsTable = new PdfPTable(2)
            {
                WidthPercentage = 100,
                RunDirection = isArabic ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };
            detailsTable.DefaultCell.RunDirection = isArabic ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
            if (isArabic)
                detailsTable.SetWidths(new float[] { 3, 1 });
            else
                detailsTable.SetWidths(new float[] { 1, 3 });


            AddRow(detailsTable, isArabic ? "تصنيف البلاغ" : "Incident Category",
                    isArabic ? (report.Incident?.NameAr ?? "N/A") : (report.Incident ?.NameEn ?? "N/A"),
                    labelFont, valueFont, isArabic);

            AddRow(detailsTable, isArabic ? "نوع البلاغ" : "Incident Type",
                    isArabic ? (incidentType?.NameAr ?? "N/A") : (incidentType?.NameEn ?? "N/A"),
                    labelFont, valueFont, isArabic);

            AddRow(detailsTable, isArabic ? "الوصف" : "Description",
                    isArabic ? (report.DescriptionAr ?? "N/A") : (report.Description ?? "N/A"),
                    labelFont, valueFont, isArabic);

            AddRow(detailsTable, isArabic ? "الإحداثيات" : "Address",
                    report.Address ?? "N/A", labelFont, valueFont, isArabic);

            AddRow(detailsTable, isArabic ? "التاريخ" : "Date",
                    report.CreatedDate.ToString("yyyy/MM/dd"), labelFont, valueFont, isArabic);

            AddRow(detailsTable, isArabic ? "رقم الهاتف" : "Phone Number",
                    "01200000000", labelFont, valueFont, isArabic);

            document.Add(detailsTable);
            document.Add(new Paragraph("\n"));


            // === Image Section ===

            PdfPTable imagesTable = new PdfPTable(2)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };

            imagesTable.DefaultCell.Border = Rectangle.NO_BORDER;

            if (!string.IsNullOrEmpty(report.ImagePath))
            {

                string fullImagePath = Path.Combine(env.WebRootPath, report.ImagePath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(fullImagePath))
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(fullImagePath);
                    img.ScaleToFit(400f, 400f);
                    img.SpacingBefore = 50f;
                    img.SpacingAfter = 50f;
                    img.Alignment = Element.ALIGN_CENTER;


                    PdfPTable imageBox = new PdfPTable(1)
                    {
                        WidthPercentage = 100,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    PdfPCell imgCell = new PdfPCell(img, true)
                    {
                        Border = Rectangle.BOX,
                        Padding = 5f,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    imageBox.AddCell(imgCell);
                    document.Add(imageBox);
                }
                else
                {
                    imagesTable.AddCell(new PdfPCell(new Phrase(isArabic ? "لم يتم العثور على الصورة" : "Image not found", valueFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 10f
                    });
                }
            }
            else
            {
                imagesTable.AddCell(new PdfPCell(new Phrase(isArabic ? "لا توجد صور مرفقة" : "No images attached", valueFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 10f
                });
            }

            document.Add(imagesTable);

            document.Close();
            return memoryStream.ToArray();
        }
        private static void AddRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont, bool isArabic)
        {
            if (isArabic)
            {

                PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    RunDirection = PdfWriter.RUN_DIRECTION_RTL,
                    Padding = 3
                };

                PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    RunDirection = PdfWriter.RUN_DIRECTION_RTL,
                    Padding = 6
                };

                table.AddCell(labelCell);
                table.AddCell(valueCell);
            }
            else
            {

                PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };

                PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };

                table.AddCell(labelCell);
                table.AddCell(valueCell);
            }
        }

    }



}
