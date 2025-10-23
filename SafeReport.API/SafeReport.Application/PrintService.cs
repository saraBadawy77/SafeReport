
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Globalization;
using SafeReport.Core.Models;
namespace SafeReport.Application
{

    public static class PrintService
    {
        public static byte[] GenerateReportPdf(Report report, IncidentType incidentType)
        {
            var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            bool isArabic = currentCulture == "ar";

            string incidentName = isArabic ? (report.Incident?.NameAr ?? "لا يوجد اسم") : (report.Incident?.NameEn ?? "N/A");
            string incidentTypeName = isArabic ? (incidentType?.NameAr ?? "لا يوجد نوع") : (incidentType?.NameEn ?? "N/A");

            using var stream = new MemoryStream();
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(14).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                       
                        col.Item()
                           .AlignCenter()
                           .PaddingBottom(20)
                           .Text($"{incidentName} ({incidentTypeName})")
                           .FontSize(20)
                           .Bold();

                        void AddLabelValue(string labelAr, string labelEn, string value)
                        {
                            string label = isArabic ? labelAr : labelEn;
                            string finalValue = value ?? (isArabic ? "غير متوفر" : "N/A");

                            col.Item().PaddingVertical(5).Row(row =>
                            {
                                row.Spacing(10);

                                if (isArabic)
                                {
                                    row.RelativeItem().AlignRight().Text(text =>
                                    {
                                        text.Span(label).Bold().FontSize(14);
                                        text.Span("   ");
                                        text.Span(finalValue).FontSize(14);
                                    });
                                }
                                else
                                {
                                    row.RelativeItem().AlignLeft().Text(text =>
                                    {
                                        text.Span(label).Bold().FontSize(14);
                                        text.Span("   ");
                                        text.Span(finalValue).FontSize(14);
                                    });
                                }
                            });
                        }

                    
                        AddLabelValue("الوصف:", "Description:", isArabic ? report.DescriptionAr : report.Description);
                        AddLabelValue("رقم الهاتف:", "Phone Number:", report.PhoneNumber);
                        AddLabelValue("العنوان:", "Address:", report.Address);
                        AddLabelValue("تاريخ الإنشاء:", "Created On:", report.CreatedDate.ToLocalTime().ToString("f"));
                       

                        
                        if (!string.IsNullOrEmpty(report.ImagePath))
                        {
                            string imagePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", report.ImagePath.Replace("/", "\\"));
                            if (File.Exists(imagePath))
                            {
                                col.Item().PaddingTop(20).AlignCenter().Element(container =>
                                {
                                    container.Height(400)
                                             .Image(imagePath)
                                             .FitArea();
                                });
                            }
                            else
                            {
                                col.Item().PaddingTop(20).AlignCenter().Text(isArabic ? "الصورة غير متوفرة" : "Image not available")
                                   .FontSize(14)
                                   .Italic();
                            }
                        }
                    });
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }
    }



}
