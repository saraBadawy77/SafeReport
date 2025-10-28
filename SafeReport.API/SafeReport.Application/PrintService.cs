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
            var document = new iTextSharp.text.Document(PageSize.A4, 36, 36, 36, 36);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();
            bool isArabic = currentCulture == "ar";
            // === Fonts ===
            string fontPath = Path.Combine(env.WebRootPath, "fonts", "arial.ttf");
            BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            Font titleFont = new Font(baseFont, 14, Font.BOLD, BaseColor.WHITE);
            Font labelFont = new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK);
            Font valueFont = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
            // ===== Background Logo =====
            string watermarkPath = Path.Combine(env.WebRootPath, "Logo", "Watermark.png");
            string logoPath = Path.Combine(env.WebRootPath, "Logo", "logo.png");
            AddLogo(document, logoPath);
            // === PAGE 1: Cover Page ===
            AddWatermark(writer, watermarkPath);
            document.Add(new Paragraph("\n\n\n\n\n\n\n\n", valueFont));

            string mainTitle = isArabic ? "تقرير الإبلاغ عن مخالفة" : "Violation Report";

            PdfPTable coverTitleTable = new PdfPTable(1)
            {
                WidthPercentage = 100,
                RunDirection = isArabic ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };

            PdfPCell titleCell = new PdfPCell(new Phrase(mainTitle, new Font(baseFont, 18, Font.BOLD, BaseColor.BLACK)))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 20f
            };

            coverTitleTable.AddCell(titleCell);
            document.Add(coverTitleTable);

            // Divider line
            PdfPTable dividerTable = new PdfPTable(1)
            {
                WidthPercentage = 100,
                RunDirection = isArabic ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };

            PdfPCell dividerCell = new PdfPCell(new Phrase("#############", new Font(baseFont, 16, Font.BOLD, BaseColor.BLACK)))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 20f
            };

            dividerTable.AddCell(dividerCell);
            document.Add(dividerTable);

            // Date
            PdfPTable dateTable = new PdfPTable(1)
            {
                WidthPercentage = 100,
                RunDirection = isArabic ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };

            PdfPCell dateCell = new PdfPCell(new Phrase($"{(isArabic ? "التاريخ" : "Date")}: {DateTime.Now:yyyy/MM/dd}", new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK)))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER
            };

            dateTable.AddCell(dateCell);
            document.Add(dateTable);
            AddPageNumber(writer, document, baseFont, isArabic);
            // Add new page for report details
            document.NewPage();

            AddWatermark(writer, watermarkPath);
            AddLogo(document, logoPath);


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
                    isArabic ? (report.Incident?.NameAr ?? "N/A") : (report.Incident?.NameEn ?? "N/A"),
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
            List<string> imagePaths = report.Images?
    .Select(img => img.ImagePath)
    .Where(p => !string.IsNullOrEmpty(p))
    .ToList() ?? new List<string>();

            int imagesPerPage = 4;
            int totalPages = (int)Math.Ceiling((double)imagePaths.Count / imagesPerPage);

            for (int page = 0; page < totalPages; page++)
            {

                if (page > 0)
                {
                    document.NewPage();
                    AddWatermark(writer, watermarkPath);
                    AddLogo(document, logoPath);
                }

                PdfPTable imageTable = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };
                imageTable.DefaultCell.Border = Rectangle.NO_BORDER;

                var currentImages = imagePaths.Skip(page * imagesPerPage).Take(imagesPerPage).ToList();

                foreach (var path in currentImages)
                {
                    string fullImagePath = Path.Combine(env.WebRootPath, path.Replace('/', Path.DirectorySeparatorChar));

                    if (File.Exists(fullImagePath))
                    {
                        var img = iTextSharp.text.Image.GetInstance(fullImagePath);
                        img.ScaleToFit(250f, 250f);
                        img.Alignment = Element.ALIGN_CENTER;

                        PdfPCell imgCell = new PdfPCell(img, true)
                        {
                            Border = Rectangle.BOX,
                            Padding = 5f,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        imageTable.AddCell(imgCell);
                    }
                    else
                    {
                        PdfPCell placeholder = new PdfPCell(new Phrase(isArabic ? "لم يتم العثور على الصورة" : "Image not found", valueFont))
                        {
                            Border = Rectangle.BOX,
                            Padding = 5f,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        imageTable.AddCell(placeholder);
                    }
                }

                int emptyCells = imagesPerPage - currentImages.Count;
                for (int i = 0; i < emptyCells; i++)
                {
                    PdfPCell emptyCell = new PdfPCell(new Phrase(" "))
                    {
                        Border = Rectangle.NO_BORDER
                    };
                    imageTable.AddCell(emptyCell);
                }

                document.Add(imageTable);
                AddPageNumber(writer, document, baseFont, isArabic);
            }
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

        // === Adds watermark logo in background ===
        private static void AddWatermark(PdfWriter writer, string logoPath)
        {
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
        }

        private static void AddLogo(iTextSharp.text.Document document, string logoPath)
        {

            if (File.Exists(logoPath))
            {
                var logoTop = iTextSharp.text.Image.GetInstance(logoPath);
                logoTop.ScaleAbsolute(200f, 60f);
                logoTop.Alignment = Element.ALIGN_RIGHT;
                document.Add(logoTop);
            }
        }

        private static void AddPageNumber(PdfWriter writer, iTextSharp.text.Document document, BaseFont baseFont, bool isArabic)
        {
            PdfContentByte cb = writer.DirectContent;
            cb.BeginText();

            string text = isArabic
                ? $"الصفحة {writer.PageNumber}"
                : $"Page {writer.PageNumber}";

            float fontSize = 10;
            float y = document.BottomMargin / 2;

            ColumnText ct = new ColumnText(cb);
            Font font = new Font(baseFont, fontSize, Font.NORMAL, BaseColor.GRAY);

            Phrase phrase = new Phrase(text, font);

            if (isArabic)
            {
                ct.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                float xRight = document.PageSize.Width - document.RightMargin;
                ct.SetSimpleColumn(
                    phrase,
                    document.LeftMargin,
                    y,
                    xRight,
                    y + 15,
                    fontSize,
                    Element.ALIGN_LEFT
                );
            }
            else
            {
                float xLeft = document.LeftMargin;
                float xRight = document.PageSize.Width - document.RightMargin;
                ct.SetSimpleColumn(
                    phrase,
                    xLeft,
                    y,
                    xRight,
                    y + 15,
                    fontSize,
                    Element.ALIGN_LEFT
                );
            }

            ct.Go();
            cb.EndText();
        }


    }



}
