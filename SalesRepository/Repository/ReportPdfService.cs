using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SalesModel.ViewModels;
using SalesModel.ViewModels.Reports;
using System.Drawing;



namespace SalesRepository.Repository
{
    public class ReportPdfService
    {
        private readonly string _schoolName = "نبض المدرسة";
        private readonly string _logoPath = "wwwroot/images/school-logo-bg.png";


        // إضافة هذه المتغيرات في الكلاس
        private readonly string _ministryLogoPath = "wwwroot/images/ministry-of-education-saudi-arabia-bg.png";
        private readonly string _schoolLogoPath = "wwwroot/images/school-logo-bg.png";


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

        private void OldComposeHeader(IContainer container, DateTime reportDate, string reportTitle)
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

        // تكوين Header التقرير بشكل رسمي
        private void ComposeHeader(IContainer container, DateTime reportDate, string reportTitle)
        {
            container.Column(column =>
            {
                column.Spacing(0);

                // الصف الأول: الشعارات والمعلومات الأساسية
                column.Item().Row(row =>
                {
                    row.Spacing(0);

                    // شعار المدرسة على اليسار

                    row.RelativeItem().AlignLeft().Column(schoolColumn =>
                    {
                        schoolColumn.Spacing(0);
                        schoolColumn.Item().PaddingTop(-5).AlignTop().Row(r =>
                        {
                            r.Spacing(0);
                            r.ConstantItem(70).Height(70).AlignTop().Column(c =>
                            {
                                c.Spacing(0);
                                if (File.Exists(_schoolLogoPath)) // مسار شعار المدرسة
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(70)
                                        .Height(70)
                                        .Image(_schoolLogoPath, ImageScaling.FitArea);
                                }
                                else
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(50)
                                        .Height(50)
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("شعار المدرسة")
                                        .FontSize(7)
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });
                        });

                    });

                    // المعلومات الرسمية في المنتصف
                    row.RelativeItem(2).AlignCenter().Column(infoColumn =>
                    {
                        infoColumn.Spacing(0);

                        // المملكة العربية السعودية
                        infoColumn.Item().PaddingTop(-5).Text("المملكة العربية السعودية")
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Green.Darken2); // اللون الأخضر الرسمي

                        // وزارة التعليم
                        infoColumn.Item().PaddingTop(2).Text("وزارة التعليم")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.Black).AlignRight();

                        // إدارة التعليم بالطائف
                        infoColumn.Item().PaddingTop(1).Text("إدارة التعليم بالطائف")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Grey.Darken2).AlignRight();

                        // المتوسطة الخامسة عشر
                        infoColumn.Item().PaddingTop(1).Text("المتوسطة الخامسة عشر")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken1).AlignRight();
                    });

                    // شعار الوزارة على اليمين
                    row.RelativeItem().AlignRight().Column(ministryColumn =>
                    {
                        ministryColumn.Spacing(0);
                        ministryColumn.Item().PaddingTop(-5).AlignTop().Row(r =>
                        {
                            r.Spacing(0);
                            r.ConstantItem(80).Height(80).AlignTop().Column(c =>
                            {
                                c.Spacing(0);
                                if (File.Exists(_ministryLogoPath)) // مسار شعار الوزارة
                                {
                                    c.Item().PaddingTop(-2)
                                        .Width(80)
                                        .Height(80)
                                        .Image(_ministryLogoPath, ImageScaling.FitArea);
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
                                        .Text("شعار الوزارة")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });
                        });
                    });

                });

                // خط فاصل - اللون الأخضر الرسمي
                column.Item().PaddingTop(10).PaddingBottom(8)
                    .LineHorizontal(2f)
                    .LineColor(Colors.Green.Darken2);

                // عنوان التقرير
                column.Item().PaddingTop(5).AlignCenter().Text(reportTitle)
                    .FontSize(18)
                    .Bold()
                    .FontColor(Colors.Blue.Darken3);

                // تاريخ التقرير
                var arabicCulture = new System.Globalization.CultureInfo("ar-SA");
                column.Item().PaddingTop(3).AlignCenter()
                    .Text($"تاريخ التقرير: {reportDate.ToString("dddd، dd MMMM yyyy", arabicCulture)}")
                    .FontSize(12)
                    .Bold()
                    .FontColor(Colors.Grey.Darken2);

                // خط فاصل خفيف
                column.Item().PaddingTop(8).PaddingBottom(5)
                    .LineHorizontal(1f)
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

        
        // قسم كل صف - لإضافة معاد الحضور
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
                            .AlignCenter().Text("رقم الجوال").FontColor(Colors.White).Bold().FontSize(10);

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
                column.Item().LineHorizontal(1).LineColor(Colors.Green.Darken2);

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

