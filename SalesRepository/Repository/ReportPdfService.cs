using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SalesModel.ViewModels.Reports;



namespace SalesRepository.Repository
{
    public class ReportPdfService
    {
        private readonly string _schoolName = "نبض المدرسة";
        private readonly string _schoolAddress = "العنوان الكامل للمدرسة";
        private readonly string _schoolPhone = "0123456789";
        private readonly string _logoPath = "wwwroot/images/school-logo.png";


        public ReportPdfService()
        {
            // تفعيل الترخيص المجاني للاستخدام التجريبي
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // إنشاء تقرير الغياب اليومي PDF
        public byte[] GenerateDailyAbsenceReport(DailyAbsenceReportViewModel data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
                    page.DefaultTextStyle(x => x.DirectionFromRightToLeft());


                    // Header
                    page.Header().Element(container => ComposeHeader(container, data.ReportDate, "تقرير الغياب اليومي"));

                    // Content
                    page.Content().Element(container => ComposeContent(container, data, "الغائبين"));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }
        public byte[] GenerateDailyLateReport(DailyAbsenceReportViewModel data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
                    page.DefaultTextStyle(x => x.DirectionFromRightToLeft());


                    // Header
                    page.Header().Element(container => ComposeHeader(container, data.ReportDate, "تقرير المتأخرين اليومي"));

                    // Content
                    page.Content().Element(container => ComposeContent(container, data, "التأخر"));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // تكوين Header التقرير

        private void ComposeHeader(IContainer container, DateTime reportDate, string reportTitle)
        {
            container.Column(column =>
            {
                // إزالة البادنج من العمود الرئيسي
                column.Spacing(0); // هذا مهم لإزالة المسافات

                // الصف الأول - اللوجو والمعلومات
                column.Item().Row(row =>
                {
                    row.Spacing(0); // إزالة المسافات بين عناصر الصف

                    // اللوجو على اليمين
                    row.RelativeItem().AlignRight().Column(logoColumn =>
                    {
                        logoColumn.Spacing(0); // إزالة المسافات في عمود اللوجو

                        logoColumn.Item().PaddingTop(-5).AlignTop().Row(r => // تقليل البادنج العلوي
                        {
                            r.Spacing(0);
                            r.ConstantItem(100).Height(100).AlignTop().Column(c => // تقليل حجم اللوجو
                            {
                                c.Spacing(0);
                                if (File.Exists(_logoPath))
                                {
                                    c.Item().PaddingTop(-2) // تقليل البادنج
                                        .Width(100) // تصغير العرض
                                        .Height(100) // تصغير الارتفاع
                                        .Image(_logoPath, ImageScaling.FitArea);
                                }
                                else
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(60) // تصغير العرض
                                        .Height(60) // تصغير الارتفاع
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("LOGO")
                                        .FontSize(8) // تصغير الخط
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });
                        });
                    });

                    // معلومات المدرسة في الوسط
                    row.RelativeItem(2).AlignCenter().Column(infoColumn =>
                    {
                        infoColumn.Spacing(0); // إزالة المسافات

                        infoColumn.Item().PaddingTop(-5).Text(_schoolName) // تقليل البادنج العلوي
                            .FontSize(18) // تصغير الخط قليلاً
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        // يمكنك إضافة المعلومات الأخرى هنا مع بادنج أقل
                        // infoColumn.Item().PaddingTop(2).Text(_schoolAddress)
                        //     .FontSize(9)
                        //     .FontColor(Colors.Grey.Darken1);
                    });

                    // تاريخ الطباعة على اليسار
                    row.RelativeItem().AlignLeft().Column(dateColumn =>
                    {
                        dateColumn.Spacing(0); // إزالة المسافات

                        dateColumn.Item().PaddingTop(-5).Text("تاريخ الطباعة") // تقليل البادنج
                            .FontSize(8) // تصغير الخط
                            .FontColor(Colors.Grey.Darken1).AlignRight();

                        dateColumn.Item().Text(DateTime.Now.ToString("dd/MM/yyyy"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();

                        dateColumn.Item().PaddingTop(1).Text("الوقت") // تقليل البادنج
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();

                        dateColumn.Item().Text(DateTime.Now.ToString("hh:mm tt"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                    });
                });

                // خط فاصل - تقليل المسافات حوله
                column.Item().PaddingTop(5).PaddingBottom(5) // تقليل من 10 إلى 5
                    .LineHorizontal(1.5f) // جعل الخط أرفع قليلاً
                    .LineColor(Colors.Blue.Darken2);

                // عنوان التقرير - تقليل البادنج
                column.Item().PaddingTop(2).AlignCenter().Text(reportTitle) // تقليل البادنج العلوي
                    .FontSize(16) // تصغير الخط قليلاً
                    .Bold()
                    .FontColor(Colors.Red.Darken2);

                // تاريخ التقرير - تقليل البادنج
                var arabicCulture = new System.Globalization.CultureInfo("ar-EG");

                column.Item().PaddingTop(2).AlignCenter() // تقليل البادنج
                    .Text($"التاريخ: {reportDate.ToString("dddd، dd MMMM yyyy", arabicCulture)}")
                    .FontSize(11) // تصغير الخط
                    .Bold()
                    .FontColor(Colors.Grey.Darken2);

                // خط فاصل - تقليل المسافات
                column.Item().PaddingTop(5).PaddingBottom(2) // تقليل المسافات
                    .LineHorizontal(0.8f) // خط أرفع
                    .LineColor(Colors.Grey.Lighten1);
            });
        }
        // تكوين محتوى التقرير
        private void ComposeContent(IContainer container, DailyAbsenceReportViewModel data, string type)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // الإحصائيات العامة
                column.Item().Element(c => ComposeSummary(c, data, type));

                column.Item().PaddingTop(15);

                // بيانات كل صف
                if (data.ClassesAbsence != null && data.ClassesAbsence.Any())
                {
                    foreach (var classData in data.ClassesAbsence)
                    {
                        column.Item().Element(c => ComposeClassSection(c, classData,type));
                        column.Item().PaddingTop(15);
                    }
                }
                else
                {
                    column.Item().AlignCenter().Text("لا يوجد طلاب "+type+" في هذا اليوم 🎉")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Green.Darken1);
                }
            });
        }

        // الإحصائيات العامة
        private void ComposeSummary(IContainer container, DailyAbsenceReportViewModel data, string type)
        {
            container.Background(Colors.Grey.Lighten3)
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("إجمالي الطلاب "+ type)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalAbsentStudents.ToString()).AlignRight()
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Red.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("عدد الفصول")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalClasses.ToString()).AlignRight()
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Orange.Darken1);
                    });
                });
        }

        // قسم كل صف
        private void ComposeClassSection(IContainer container, ClassAbsenceViewModel classData, string type)
        {
            container.Column(column =>
            {
                // عنوان الصف
                column.Item().Background(Colors.Blue.Lighten3)
                    .Padding(8)
                    .Row(row =>
                    {

                        row.ConstantItem(300).AlignLeft().Text(
                            $"النسبة: {classData.AbsencePercentage}% | " +
                            $"{type}: {classData.AbsentStudents} | " +
                            $"إجمالي الطلاب: {classData.TotalStudents}")
                            .FontSize(10)
                            .AlignRight()
                            .FontColor(Colors.Red.Darken2);

                        row.RelativeItem().AlignRight().Text($"{classData.ClassName} - {classData.ClassRoomName}")
                           .FontSize(14)
                           .Bold()
                           .FontColor(Colors.Blue.Darken3);
                    });

                // جدول الغياب RTL
                column.Item().Table(table =>
                {

                    // الأعمدة (مقلوبة)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);   // ملاحظات
                        columns.RelativeColumn(2);   // أيام الغياب
                        columns.RelativeColumn(2);   // الهاتف
                        columns.RelativeColumn(2);   // الكود
                        columns.RelativeColumn(3);   // الاسم
                        columns.ConstantColumn(40);  // #
                    });

                    // Header RTL
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("ملاحظات").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("أيام "+type+" المتتالية").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("رقم الهاتف").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("كود الطالب").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("اسم الطالب").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("#").FontColor(Colors.White).Bold().FontSize(10);
                    });

                    // Rows RTL
                    int index = 1;
                    foreach (var st in classData.AbsentStudentsList)
                    {
                        var bgColor = st.ConsecutiveAbsenceDays >= 3
                            ? Colors.Red.Lighten4
                            : (index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White);

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignRight().Text(st.Notes ?? "-").FontSize(9);

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignCenter().Text($"{st.ConsecutiveAbsenceDays} يوم")
                            .FontSize(9)
                            .FontColor(st.ConsecutiveAbsenceDays >= 3 ? Colors.Red.Darken2 : Colors.Black);

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignRight().Text(st.StudentPhone ?? "-").FontSize(9);

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignRight().Text(st.StudentCode ?? "-").FontSize(9);

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignRight().Text(st.StudentName).FontSize(9);

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignCenter().Text(index.ToString()).FontSize(9);

                        index++;
                    }
                });
            });
        }

        // Footer التقرير
        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().AlignLeft()
                        .Text(text =>
                        {
                            text.Span("صفحة ")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                            text.CurrentPageNumber()
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                            text.Span(" من ")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                            text.TotalPages()
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                    //row.RelativeItem().AlignCenter()
                    //    .Text("تم إنشاء هذا التقرير بواسطة نظام إدارة حضور الطلاب")
                    //    .FontSize(8)
                    //    .FontColor(Colors.Grey.Medium);

                    //row.RelativeItem().AlignRight()
                    //    .Text(DateTime.Now.ToString("yyyy"))
                    //    .FontSize(9)
                    //    .FontColor(Colors.Grey.Darken1);
                });
            });
        }
    }
}