using EmployerTimeManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Windows.Media;
using EmployerTimeManagement.Ergani;
using System.Windows;

namespace EmployerTimeManagement
{
    public partial class DashboardControl : UserControl
    {
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        private readonly AppDbContext _context;

        public DashboardControl()
        {
            InitializeComponent();
            _context = new AppDbContext();

            LoadStats();
            LoadCharts();
            LoadTodayPairs();
            CheckErganiConnection();

            DataContext = this;
        }

        private void LoadStats()
        {
            int employeeCount = _context.Employees.Count();
            EmployeeCountText.Text = employeeCount.ToString();

            int sent = _context.WorkLogs.Count(w => w.IsSent);
            int failed = _context.WorkLogs.Count(w => !w.IsSent);

            if (failed > 0)
            {
                ErganiSubmissionBorder.Visibility = Visibility.Collapsed;
                FailedSubmissionBorder.Visibility = Visibility.Visible;
                FailedSubmissionText.Text = $"Αποτυχημένες: {failed}";
            }
            else
            {
                FailedSubmissionBorder.Visibility = Visibility.Collapsed;
                ErganiSubmissionBorder.Visibility = Visibility.Visible;
                ErganiSubmissionText.Text = $"Επιτυχίες: {sent}";
            }
        }


        private void LoadCharts()
        {
            var lastDays = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i).Date)
                .ToList();

            var dailyData = _context.WorkLogs
                .Where(w => w.f_type == 0 || w.f_type == 1)
                .GroupBy(w => new { w.EmployeeId, w.f_reference_date, w.f_type })
                .ToList()
                .GroupBy(g => g.Key.f_reference_date)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var logs = g.SelectMany(x => x);
                        var entryLogs = logs.Where(x => x.f_type == 0).ToList();
                        var exitLogs = logs.Where(x => x.f_type == 1).ToList();
                        double total = 0;
                        foreach (var entry in entryLogs)
                        {
                            var match = exitLogs.FirstOrDefault(e => e.EmployeeId == entry.EmployeeId);
                            if (match != null)
                            {
                                var duration = (match.f_date - entry.f_date).TotalHours;
                                if (duration > 0)
                                    total += duration;
                            }
                        }
                        return Math.Round(total, 2);
                    });

            var hoursPerDay = lastDays
                .Select(date => dailyData.TryGetValue(date, out var hrs) ? hrs : 0)
                .ToArray();

            double total = hoursPerDay.Sum();
            totalHoursText.Text = $"{total} ώρες";

            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = hoursPerDay,
                    Name = "ώρες",
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint(SKColors.Green, 3),
                    Fill = null
                },
                new LineSeries<double>
                {
                    Values = hoursPerDay,
                    GeometrySize = 10,
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.AliceBlue),
                    GeometryStroke = null,
                    GeometryFill = new SolidColorPaint(SKColors.DarkRed)
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = lastDays.Select(d => d.ToString("dd/MM")).ToArray(),
                    LabelsRotation = 0,
                    TextSize = 14,
                    Padding = new LiveChartsCore.Drawing.Padding(10),
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "",
                    TextSize = 14,
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                }
            };
        }

        private void LoadTodayPairs()
        {
            using var db = new AppDbContext();
            var today = DateTime.Today;

            var pairs = db.WorkLogs
                .Include(w => w.Employee)
                .Where(w => w.f_date.Date == today)
                .AsEnumerable()
                .GroupBy(w => new { w.EmployeeId, Date = w.f_date.Date })
                .Select((g, i) => new WorkLogPair
                {
                    Id = i + 1,
                    EmployeeId = g.Key.EmployeeId,
                    AFM = g.First().Employee.AFM,
                    FirstName = g.First().Employee.FirstName,
                    LastName = g.First().Employee.LastName,
                    Date = g.Key.Date,
                    EntryTime = g.Where(w => w.f_type == 0).OrderBy(w => w.f_date).FirstOrDefault()?.f_date.TimeOfDay,
                    ExitTime = g.Where(w => w.f_type == 1).OrderBy(w => w.f_date).FirstOrDefault()?.f_date.TimeOfDay
                })
                .ToList();

            todayGrid.ItemsSource = pairs;
        }

        private void ViewSubmissionHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new Ergani.SubmissionHistoryWindow(true); // true για να δείξει μόνο τις αποτυχημένες
            historyWindow.ShowDialog();
        }



        private void CheckErganiConnection()
        {
            try
            {
                var isConnected = ErganiApiService.TestConnection();

                if (isConnected)
                {
                    ErganiStatusBorder.Background = new SolidColorBrush(Colors.ForestGreen);
                    ErganiStatusText.Text = "Συνδεδεμένο με";
                }
                else
                {
                    ErganiStatusBorder.Background = new SolidColorBrush(Colors.DarkRed);
                    ErganiStatusText.Text = "Μη Συνδεδεμένο";
                }
            }
            catch
            {
                ErganiStatusBorder.Background = new SolidColorBrush(Colors.DarkRed);
                ErganiStatusText.Text = "Μη Συνδεδεμένο";
            }
        }
    }
}