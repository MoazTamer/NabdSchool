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
        private void ComposeClassSectionOld(IContainer container, ClassAbsenceViewModel classData, string type)
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

        // قسم كل صف - معدلة لإضافة معاد الحضور
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

                // جدول التأخر RTL - معدل لإضافة معاد الحضور
                column.Item().Table(table =>
                {
                    // الأعمدة (مقلوبة) - إضافة عمود معاد الحضور
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);   // ملاحظات
                        columns.RelativeColumn(2);   // أيام التأخر المتتالية
                        columns.RelativeColumn(2);   // معاد الحضور - جديد
                        columns.RelativeColumn(2);   // الهاتف
                        columns.RelativeColumn(2);   // الكود
                        columns.RelativeColumn(3);   // الاسم
                        columns.ConstantColumn(40);  // #
                    });

                    // Header RTL - إضافة عنوان معاد الحضور
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("ملاحظات").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("أيام " + type + " المتتالية").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("معاد الحضور").FontColor(Colors.White).Bold().FontSize(10); // جديد

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("رقم الهاتف").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("كود الطالب").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("اسم الطالب").FontColor(Colors.White).Bold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .AlignCenter().Text("#").FontColor(Colors.White).Bold().FontSize(10);
                    });

                    // Rows RTL - إضافة معاد الحضور
                    int index = 1;
                    foreach (var st in classData.AbsentStudentsList)
                    {
                        var bgColor = st.ConsecutiveAbsenceDays >= 3
                            ? Colors.Red.Lighten4
                            : (index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White);

                        // تنسيق معاد الحضور
                        string attendanceTimeDisplay = "-";
                        if (st.AttendanceTime.HasValue)
                        {
                            var time = st.AttendanceTime.Value;
                            attendanceTimeDisplay = $"{time.Hours:00}:{time.Minutes:00}";
                        }

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignRight().Text(st.Notes ?? "-").FontSize(9);

                        table.Cell().Background(bgColor).Padding(5)
                            .AlignCenter().Text($"{st.ConsecutiveAbsenceDays} يوم")
                            .FontSize(9)
                            .FontColor(st.ConsecutiveAbsenceDays >= 3 ? Colors.Red.Darken2 : Colors.Black);

                        table.Cell().Background(bgColor).Padding(5) // خلية معاد الحضور الجديدة
                            .AlignCenter().Text(attendanceTimeDisplay)
                            .FontSize(9)
                            .FontColor(Colors.Blue.Darken2)
                            .Bold();

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




        //
        // إنشاء تقرير أكثر الطلاب غياب PDF
        public byte[] GenerateMostAbsentStudentsReport(MostAbsentStudentsReportViewModel data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
                    page.DefaultTextStyle(x => x.DirectionFromRightToLeft());

                    // Header
                    page.Header().Element(container => ComposeMostAbsentHeader(container, data));

                    // Content
                    page.Content().Element(container => ComposeMostAbsentContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // Header لتقرير أكثر الطلاب غياب
        private void ComposeMostAbsentHeader(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.Column(column =>
            {
                column.Spacing(0);

                // الصف الأول - اللوجو والمعلومات
                column.Item().Row(row =>
                {
                    row.Spacing(0);

                    // اللوجو على اليمين
                    row.RelativeItem().AlignRight().Column(logoColumn =>
                    {
                        logoColumn.Spacing(0);
                        logoColumn.Item().PaddingTop(-5).AlignTop().Row(r =>
                        {
                            r.Spacing(0);
                            r.ConstantItem(100).Height(100).AlignTop().Column(c =>
                            {
                                c.Spacing(0);
                                if (File.Exists(_logoPath))
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(100)
                                        .Height(100)
                                        .Image(_logoPath, ImageScaling.FitArea);
                                }
                                else
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(60)
                                        .Height(60)
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("LOGO")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });
                        });
                    });

                    // معلومات المدرسة في الوسط
                    row.RelativeItem(2).AlignCenter().Column(infoColumn =>
                    {
                        infoColumn.Spacing(0);
                        infoColumn.Item().PaddingTop(-5).Text(_schoolName)
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });

                    // تاريخ الطباعة على اليسار
                    row.RelativeItem().AlignLeft().Column(dateColumn =>
                    {
                        dateColumn.Spacing(0);
                        dateColumn.Item().PaddingTop(-5).Text("تاريخ الطباعة")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().Text(DateTime.Now.ToString("dd/MM/yyyy"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().PaddingTop(1).Text("الوقت")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().Text(DateTime.Now.ToString("hh:mm tt"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                    });
                });

                // خط فاصل
                column.Item().PaddingTop(5).PaddingBottom(5)
                    .LineHorizontal(1.5f)
                    .LineColor(Colors.Blue.Darken2);

                // عنوان التقرير
                column.Item().PaddingTop(2).AlignCenter().Text("تقرير أكثر الطلاب غياب")
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Red.Darken2);

                // فترة التقرير
                column.Item().PaddingTop(2).AlignCenter()
                    .Text($"الفترة: {data.PeriodText}")
                    .FontSize(11)
                    .Bold()
                    .FontColor(Colors.Grey.Darken2);

                // معلومات التصفية
                var filterText = "جميع الصفوف والفصول";
                if (data.ClassId.HasValue || data.ClassRoomId.HasValue)
                {
                    filterText = "تصفية حسب: ";
                    if (data.ClassId.HasValue) filterText += $"الصف {data.ClassName} ";
                    if (data.ClassRoomId.HasValue) filterText += $"الفصل {data.ClassRoomName}";
                }

                column.Item().PaddingTop(1).AlignCenter()
                    .Text(filterText)
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                // خط فاصل
                column.Item().PaddingTop(5).PaddingBottom(2)
                    .LineHorizontal(0.8f)
                    .LineColor(Colors.Grey.Lighten1);
            });
        }

        // محتوى تقرير أكثر الطلاب غياب
        private void ComposeMostAbsentContent(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // الإحصائيات العامة
                column.Item().Element(c => ComposeMostAbsentSummary(c, data));

                column.Item().PaddingTop(15);

                // جدول الطلاب
                if (data.Students != null && data.Students.Any())
                {
                    column.Item().Element(c => ComposeMostAbsentTable(c, data));
                }
                else
                {
                    column.Item().AlignCenter().Text("لا توجد بيانات للعرض")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Grey.Medium);
                }
            });
        }

        // الإحصائيات العامة لتقرير أكثر الطلاب غياب
        private void ComposeMostAbsentSummary(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.Background(Colors.Grey.Lighten3)
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("عدد الطلاب")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalStudents.ToString())
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("إجمالي أيام الغياب")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalAbsentDays.ToString())
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Red.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى غياب")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.Students.FirstOrDefault()?.AbsentDays.ToString() ?? "0")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Orange.Darken1);
                    });
                });
        }

        // جدول أكثر الطلاب غياب
        private void ComposeMostAbsentTable(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.Table(table =>
            {
                // تعريف الأعمدة (RTL)
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // النسبة
                    columns.RelativeColumn(1.5f); // أيام الغياب
                    columns.RelativeColumn(2);   // الفصل
                    columns.RelativeColumn(2);   // الصف
                    columns.RelativeColumn(2);   // الكود
                    columns.RelativeColumn(3);   // الاسم
                    columns.ConstantColumn(40);  // الترتيب
                });

                // Header RTL
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("نسبة الغياب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("أيام الغياب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("الفصل").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("الصف").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("كود الطالب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("اسم الطالب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("#").FontColor(Colors.White).Bold().FontSize(10);
                });

                // Rows RTL
                int index = 1;
                foreach (var student in data.Students)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    var percentageColor = student.AbsentPercentage >= 50 ? Colors.Red.Darken2 :
                                        student.AbsentPercentage >= 30 ? Colors.Orange.Darken2 : Colors.Green.Darken2;

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text($"{student.AbsentPercentage}%")
                        .FontSize(9)
                        .FontColor(percentageColor)
                        .Bold();

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(student.AbsentDays.ToString())
                        .FontSize(9)
                        .Bold();

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.ClassRoomName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.ClassName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.StudentCode)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.StudentName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(index.ToString())
                        .FontSize(9)
                        .Bold();

                    index++;
                }
            });
        }


        // إنشاء تقرير أكثر الطلاب تأخر PDF
        public byte[] GenerateMostLateStudentsReport(MostAbsentStudentsReportViewModel data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
                    page.DefaultTextStyle(x => x.DirectionFromRightToLeft());

                    // Header
                    page.Header().Element(container => ComposeMostLateHeader(container, data));

                    // Content
                    page.Content().Element(container => ComposeMostLateContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // Header لتقرير أكثر الطلاب تأخر
        private void ComposeMostLateHeader(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.Column(column =>
            {
                column.Spacing(0);

                // الصف الأول - اللوجو والمعلومات
                column.Item().Row(row =>
                {
                    row.Spacing(0);

                    // اللوجو على اليمين
                    row.RelativeItem().AlignRight().Column(logoColumn =>
                    {
                        logoColumn.Spacing(0);
                        logoColumn.Item().PaddingTop(-5).AlignTop().Row(r =>
                        {
                            r.Spacing(0);
                            r.ConstantItem(100).Height(100).AlignTop().Column(c =>
                            {
                                c.Spacing(0);
                                if (File.Exists(_logoPath))
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(100)
                                        .Height(100)
                                        .Image(_logoPath, ImageScaling.FitArea);
                                }
                                else
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(60)
                                        .Height(60)
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("LOGO")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });
                        });
                    });

                    // معلومات المدرسة في الوسط
                    row.RelativeItem(2).AlignCenter().Column(infoColumn =>
                    {
                        infoColumn.Spacing(0);
                        infoColumn.Item().PaddingTop(-5).Text(_schoolName)
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });

                    // تاريخ الطباعة على اليسار
                    row.RelativeItem().AlignLeft().Column(dateColumn =>
                    {
                        dateColumn.Spacing(0);
                        dateColumn.Item().PaddingTop(-5).Text("تاريخ الطباعة")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().Text(DateTime.Now.ToString("dd/MM/yyyy"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().PaddingTop(1).Text("الوقت")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().Text(DateTime.Now.ToString("hh:mm tt"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                    });
                });

                // خط فاصل
                column.Item().PaddingTop(5).PaddingBottom(5)
                    .LineHorizontal(1.5f)
                    .LineColor(Colors.Blue.Darken2);

                // عنوان التقرير
                column.Item().PaddingTop(2).AlignCenter().Text("تقرير أكثر الطلاب تأخر")
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Orange.Darken2); // لون مختلف للتأخر

                // فترة التقرير
                column.Item().PaddingTop(2).AlignCenter()
                    .Text($"الفترة: {data.PeriodText}")
                    .FontSize(11)
                    .Bold()
                    .FontColor(Colors.Grey.Darken2);

                // معلومات التصفية
                var filterText = "جميع الصفوف والفصول";
                if (data.ClassId.HasValue || data.ClassRoomId.HasValue)
                {
                    filterText = "تصفية حسب: ";
                    if (data.ClassId.HasValue) filterText += $"الصف {data.ClassName} ";
                    if (data.ClassRoomId.HasValue) filterText += $"الفصل {data.ClassRoomName}";
                }

                column.Item().PaddingTop(1).AlignCenter()
                    .Text(filterText)
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                // خط فاصل
                column.Item().PaddingTop(5).PaddingBottom(2)
                    .LineHorizontal(0.8f)
                    .LineColor(Colors.Grey.Lighten1);
            });
        }

        // محتوى تقرير أكثر الطلاب تأخر
        private void ComposeMostLateContent(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // الإحصائيات العامة
                column.Item().Element(c => ComposeMostLateSummary(c, data));

                column.Item().PaddingTop(15);

                // جدول الطلاب
                if (data.Students != null && data.Students.Any())
                {
                    column.Item().Element(c => ComposeMostLateTable(c, data));
                }
                else
                {
                    column.Item().AlignCenter().Text("لا توجد بيانات للعرض")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Grey.Medium);
                }
            });
        }

        // الإحصائيات العامة لتقرير أكثر الطلاب تأخر
        private void ComposeMostLateSummary(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.Background(Colors.Grey.Lighten3)
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("عدد الطلاب")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalStudents.ToString())
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("إجمالي أيام التأخر")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalAbsentDays.ToString())
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Orange.Darken2); // لون مختلف للتأخر
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى تأخر")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.Students.FirstOrDefault()?.AbsentDays.ToString() ?? "0")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.DeepOrange.Darken1);
                    });
                });
        }

        // جدول أكثر الطلاب تأخر
        private void ComposeMostLateTable(IContainer container, MostAbsentStudentsReportViewModel data)
        {
            container.Table(table =>
            {
                // تعريف الأعمدة (RTL)
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // النسبة
                    columns.RelativeColumn(1.5f); // أيام التأخر
                    columns.RelativeColumn(2);   // الفصل
                    columns.RelativeColumn(2);   // الصف
                    columns.RelativeColumn(2);   // الكود
                    columns.RelativeColumn(3);   // الاسم
                    columns.ConstantColumn(40);  // الترتيب
                });

                // Header RTL
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Orange.Darken2).Padding(8) // لون مختلف للتأخر
                        .AlignCenter().Text("نسبة التأخر").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Orange.Darken2).Padding(8)
                        .AlignCenter().Text("أيام التأخر").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Orange.Darken2).Padding(8)
                        .AlignCenter().Text("الفصل").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Orange.Darken2).Padding(8)
                        .AlignCenter().Text("الصف").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Orange.Darken2).Padding(8)
                        .AlignCenter().Text("كود الطالب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Orange.Darken2).Padding(8)
                        .AlignCenter().Text("اسم الطالب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Orange.Darken2).Padding(8)
                        .AlignCenter().Text("#").FontColor(Colors.White).Bold().FontSize(10);
                });

                // Rows RTL
                int index = 1;
                foreach (var student in data.Students)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    var percentageColor = student.AbsentPercentage >= 50 ? Colors.Orange.Darken2 :
                                        student.AbsentPercentage >= 30 ? Colors.Amber.Darken2 : Colors.LightGreen.Darken2;

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text($"{student.AbsentPercentage}%")
                        .FontSize(9)
                        .FontColor(percentageColor)
                        .Bold();

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(student.AbsentDays.ToString())
                        .FontSize(9)
                        .Bold();

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.ClassRoomName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.ClassName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.StudentCode)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(student.StudentName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(index.ToString())
                        .FontSize(9)
                        .Bold();

                    index++;
                }
            });
        }

        // إنشاء تقرير حضور الطالب PDF
        public byte[] GenerateStudentAttendanceReport(StudentAttendanceReportViewModel data)
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
                    page.Header().Element(container => ComposeStudentReportHeader(container, data));

                    // Content
                    page.Content().Element(container => ComposeStudentReportContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // Header لتقرير الطالب
        private void ComposeStudentReportHeader(IContainer container, StudentAttendanceReportViewModel data)
        {
            container.Column(column =>
            {
                column.Spacing(0);

                // الصف الأول - اللوجو والمعلومات
                column.Item().Row(row =>
                {
                    row.Spacing(0);

                    // اللوجو على اليمين
                    row.RelativeItem().AlignRight().Column(logoColumn =>
                    {
                        logoColumn.Spacing(0);
                        logoColumn.Item().PaddingTop(-5).AlignTop().Row(r =>
                        {
                            r.Spacing(0);
                            r.ConstantItem(100).Height(100).AlignTop().Column(c =>
                            {
                                c.Spacing(0);
                                if (File.Exists(_logoPath))
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(100)
                                        .Height(100)
                                        .Image(_logoPath, ImageScaling.FitArea);
                                }
                                else
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(60)
                                        .Height(60)
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("LOGO")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });
                        });
                    });

                    // معلومات المدرسة في الوسط
                    row.RelativeItem(2).AlignCenter().Column(infoColumn =>
                    {
                        infoColumn.Spacing(0);
                        infoColumn.Item().PaddingTop(-5).Text(_schoolName)
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });

                    // تاريخ الطباعة على اليسار
                    row.RelativeItem().AlignLeft().Column(dateColumn =>
                    {
                        dateColumn.Spacing(0);
                        dateColumn.Item().PaddingTop(-5).Text("تاريخ الطباعة")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().Text(DateTime.Now.ToString("dd/MM/yyyy"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().PaddingTop(1).Text("الوقت")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                        dateColumn.Item().Text(DateTime.Now.ToString("hh:mm tt"))
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                    });
                });

                // خط فاصل
                column.Item().PaddingTop(5).PaddingBottom(5)
                    .LineHorizontal(1.5f)
                    .LineColor(Colors.Blue.Darken2);

                // عنوان التقرير
                column.Item().PaddingTop(2).AlignCenter().Text("تقرير حضور الطالبة")
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                // معلومات الطالب
                column.Item().PaddingTop(2).AlignCenter()
                    .Text($"{data.StudentName} - {data.StudentCode}")
                    .FontSize(12)
                    .Bold()
                    .FontColor(Colors.Grey.Darken2);

                // معلومات الفصل
                column.Item().PaddingTop(1).AlignCenter()
                    .Text($"{data.ClassName} - {data.ClassRoomName}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                // فترة التقرير
                var arabicCulture = new System.Globalization.CultureInfo("ar-EG");
                column.Item().PaddingTop(1).AlignCenter()
                .Text($"من تاريخ: {data.FromDate.ToString("dddd، dd MMMM yyyy", arabicCulture)}  حتى تاريخ: {data.ReportDate.ToString("dddd، dd MMMM yyyy", arabicCulture)}")

                    //.Text($"آخر 30 يوم حتى: {data.ReportDate.ToString("dddd، dd MMMM yyyy", arabicCulture)}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                // خط فاصل
                column.Item().PaddingTop(5).PaddingBottom(2)
                    .LineHorizontal(0.8f)
                    .LineColor(Colors.Grey.Lighten1);
            });
        }

        // محتوى تقرير الطالب
        private void ComposeStudentReportContent(IContainer container, StudentAttendanceReportViewModel data)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // الإحصائيات العامة
                column.Item().Element(c => ComposeStudentReportSummary(c, data));

                column.Item().PaddingTop(15);

                // جدول الحضور
                if (data.Days != null && data.Days.Any())
                {
                    column.Item().Element(c => ComposeStudentAttendanceTable(c, data));
                }
                else
                {
                    column.Item().AlignCenter().Text("لا توجد سجلات حضور للعرض")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Grey.Medium);
                }
            });
        }

        // الإحصائيات العامة لتقرير الطالب
        // الإحصائيات العامة لتقرير الطالب - معدلة
        private void ComposeStudentReportSummary(IContainer container, StudentAttendanceReportViewModel data)
        {
            container.Column(column =>
            {
                // الصف الأول - الإحصائيات الرئيسية
                column.Item().Background(Colors.Grey.Lighten3)
                    .Padding(10)
                    .Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("الحضور")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);
                            col.Item().Text(data.TotalPresent.ToString()).AlignCenter()
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Green.Darken2);
                        });

                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("التأخر")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);
                            col.Item().Text(data.TotalLate.ToString()).AlignCenter()
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Orange.Darken2);
                        });

                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("الغياب")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);
                            col.Item().Text(data.TotalAbsent.ToString()).AlignCenter()
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Red.Darken2);
                        });

                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("النسبة الإجمالية")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);
                            var totalDays = data.TotalPresent + data.TotalLate + data.TotalAbsent;
                            var percentage = totalDays > 0 ? Math.Round((data.TotalPresent * 100.0) / totalDays, 1) : 0;
                            col.Item().Text($"{percentage}%").AlignCenter()
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);
                        });
                    });

                // الصف الثاني - المعلومات الإضافية
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى تأخر متتالي")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"{data.ConsecutiveLate} يوم").AlignCenter()
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Orange.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى غياب متتالي")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"{data.ConsecutiveAbsent} يوم").AlignCenter()
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Red.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("إجمالي الأيام")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                        var totalDays = data.TotalPresent + data.TotalLate + data.TotalAbsent;
                        col.Item().Text($"يوم {totalDays}").AlignCenter()
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("معدل الحضور")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                        var totalDays = data.TotalPresent + data.TotalLate + data.TotalAbsent;
                        var percentage = totalDays > 0 ? Math.Round((data.TotalPresent * 100.0) / totalDays, 1) : 0;
                        col.Item().Text($"{percentage}%").AlignCenter()
                            .FontSize(12)
                            .Bold()
                            .FontColor(percentage >= 80 ? Colors.Green.Darken2 :
                                      percentage >= 60 ? Colors.Orange.Darken2 : Colors.Red.Darken2);
                    });
                });
            });
        }
        // جدول حضور الطالب
        private void ComposeStudentAttendanceTable(IContainer container, StudentAttendanceReportViewModel data)
        {
            container.Table(table =>
            {
                // تعريف الأعمدة (RTL)
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // ملاحظات
                    columns.RelativeColumn(2);   // الوقت
                    columns.RelativeColumn(2);   // الحالة
                    columns.RelativeColumn(3);   // التاريخ
                    columns.ConstantColumn(40);  // #
                });

                // Header RTL
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("ملاحظات").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("الوقت").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("الحالة").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("التاريخ").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                        .AlignCenter().Text("#").FontColor(Colors.White).Bold().FontSize(10);
                });

                // Rows RTL
                int index = 1;
                foreach (var day in data.Days)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    var statusColor = day.Status == "حضور" ? Colors.Green.Darken2 :
                                    day.Status == "متأخر" ? Colors.Orange.Darken2 : Colors.Red.Darken2;

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(day.Notes ?? "-")
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(day.Time)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(day.Status)
                        .FontSize(9)
                        .Bold()
                        .FontColor(statusColor);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignRight().Text(day.Date.ToString("yyyy/MM/dd"))
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(index.ToString())
                        .FontSize(9)
                        .Bold();

                    index++;
                }
            });
        }
    }
}