                    row.RelativeItem().AlignCenter()
                        .Text("إدارة التعليم بالطائف - المتوسطة الخامسة عشر")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);

                    row.RelativeItem().AlignRight()
                        .Text($"تاريخ النظام: {DateTime.Now.ToString("yyyy/MM/dd")}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
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
                    page.Header().Element(container => ComposeHeader(container, DateTime.Now, "تقرير أكثر الطلاب غيابا"));

                    // Content
                    page.Content().Element(container => ComposeMostAbsentContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
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
                            .FontColor(Colors.Blue.Darken2).AlignCenter();
                    });


                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى غياب")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.Students.FirstOrDefault()?.AbsentDays.ToString() ?? "0")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Orange.Darken1).AlignCenter();
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
                    page.Header().Element(container => ComposeHeader(container,DateTime.Now, "تقرير أكثر الطلاب تأخرا"));

                    // Content
                    page.Content().Element(container => ComposeMostLateContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
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
                            .FontColor(Colors.Blue.Darken2).AlignCenter();
                    });


                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى تأخر")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.Students.FirstOrDefault()?.AbsentDays.ToString() ?? "0")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.DeepOrange.Darken1).AlignCenter();
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
                    page.Header().Element(container => ComposeHeader(container, DateTime.Now, "تقرير حضور الطالبة"));

                    // Content
                    page.Content().Element(container => ComposeStudentReportContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
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
                            var totalDays = data.TotalPresent + data.TotalAbsent;
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
                        var totalDays = data.TotalPresent + data.TotalAbsent;
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
                        var totalDays = data.TotalPresent + data.TotalAbsent;
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


        // إنشاء تقرير الطلاب الأكثر انضباطاً PDF
        public byte[] GenerateMostDisciplinedStudentsReport(MostDisciplinedStudentsReportViewModel data)
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
                    page.Header().Element(container => ComposeHeader(container, DateTime.Now, "تقرير الطلاب الأكثر انضباطاً"));

                    // Content
                    page.Content().Element(container => ComposeMostDisciplinedStudentsContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // محتوى تقرير الطلاب الأكثر انضباطاً
        private void ComposeMostDisciplinedStudentsContent(IContainer container, MostDisciplinedStudentsReportViewModel data)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // الإحصائيات العامة
                column.Item().Element(c => ComposeMostDisciplinedStudentsSummary(c, data));

                column.Item().PaddingTop(15);

                // جدول الطلاب
                if (data.Students != null && data.Students.Any())
                {
                    column.Item().Element(c => ComposeMostDisciplinedStudentsTable(c, data));
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

        // الإحصائيات العامة لتقرير الطلاب الأكثر انضباطاً
        private void ComposeMostDisciplinedStudentsSummary(IContainer container, MostDisciplinedStudentsReportViewModel data)
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
                            .FontColor(Colors.Blue.Darken2)
                            .AlignCenter();
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("إجمالي أيام الحضور")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalPresentDays.ToString())
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Green.Darken2)
                            .AlignCenter();
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى نسبة انضباط")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"{data.Students.FirstOrDefault()?.DisciplinePercentage ?? 0}%")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Green.Darken2)
                            .AlignCenter();
                    });
                });
        }

        // جدول الطلاب الأكثر انضباطاً
        private void ComposeMostDisciplinedStudentsTable(IContainer container, MostDisciplinedStudentsReportViewModel data)
        {
            container.Table(table =>
            {
                // تعريف الأعمدة (RTL)
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // نسبة الانضباط
                    columns.RelativeColumn(1.5f); // أيام الحضور
                    columns.RelativeColumn(1.2f); // أيام التأخر
                    columns.RelativeColumn(1.2f); // أيام الغياب
                    columns.RelativeColumn(2);   // الفصل
                    columns.RelativeColumn(2);   // الصف
                    columns.RelativeColumn(2);   // الكود
                    columns.RelativeColumn(3);   // الاسم
                    columns.ConstantColumn(40);  // الترتيب
                });

                // Header RTL
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("نسبة الانضباط").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("أيام الحضور").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("أيام التأخر").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("أيام الغياب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("الفصل").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("الصف").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("كود الطالب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("اسم الطالب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("#").FontColor(Colors.White).Bold().FontSize(10);
                });

                // Rows RTL
                int index = 1;
                foreach (var student in data.Students)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    var percentageColor = student.DisciplinePercentage >= 90 ? Colors.Green.Darken2 :
                                        student.DisciplinePercentage >= 80 ? Colors.LightGreen.Darken2 :
                                        student.DisciplinePercentage >= 70 ? Colors.Yellow.Darken2 :
                                        student.DisciplinePercentage >= 60 ? Colors.Orange.Darken2 : Colors.Red.Darken2;

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text($"{student.DisciplinePercentage}%")
                        .FontSize(9)
                        .FontColor(percentageColor)
                        .Bold();

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(student.PresentDays.ToString())
                        .FontSize(9)
                        .Bold()
                        .FontColor(Colors.Green.Darken2);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(student.LateDays.ToString())
                        .FontSize(9)
                        .FontColor(Colors.Orange.Darken2);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(student.AbsentDays.ToString())
                        .FontSize(9)
                        .FontColor(Colors.Red.Darken2);

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

        // إنشاء تقرير الفصول الأكثر انضباطاً PDF
        public byte[] GenerateMostDisciplinedClassesReport(MostDisciplinedClassesReportViewModel data)
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
                    page.Header().Element(container => ComposeHeader(container, DateTime.Now, "تقرير الفصول الأكثر انضباطاً"));

                    // Content
                    page.Content().Element(container => ComposeMostDisciplinedClassesContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // محتوى تقرير الفصول الأكثر انضباطاً
        private void ComposeMostDisciplinedClassesContent(IContainer container, MostDisciplinedClassesReportViewModel data)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // الإحصائيات العامة
                column.Item().Element(c => ComposeMostDisciplinedClassesSummary(c, data));

                column.Item().PaddingTop(15);

                // جدول الفصول
                if (data.Classes != null && data.Classes.Any())
                {
                    column.Item().Element(c => ComposeMostDisciplinedClassesTable(c, data));
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

        // الإحصائيات العامة لتقرير الفصول الأكثر انضباطاً
        private void ComposeMostDisciplinedClassesSummary(IContainer container, MostDisciplinedClassesReportViewModel data)
        {
            container.Background(Colors.Grey.Lighten3)
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("عدد الفصول")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalClasses.ToString())
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2)
                            .AlignCenter();
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("إجمالي الطلاب")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(data.TotalStudents.ToString())
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Green.Darken2)
                            .AlignCenter();
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("أعلى نسبة انضباط")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"{data.Classes.FirstOrDefault()?.DisciplinePercentage ?? 0}%")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Green.Darken2)
                            .AlignCenter();
                    });
                });
        }

        // جدول الفصول الأكثر انضباطاً
        private void ComposeMostDisciplinedClassesTable(IContainer container, MostDisciplinedClassesReportViewModel data)
        {
            container.Table(table =>
            {
                // تعريف الأعمدة (RTL)
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // نسبة الانضباط
                    //columns.RelativeColumn(1.5f); // أيام الحضور
                    //columns.RelativeColumn(1.2f); // أيام التأخر
                    //columns.RelativeColumn(1.2f); // أيام الغياب
                    columns.RelativeColumn(2);   // عدد الطلاب
                    columns.RelativeColumn(3);   // اسم الفصل
                    columns.RelativeColumn(2);   // اسم الصف
                    columns.ConstantColumn(40);  // الترتيب
                });

                // Header RTL
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("نسبة الانضباط").FontColor(Colors.White).Bold().FontSize(10);

                    //header.Cell().Background(Colors.Green.Darken2).Padding(8)
                    //    .AlignCenter().Text("أيام الحضور").FontColor(Colors.White).Bold().FontSize(10);

                    //header.Cell().Background(Colors.Green.Darken2).Padding(8)
                    //    .AlignCenter().Text("أيام التأخر").FontColor(Colors.White).Bold().FontSize(10);

                    //header.Cell().Background(Colors.Green.Darken2).Padding(8)
                    //    .AlignCenter().Text("أيام الغياب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("عدد الطلاب").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("اسم الفصل").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("اسم الصف").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Green.Darken2).Padding(8)
                        .AlignCenter().Text("#").FontColor(Colors.White).Bold().FontSize(10);
                });

                // Rows RTL
                foreach (var classData in data.Classes)
                {
                    var bgColor = classData.Rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    var percentageColor = classData.DisciplinePercentage >= 90 ? Colors.Green.Darken2 :
                                        classData.DisciplinePercentage >= 80 ? Colors.LightGreen.Darken2 :
                                        classData.DisciplinePercentage >= 70 ? Colors.Yellow.Darken2 :
                                        classData.DisciplinePercentage >= 60 ? Colors.Orange.Darken2 : Colors.Red.Darken2;

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text($"{classData.DisciplinePercentage}%")
                        .FontSize(9)
                        .FontColor(percentageColor)
                        .Bold();

                    //table.Cell().Background(bgColor).Padding(6)
                    //    .AlignCenter().Text(classData.TotalPresentDays.ToString())
                    //    .FontSize(9)
                    //    .Bold()
                    //    .FontColor(Colors.Green.Darken2);

                    //table.Cell().Background(bgColor).Padding(6)
                    //    .AlignCenter().Text(classData.TotalLateDays.ToString())
                    //    .FontSize(9)
                    //    .FontColor(Colors.Orange.Darken2);

                    //table.Cell().Background(bgColor).Padding(6)
                    //    .AlignCenter().Text(classData.TotalAbsentDays.ToString())
                    //    .FontSize(9)
                    //    .FontColor(Colors.Red.Darken2);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(classData.TotalStudents.ToString())
                        .FontSize(9)
                        .Bold();

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(classData.ClassRoomName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(classData.ClassName)
                        .FontSize(9);

                    table.Cell().Background(bgColor).Padding(6)
                        .AlignCenter().Text(classData.Rank.ToString())
                        .FontSize(9)
                        .Bold();
                }
            });
        }


        // إنشاء تقرير الخروج المبكر PDF
        public byte[] GenerateDailyEarlyExitReport(DailyEarlyExitReportViewModel data)
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
                    page.Header().Element(container => ComposeHeader(container, data.ReportDate, "تقرير الخروج المبكر (استئذان)"));

                    // Content
                    page.Content().Element(container => ComposeEarlyExitContent(container, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // تكوين محتوى تقرير الخروج المبكر
        private void ComposeEarlyExitContent(IContainer container, DailyEarlyExitReportViewModel data)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Item().Element(c => ComposeEarlyExitSummary(c, data));

                column.Item().PaddingTop(15);

                if (data.ClassesReport != null && data.ClassesReport.Any())
                {
                    foreach (var classData in data.ClassesReport)
                    {
                        column.Item().Element(c => ComposeClassEarlyExitSection(c, classData));
                        column.Item().PaddingTop(15);
                    }
                }
                else
                {
                    column.Item().AlignCenter().Text("لا يوجد طلاب بخروج مبكر في هذا اليوم 🎉")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Green.Darken1);
                }
            });
        }

        // الإحصائيات العامة لتقرير الخروج المبكر
        private void ComposeEarlyExitSummary(IContainer container, DailyEarlyExitReportViewModel data)
        {
            int totalClasses = data.ClassesReport?.Count ?? 0;
            int totalEarlyExit = data.ClassesReport?.Sum(x => x.EarlyExitStudents) ?? 0;

            container.Background(Colors.Grey.Lighten3)
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("إجمالي الطلاب مستأذنين")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);

                        col.Item().Text(totalEarlyExit.ToString())
                            .AlignRight()
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Orange.Darken2);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text("عدد الفصول")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);

                        col.Item().Text(totalClasses.ToString())
                            .AlignRight()
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });
                });
        }

        // قسم كل صف في تقرير الخروج المبكر
        private void ComposeClassEarlyExitSection(IContainer container, ClassEarlyExitViewModel classData)
        {
            double percentage = classData.TotalStudents == 0
                ? 0
                : Math.Round((double)classData.EarlyExitStudents / classData.TotalStudents * 100, 2);

            container.Column(column =>
            {
                column.Item().Background(Colors.Orange.Lighten3)
                    .Padding(8)
                    .Row(row =>
                    {
                        row.ConstantItem(300).AlignLeft().Text(
                            $"النسبة: {percentage}% | " +
                            $"مستأذنين: {classData.EarlyExitStudents} | " +
                            $"إجمالي الطلاب: {classData.TotalStudents}")
                            .FontSize(10)
                            .AlignRight()
                            .FontColor(Colors.Red.Darken2);

                        row.RelativeItem().AlignRight().Text($"{classData.ClassName} - {classData.ClassRoomName}")
                           .FontSize(14)
                           .Bold()
                           .FontColor(Colors.Orange.Darken3);
                    });

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(40);
                    });

                    table.Header(header =>
                    {
                        string[] headers = {
                    "ملاحظات","أيام الاستئذان المتتالية","السبب","وقت الخروج",
                    "رقم الجوال","كود الطالب","اسم الطالب","#"
                };

                        foreach (var h in headers)
                        {
                            header.Cell().Background(Colors.Orange.Darken2)
                                .Padding(5).AlignCenter()
                                .Text(h).FontColor(Colors.White).Bold().FontSize(10);
                        }
                    });

                    int index = 1;

                    foreach (var st in classData.EarlyExitStudentsList)
                    {
                        var bgColor = (index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White);

                        string exitTimeDisplay = string.IsNullOrWhiteSpace(st.ExitTime)
                            ? "-"
                            : st.ExitTime;

                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text(st.Notes ?? "-").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignCenter().Text($"{st.ConsecutiveEarlyExitDays} يوم").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(st.Reason ?? "استئذان").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(exitTimeDisplay).FontColor(Colors.Orange.Darken2).Bold().FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text(st.StudentPhone ?? "-").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text(st.StudentCode ?? "-").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text(st.StudentName).FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(index.ToString()).FontSize(9);

                        index++;
                    }
                });
            });
        }


        // إنشاء بطاقات الطلاب PDF
        public byte[] GenerateStudentCards(List<StudentCardViewModel> students)
        {
            var qrCache = new Dictionary<string, byte[]>();

            int cardsPerPage = 6; // ← عدد البطاقات في الصفحة (3 صفوف × 2 كارت)

            var document = Document.Create(container =>
            {
                // تقسيم الطلاب إلى صفحات — كل صفحة تحتوي 6 طلاب فقط
                var pages = students
                    .Select((s, index) => new { s, index })
                    .GroupBy(x => x.index / cardsPerPage)
                    .Select(g => g.Select(x => x.s).ToList())
                    .ToList();

                foreach (var pageStudents in pages)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
                        page.DefaultTextStyle(x => x.DirectionFromRightToLeft());

                        page.Content().Column(column =>
                        {
                            for (int i = 0; i < pageStudents.Count; i += 2)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().MaxWidth(270).Padding(5)
                                        .Element(c => ComposeStudentCard(c, pageStudents[i], qrCache));

                                    if (i + 1 < pageStudents.Count)
                                    {
                                        row.RelativeItem().MaxWidth(270).Padding(5)
                                            .Element(c => ComposeStudentCard(c, pageStudents[i + 1], qrCache));
                                    }
                                    else
                                    {
                                        row.RelativeItem();
                                    }
                                });

                                // مسافة بين الصفوف
                                if (i + 2 < pageStudents.Count)
                                {
                                    column.Item().PaddingVertical(10);
                                }
                            }
                        });

                        page.Footer()
                            .AlignCenter()
                            .Text($"تم الطباعة في: {DateTime.Now:yyyy/MM/dd hh:mm tt}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
                }
            });

            return document.GeneratePdf();
        }


        // تكوين بطاقة الطالب
        private void ComposeStudentCard(IContainer container, StudentCardViewModel student, Dictionary<string, byte[]> qrCache)
        {
            container.Border(2)
                .BorderColor(Colors.Blue.Darken2)
                .Background(Colors.White)
                .Column(column =>
                {
                    // Header مع الشريط العلوي
                    column.Item().Height(35).Background(Colors.Blue.Darken2)
                        .Padding(5)
                        .AlignCenter() // محاذاة المركز
                        .AlignMiddle()
                        .Text(_schoolName)
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.White);

                    // اسم الطالبة على سطر منفصل
                    column.Item().Padding(10).PaddingBottom(5)
                        .AlignCenter()
                        .Text(student.StudentName)
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    // خط فاصل
                    column.Item().PaddingHorizontal(10).PaddingBottom(8)
                        .LineHorizontal(1)
                        .LineColor(Colors.Grey.Lighten2);

                    // محتوى البطاقة - البيانات والـ QR
                    column.Item().PaddingHorizontal(10).PaddingBottom(10).Row(row =>
                    {

                        // القسم الأيسر - QR Code
                        row.RelativeItem().AlignLeft().AlignTop().Column(qrColumn =>
                        {
                            qrColumn.Item().AlignCenter().Width(85).Height(85)
                                .Border(2)
                                .BorderColor(Colors.Blue.Lighten2)
                                .Background(Colors.White)
                                .Padding(2)
                                .Element(c =>
                                {
                                    var qrKey = $"{student.StudentCode}_{student.StudentName}";

                                    if (!qrCache.ContainsKey(qrKey))
                                    {
                                        var qrGenerator = new QRCodeGenerator();
                                        var qrData = $"الأسم: {student.StudentName}\nكود الطالبة: {student.StudentCode}\nجوال: {student.StudentPhone}";
                                        var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.H);
                                        var qrCode = new QRCode(qrCodeData);
                                        var qrBitmap = qrCode.GetGraphic(20);

                                        using (var ms = new MemoryStream())
                                        {
                                            qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                            qrCache[qrKey] = ms.ToArray();
                                        }
                                    }

                                    c.Image(qrCache[qrKey]);
                                });
                        });


                        
                        // القسم الأيمن - المعلومات
                        row.RelativeItem(1).AlignRight().Column(infoColumn =>
                        {
                            infoColumn.Item().PaddingBottom(6).AlignRight().Text(text =>
                            {
                                text.Span("كود الطالبة: ").FontSize(9).FontColor(Colors.Black).Bold();
                                text.Span(" ");
                                text.Span(student.StudentCode ?? "-").FontSize(9).FontColor(Colors.Black).Bold();
                            });

                            infoColumn.Item().PaddingBottom(6).AlignRight().Text(text =>
                            {
                                text.Span("الصف: ").FontSize(9).FontColor(Colors.Black).Bold();
                                text.Span(" ");
                                text.Span(student.ClassName ?? "-").FontSize(9).FontColor(Colors.Black).Bold();
                            });

                            //infoColumn.Item().PaddingBottom(6).AlignRight().Text(text =>
                            //{
                            //    text.Span("الفصل: ").FontSize(9).FontColor(Colors.Black).Bold();
                            //    text.Span(" ");
                            //    text.Span(student.ClassRoomName ?? "-").FontSize(9).FontColor(Colors.Black).Bold();
                            //});

                            //infoColumn.Item().PaddingBottom(6).AlignRight().Text(text =>
                            //{
                            //    text.Span("رقم الجوال: ").FontSize(9).FontColor(Colors.Black).Bold();
                            //    text.Span(" ");
                            //    text.Span(student.StudentPhone ?? "-").FontSize(9).FontColor(Colors.Black).Bold();
                            //});


                        });

                    });

                });
        }



        //   public byte[] GenerateStudentAbsenceNotice(StudentAbsenceNoticeViewModel data)
        //   {
        //       var document = Document.Create(container =>
        //       {
        //           container.Page(page =>
        //           {
        //               page.Size(PageSizes.A4);
        //               page.Margin(1.5f, Unit.Centimetre);
        //               page.PageColor(Colors.White);
        //               page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(12));
        //               page.DefaultTextStyle(x => x.DirectionFromRightToLeft());

        //               page.Content().Element(c => ComposeAbsenceNotice(c, data));
        //           });
        //       });

        //       return document.GeneratePdf();
        //   }

        //   // تكوين إخطار الغياب
        //   private void ComposeAbsenceNotice(IContainer container, StudentAbsenceNoticeViewModel data)
        //   {
        //       container.Column(column =>
        //       {
        //           column.Spacing(0);

        //           // ===== Header الرسمي =====
        //           column.Item().Border(2).BorderColor(Colors.Black).Padding(10).Column(headerCol =>
        //           {
        //               headerCol.Spacing(0);

        //               headerCol.Item().Row(row =>
        //               {
        //                   row.Spacing(10);

        //                   // الرقم على اليسار
        //                   row.ConstantItem(120).AlignLeft().Column(numCol =>
        //                   {
        //                       numCol.Item().Text("العدد: 1447/")
        //                           .FontSize(11)
        //                           .FontColor(Colors.Black);

        //                       numCol.Item().PaddingTop(3).Text("الفصل الدراسي: الأول")
        //                           .FontSize(10)
        //                           .FontColor(Colors.Black);
        //                   });

        //                   // الشعارات والمعلومات في المنتصف
        //                   row.RelativeItem().AlignCenter().Column(centerCol =>
        //                   {
        //                       centerCol.Spacing(3);

        //                       // شعار الوزارة (أعلى)
        //                       centerCol.Item().AlignCenter().Column(logoCol =>
        //                       {
        //                           if (File.Exists(_ministryLogoPath))
        //                           {
        //                               logoCol.Item()
        //                                   .Width(60)
        //                                   .Height(60)
        //                                   .Image(_ministryLogoPath, ImageScaling.FitArea);
        //                           }
        //                       });

        //                       // وزارة التعليم
        //                       centerCol.Item().AlignCenter().Text("وزارة التعليم")
        //                           .FontSize(14)
        //                           .Bold()
        //                           .FontColor(Colors.Black);

        //                       // المتوسطة الخامسة عشرة
        //                       centerCol.Item().AlignCenter().Text("المتوسطة الخامسة عشرة")
        //                           .FontSize(12)
        //                           .Bold()
        //                           .FontColor(Colors.Black);

        //                       // بالطائف
        //                       centerCol.Item().AlignCenter().Text("بالطائف")
        //                           .FontSize(11)
        //                           .FontColor(Colors.Black);
        //                   });

        //                   // شعار المملكة على اليمين
        //                   row.ConstantItem(120).AlignRight().Column(saudiCol =>
        //                   {
        //                       // شعار المملكة
        //                       if (File.Exists(_schoolLogoPath))
        //                       {
        //                           saudiCol.Item().AlignRight()
        //                               .Width(70)
        //                               .Height(50)
        //                               .Image(_schoolLogoPath, ImageScaling.FitArea);
        //                       }

        //                       saudiCol.Item().PaddingTop(3).AlignRight().Text("المملكة العربية السعودية")
        //                           .FontSize(9)
        //                           .FontColor(Colors.Black);
        //                   });
        //               });
        //           });

        //           // ===== عنوان الإخطار =====
        //           column.Item().PaddingTop(20).Border(1).BorderColor(Colors.Black)
        //               .AlignCenter().Padding(8)
        //               .Text("إخطار غياب طالب")
        //               .FontSize(16)
        //               .Bold()
        //               .FontColor(Colors.Black);

        //           // ===== معلومات الطالب =====
        //           column.Item().PaddingTop(15).PaddingHorizontal(20).Column(infoCol =>
        //           {
        //               infoCol.Spacing(8);

        //               // اسم الطالب
        //               infoCol.Item().Row(row =>
        //               {
        //                   row.RelativeItem().AlignRight()
        //                       .Text($"اسم الطالب/ــة: {data.StudentName}")
        //                       .FontSize(12)
        //                       .FontColor(Colors.Black);
        //               });

        //               // الفصل
        //               infoCol.Item().Row(row =>
        //               {
        //                   row.RelativeItem().AlignRight()
        //                       .Text($"الفصل: {data.ClassName}")
        //                       .FontSize(12)
        //                       .FontColor(Colors.Black);
        //               });
        //           });

        //           // ===== خط منقط =====
        //           //column.Item().PaddingTop(10).PaddingHorizontal(20)
        //           //    .Canvas((canvas, size) =>
        //           //    {
        //           //        canvas.DrawLine(
        //           //            new Point(0, 0),
        //           //            new Point(size.Width, 0),
        //           //            lineColor: Colors.Black,
        //           //            lineWidth: 1,
        //           //            dashPattern: new float[] { 5, 3 }
        //           //        );
        //           //    });

        //           column.Item().PaddingTop(10).PaddingHorizontal(20)
        //            .BorderTop(1, Unit.Point)
        //            .BorderColor(Colors.Black)
        //            .ExtendHorizontal();

        ////           column.Item().PaddingTop(10).PaddingHorizontal(20)
        ////.Height(1).Background(Colors.Black);

        //           // ===== جدول أيام الغياب =====
        //           column.Item().PaddingTop(15).PaddingHorizontal(20).Column(tableCol =>
        //           {
        //               tableCol.Item().AlignRight().PaddingBottom(8)
        //                   .Text("أيام الغياب")
        //                   .FontSize(13)
        //                   .Bold()
        //                   .FontColor(Colors.Black);

        //               tableCol.Item().Table(table =>
        //               {
        //                   // تعريف الأعمدة (من اليسار لليمين في RTL)
        //                   table.ColumnsDefinition(columns =>
        //                   {
        //                       columns.RelativeColumn(3); // اليوم
        //                       columns.RelativeColumn(3); // التاريخ
        //                       columns.ConstantColumn(50); // م
        //                   });

        //                   // Header
        //                   table.Header(header =>
        //                   {
        //                       header.Cell().Border(1).BorderColor(Colors.Black)
        //                           .Background(Colors.Grey.Lighten3)
        //                           .Padding(5).AlignCenter()
        //                           .Text("اليوم").FontSize(11).Bold();

        //                       header.Cell().Border(1).BorderColor(Colors.Black)
        //                           .Background(Colors.Grey.Lighten3)
        //                           .Padding(5).AlignCenter()
        //                           .Text("التاريخ").FontSize(11).Bold();

        //                       header.Cell().Border(1).BorderColor(Colors.Black)
        //                           .Background(Colors.Grey.Lighten3)
        //                           .Padding(5).AlignCenter()
        //                           .Text("م").FontSize(11).Bold();
        //                   });

        //                   // Rows
        //                   foreach (var absence in data.AbsenceDates)
        //                   {
        //                       table.Cell().Border(1).BorderColor(Colors.Black)
        //                           .Padding(5).AlignCenter()
        //                           .Text(absence.DayName).FontSize(11);

        //                       table.Cell().Border(1).BorderColor(Colors.Black)
        //                           .Padding(5).AlignCenter()
        //                           .Text(absence.Date).FontSize(11);

        //                       table.Cell().Border(1).BorderColor(Colors.Black)
        //                           .Padding(5).AlignCenter()
        //                           .Text(absence.RowNumber.ToString()).FontSize(11);
        //                   }
        //               });
        //           });

        //           // ===== التذكير الأول =====
        //           column.Item().PaddingTop(20).Border(1).BorderColor(Colors.Black)
        //               .AlignCenter().Padding(8)
        //               .Text("التذكير الأول")
        //               .FontSize(14)
        //               .Bold()
        //               .FontColor(Colors.Black);

        //           // ===== نص الإخطار =====
        //           column.Item().PaddingTop(15).PaddingHorizontal(20)
        //               .Text(text =>
        //               {
        //                   text.Span($"عزيزي/ـتي {data.StudentGuardianType} آل حوج اختطافي\n")
        //                       .FontSize(12).FontColor(Colors.Black);

        //                   text.Span("نود تنبيهك بأن مصيحة ابنك/ــتك مضمون بأننا قد بدأنا بملاحظة تكرار غيابه/ـها عن المدرسة. نأمل منكم الاهتمام ")
        //                       .FontSize(11).FontColor(Colors.Black);

        //                   text.Span("بانتظام ابنك/ــتك في الحضور إلى المدرسة حيث إن غيابك يؤثر على تحصيله/ـها الدراسي ويجب حضوره/ـها لضمان تعزيز المسؤولية ")
        //                       .FontSize(11).FontColor(Colors.Black);

        //                   text.Span("لدراستكم لديه/ـها توجد تبعات تعوق على عدم الغياب والمواظبة على الحضور للمدرسة.")
        //                       .FontSize(11).FontColor(Colors.Black);
        //               });

        //           // ===== التوقيعات =====
        //           column.Item().PaddingTop(30).PaddingHorizontal(40).Row(row =>
        //           {
        //               row.Spacing(20);

        //               // توقيع الطالب/ـة على اليسار
        //               row.RelativeItem().AlignLeft().Column(sigCol =>
        //               {
        //                   sigCol.Item().AlignRight().Text("اسم الطالب/ــة:")
        //                       .FontSize(11).FontColor(Colors.Black);

        //                   sigCol.Item().PaddingTop(20).AlignRight()
        //                       .LineHorizontal(1).LineColor(Colors.Black);

        //                   sigCol.Item().PaddingTop(3).AlignRight().Text("التوقيع:")
        //                       .FontSize(11).FontColor(Colors.Black);

        //                   sigCol.Item().PaddingTop(20).AlignRight()
        //                       .LineHorizontal(1).LineColor(Colors.Black);
        //               });

        //               // التاريخ على اليمين
        //               row.RelativeItem().AlignRight().Column(dateCol =>
        //               {
        //                   dateCol.Item().AlignRight().Text("التاريخ:")
        //                       .FontSize(11).FontColor(Colors.Black);

        //                   dateCol.Item().PaddingTop(5).AlignRight()
        //                       .Text($"     /     / 1447هـ")
        //                       .FontSize(11).FontColor(Colors.Black);
        //               });
        //           });

        //           // ===== ملاحظة في الأسفل =====
        //           column.Item().PaddingTop(30).PaddingHorizontal(20)
        //               .Border(1).BorderColor(Colors.Black)
        //               .Background(Colors.Grey.Lighten4)
        //               .Padding(10)
        //               .Text("ملاحظة: يرجى إعادة هذا الإخطار موقعاً من ولي الأمر والطالب/ـة")
        //               .FontSize(10)
        //               .Italic()
        //               .FontColor(Colors.Grey.Darken2);
        //       });
        //   }

        public byte[] GenerateStudentAbsenceNotice(StudentAbsenceNoticeViewModel data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(12));
                    page.DefaultTextStyle(x => x.DirectionFromRightToLeft());

                    page.Content().Element(c => ComposeAbsenceNotice(c, data));
                });
            });

            return document.GeneratePdf();
        }

        // تكوين إخطار الغياب
        private void ComposeAbsenceNotice(IContainer container, StudentAbsenceNoticeViewModel data)
        {
            container.Column(column =>
            {
                column.Spacing(0);

                // ===== Header الرسمي =====
                column.Item().Border(2).BorderColor(Colors.Black).Padding(10).Column(headerCol =>
                {
                    headerCol.Spacing(0);

                    headerCol.Item().Row(row =>
                    {
                        row.Spacing(10);

                        // الرقم على اليسار
                        row.ConstantItem(120).AlignLeft().Column(numCol =>
                        {
                            numCol.Item().Text("العدد: 1447/")
                                .FontSize(11)
                                .FontColor(Colors.Black);

                            numCol.Item().PaddingTop(3).Text("الفصل الدراسي: الأول")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                        });

                        // الشعارات والمعلومات في المنتصف
                        row.RelativeItem().AlignCenter().Column(centerCol =>
                        {
                            centerCol.Spacing(3);

                            // شعار الوزارة (أعلى)
                            centerCol.Item().AlignCenter().Column(logoCol =>
                            {
                                if (File.Exists(_ministryLogoPath))
                                {
                                    logoCol.Item()
                                        .Width(60)
                                        .Height(60)
                                        .Image(_ministryLogoPath, ImageScaling.FitArea);
                                }
                            });

                            // وزارة التعليم
                            centerCol.Item().AlignCenter().Text("وزارة التعليم")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Black);

                            // المتوسطة الخامسة عشرة
                            centerCol.Item().AlignCenter().Text("المتوسطة الخامسة عشرة")
                                .FontSize(12)
                                .Bold()
                                .FontColor(Colors.Black);

                            // بالطائف
                            centerCol.Item().AlignCenter().Text("بالطائف")
                                .FontSize(11)
                                .FontColor(Colors.Black);
                        });

                        // شعار المملكة على اليمين
                        row.ConstantItem(120).AlignRight().Column(saudiCol =>
                        {
                            // شعار المملكة
                            if (File.Exists(_schoolLogoPath))
                            {
                                saudiCol.Item().AlignRight()
                                    .Width(70)
                                    .Height(50)
                                    .Image(_schoolLogoPath, ImageScaling.FitArea);
                            }

                            saudiCol.Item().PaddingTop(3).AlignRight().Text("المملكة العربية السعودية")
                                .FontSize(9)
                                .FontColor(Colors.Black);
                        });
                    });
                });

                // ===== عنوان الإخطار =====
                column.Item().PaddingTop(20).Border(1).BorderColor(Colors.Black)
                    .AlignCenter().Padding(8)
                    .Text("إخطار غياب طالب")
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Black);

                // ===== معلومات الطالب =====
                column.Item().PaddingTop(15).PaddingHorizontal(20).Column(infoCol =>
                {
                    infoCol.Spacing(8);

                    // اسم الطالب
                    infoCol.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight()
                            .Text($"اسم الطالب/ــة: {data.StudentName}")
                            .FontSize(12)
                            .FontColor(Colors.Black);
                    });

                    // الفصل
                    infoCol.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight()
                            .Text($"الفصل: {data.ClassName}")
                            .FontSize(12)
                            .FontColor(Colors.Black);
                    });
                });

                // ===== خط منقط =====
                column.Item().PaddingTop(10).PaddingHorizontal(20)
                    .Height(1)
                    .Background(Colors.White)
                    .ExtendHorizontal();

                // ===== جدول أيام الغياب =====
                column.Item().PaddingTop(15).PaddingHorizontal(20).Column(tableCol =>
                {
                    tableCol.Item().AlignRight().PaddingBottom(8)
                        .Text("أيام الغياب")
                        .FontSize(13)
                        .Bold()
                        .FontColor(Colors.Black);

                    tableCol.Item().Table(table =>
                    {
                        // تعريف الأعمدة (من اليسار لليمين في RTL)
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // م
                            columns.RelativeColumn(2); // التاريخ
                            columns.RelativeColumn(1); // اليوم
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .Background(Colors.White)
                                .Padding(5).AlignCenter()
                                .Text("م").FontSize(11).Bold();

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .Background(Colors.White)
                                .Padding(5).AlignCenter()
                                .Text("التاريخ").FontSize(11).Bold();

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .Background(Colors.White)
                                .Padding(5).AlignCenter()
                                .Text("اليوم").FontSize(11).Bold();
                        });

                        // Rows
                        for (int i = 0; i < data.AbsenceDates.Count; i++)
                        {
                            var absence = data.AbsenceDates[i];

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Background(Colors.White)
                                .Padding(5).AlignCenter()
                                .Text((i + 1).ToString()).FontSize(11);

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Background(Colors.White)
                                .Padding(5).AlignCenter()
                                .Text(absence.Date).FontSize(11);

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Background(Colors.White)
                                .Padding(5).AlignCenter()
                                .Text(absence.DayName).FontSize(11);
                        }
                    });
                });

                // ===== التذكير الأول =====
                column.Item().PaddingTop(20).Border(1).BorderColor(Colors.Black)
                    .AlignCenter().Padding(8)
                    .Text("التذكير الأول")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Black);

                // ===== نص الإخطار =====
                column.Item().PaddingTop(15).PaddingHorizontal(20)
                    .Text(text =>
                    {
                        text.Span($"عزيزي/ـتي الطالب/ــة {data.StudentName}\n")
                            .FontSize(12).FontColor(Colors.Black);

                        text.Span("أود تنبيهك بأن مجموع أيام غيابك قد بلغ ما يستدعي إرسال هذا الإخطار. لذا نرجو منك الالتزام والانتظام في الحضور إلى المدرسة حيث إن غيابك يؤثر على ")
                            .FontSize(11).FontColor(Colors.Black);

                        text.Span("تحصيلك الدراسي ويخفض مستواك بين زملائك. لذا نرجو منك الالتزام بعدم الغياب والمواظبة على الحضور للمدرسة.\n\n")
                            .FontSize(11).FontColor(Colors.Black);

                        text.Span("ونأمل من ولي أمرك متابعة انتظامك في الحضور والالتزام بالدوام المدرسي.")
                            .FontSize(11).FontColor(Colors.Black);
                    });

                // ===== التوقيعات =====
                column.Item().PaddingTop(30).PaddingHorizontal(40).Row(row =>
                {
                    row.Spacing(20);

                    // توقيع الطالب/ـة على اليسار
                    row.RelativeItem().AlignLeft().Column(sigCol =>
                    {
                        sigCol.Item().AlignRight().Text("اسم الطالب/ــة:")
                            .FontSize(11).FontColor(Colors.Black);

                        sigCol.Item().PaddingTop(15).AlignRight()
                            .LineHorizontal(1).LineColor(Colors.Black);

                        sigCol.Item().PaddingTop(3).AlignRight().Text("التوقيع:")
                            .FontSize(11).FontColor(Colors.Black);

                        sigCol.Item().PaddingTop(15).AlignRight()
                            .LineHorizontal(1).LineColor(Colors.Black);
                    });

                    // التاريخ على اليمين
                    row.RelativeItem().AlignRight().Column(dateCol =>
                    {
                        dateCol.Item().AlignRight().Text("التاريخ:")
                            .FontSize(11).FontColor(Colors.Black);

                        dateCol.Item().PaddingTop(15).AlignRight()
                            .LineHorizontal(1).LineColor(Colors.Black);
                    });
                });

                // ===== ملاحظة في الأسفل =====
                column.Item().PaddingTop(30).PaddingHorizontal(20)
                    .Border(1).BorderColor(Colors.Black)
                    .Background(Colors.Grey.Lighten4)
                    .Padding(10)
                    .Text("ملاحظة: يرجى إعادة هذا الإخطار موقعاً من ولي الأمر والطالب/ـة")
                    .FontSize(10)
                    .Italic()
                    .FontColor(Colors.Grey.Darken2);
            });
        }
    }

}
