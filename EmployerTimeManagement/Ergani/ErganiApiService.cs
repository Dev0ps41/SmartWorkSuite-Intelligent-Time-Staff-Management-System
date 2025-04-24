using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EmployerTimeManagement.Ergani;
using System.Windows.Controls;
using System.Windows;
using System.Net.Http.Json;


namespace EmployerTimeManagement.Ergani
{
    public static class ErganiApiService
    {
        public static async Task<bool> SubmitToErganiAsync(List<WorkLog> logs)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            string baseUrl = company.UseProductionApi
                ? "https://ergani-api-prod.example.gr/api/submit"
                : "https://ergani-api-test.example.gr/api/submit";

            var entries = logs.Select(log => new
            {
                afm = log.f_afm,
                surname = log.f_eponymo,
                firstname = log.f_onoma,
                type = log.f_type,
                referenceDate = log.f_reference_date.ToString("yyyy-MM-dd"),
                eventDate = log.f_date.ToString("yyyy-MM-dd"),
                eventTime = log.f_date.ToString("HH:mm"),
                comment = log.f_aitiologia
            }).ToList();

            var payload = new
            {
                afmErgodoti = company.AFM,
                aaParartima = company.BranchId,
                username = company.ErganiUsername,
                password = company.ErganiPassword,
                submissionDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                production = company.UseProductionApi,
                entries = entries
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", company.ErganiApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                foreach (var log in logs)
                {
                    log.IsSent = true;
                    log.SentAt = DateTime.Now;
                }

                context.SaveChanges();
            }

            return response.IsSuccessStatusCode;
        }
        public static async Task<bool> SubmitWTOAsync(List<WTOEntry> entries, bool isWeekly)
        {
            if (entries == null || entries.Count == 0)
                return false;

            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            var groupedEntries = entries
                .GroupBy(e => new { e.EmployeeAFM, e.EmployeeLastName, e.EmployeeFirstName })
                .Select(g => new
                {
                    f_afm = g.Key.EmployeeAFM,
                    f_eponymo = g.Key.EmployeeLastName,
                    f_onoma = g.Key.EmployeeFirstName,
                    Analytics = g.Select(e => new
                    {
                        f_type = e.WorkType,
                        f_from = e.FromTime,
                        f_to = e.ToTime,
                        f_date = !isWeekly ? e.Date?.ToString("dd/MM/yyyy") : null,
                        f_day = isWeekly ? e.DayOfWeek : null
                    }).ToList()
                }).ToList();

            var payload = new
            {
                WTOS = new
                {
                    WTO = new[]
                    {
                new
                {
                    f_aa_pararthmatos = company.BranchId ?? "0",
                    f_rel_protocol = "",
                    f_rel_date = "",
                    f_comments = "Υποβολή WTO από εφαρμογή",
                    f_from_date = "",
                    f_to_date = "",
                    Ergazomenoi = new
                    {
                        ErgazomenoiWTO = groupedEntries.Select(e => new
                        {
                            f_afm = e.f_afm,
                            f_eponymo = e.f_eponymo,
                            f_onoma = e.f_onoma,
                            f_date = isWeekly ? null : e.Analytics.First().f_date,
                            f_day = isWeekly ? e.Analytics.First().f_day : null,
                            ErgazomenosAnalytics = new
                            {
                                ErgazomenosWTOAnalytics = e.Analytics.Select(a => new
                                {
                                    f_type = a.f_type,
                                    f_from = a.f_from,
                                    f_to = a.f_to
                                }).ToList()
                            }
                        }).ToList()
                    }
                }
            }
                }
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            string apiUrl = "http://trialeservices.yeka.gr/WebServicesAPI/api/Documents/WTO";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", company.ErganiApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(apiUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"WTO Error: {error}");
                return false;
            }
        }

        public static async Task<bool> SubmitOvertimeAsync(List<OvertimeEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return false;

            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            string apiUrl = "http://trialeservices.yeka.gr/WebServicesAPI/api/Documents/EOvertime";

            // Δημιουργία JSON payload
            var payload = new
            {
                EOvertime = new
                {
                    f_aa_pararthmatos = company.BranchId ?? "0",
                    f_rel_protocol = "",
                    f_rel_date = "",
                    f_comments = "Υποβολή υπερωριών από εφαρμογή",
                    Ergazomenoi = new
                    {
                        ErgazomenoiEOvertime = entries.Select(e => new
                        {
                            f_afm = e.EmployeeAFM,
                            f_date = e.Date.ToString("dd/MM/yyyy"),
                            f_hours = e.Hours,
                            f_aitiologia = e.Reason ?? ""
                        }).ToList()
                    }
                }
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", company.ErganiApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(apiUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                foreach (var e in entries)
                {
                    e.IsSent = true;
                    e.SentAt = DateTime.Now;
                }

                context.SaveChanges();
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("EOvertime API error: " + error);
                return false;
            }
        }

        public static async Task<bool> SubmitE3Async(E3Entry entry)
        {
            if (entry == null)
                return false;

            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            string baseUrl = company.UseProductionApi
                ? "https://ergani-api-prod.example.gr/api/e3"
                : "https://ergani-api-test.example.gr/api/e3";

            var payload = new
            {
                afmErgodoti = company.AFM,
                aaParartima = company.BranchId,
                username = company.ErganiUsername,
                password = company.ErganiPassword,
                submissionDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                production = company.UseProductionApi,
                employee = new
                {
                    afm = entry.AFM,
                    amka = entry.AMKA,
                    firstName = entry.FirstName,
                    lastName = entry.LastName,
                    fatherName = entry.FatherName,
                    hireDate = entry.HireDate.ToString("yyyy-MM-dd"),
                    specialty = entry.Specialty,
                    address = entry.Address,
                    city = entry.City,
                    postalCode = entry.PostalCode,
                    doy = entry.DOY,
                    contractType = entry.ContractType,
                    weeklyHours = entry.WeeklyHours,
                    notes = entry.Notes
                }
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", company.ErganiApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                entry.IsSent = true;
                entry.SentAt = DateTime.Now;
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine("E3 Submit Error: " + error);
            return false;
        }

        public static async Task<bool> SubmitWorkingStatusChangesAsync(List<WorkingStatusChangeEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return false;

            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            string baseUrl = company.UseProductionApi
                ? "https://ergani-api-prod.example.gr/api/working-status-change"
                : "https://ergani-api-test.example.gr/api/working-status-change";

            var payload = new
            {
                afmErgodoti = company.AFM,
                aaParartima = company.BranchId,
                username = company.ErganiUsername,
                password = company.ErganiPassword,
                submissionDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                production = company.UseProductionApi,
                changes = entries.Select(e => new
                {
                    afm = e.EmployeeAFM,
                    name = e.EmployeeName,
                    changeDate = e.Date.ToString("yyyy-MM-dd"),
                    changeType = e.ChangeType,
                    comment = e.Comment
                }).ToList()
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", company.ErganiApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                context.SaveChanges();
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine("WorkingStatusChange Error: " + error);
            return false;
        }

        public static async Task<bool> SubmitHolidaysAsync(List<HolidayEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return false;

            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            string baseUrl = company.UseProductionApi
                ? "https://ergani-api-prod.example.gr/api/wtoHoliday"
                : "https://ergani-api-test.example.gr/api/wtoHoliday";

            var payload = new
            {
                afmErgodoti = company.AFM,
                aaParartima = company.BranchId,
                username = company.ErganiUsername,
                password = company.ErganiPassword,
                submissionDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                production = company.UseProductionApi,
                holidays = entries.Select(e => new
                {
                    afm = e.EmployeeAFM,
                    fromDate = e.FromDate.ToString("yyyy-MM-dd"),
                    toDate = e.ToDate.ToString("yyyy-MM-dd"),
                    leaveType = e.LeaveType
                }).ToList()
            };

            return await PostAndHandleHolidayResponse(entries, payload, baseUrl, context);
        }
        public static async Task<bool> SubmitHolidayCorrectionAsync(List<HolidayEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return false;

            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            string baseUrl = company.UseProductionApi
                ? "https://ergani-api-prod.example.gr/api/wtoHolidayCor"
                : "https://ergani-api-test.example.gr/api/wtoHolidayCor";

            var payload = new
            {
                afmErgodoti = company.AFM,
                aaParartima = company.BranchId,
                username = company.ErganiUsername,
                password = company.ErganiPassword,
                submissionDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                production = company.UseProductionApi,
                corrections = entries.Select(e => new
                {
                    rel_protocol = e.RelProtocol,
                    afm = e.EmployeeAFM,
                    fromDate = e.FromDate.ToString("yyyy-MM-dd"),
                    toDate = e.ToDate.ToString("yyyy-MM-dd"),
                    leaveType = e.LeaveType
                }).ToList()
            };

            return await PostAndHandleHolidayResponse(entries, payload, baseUrl, context);
        }
        private static async Task<bool> PostAndHandleHolidayResponse(List<HolidayEntry> entries, object payload, string url, AppDbContext context)
        {
            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.CompanyInfos.First().ErganiApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                context.SaveChanges();
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Holiday Submit Error: " + error);
            return false;
        }

        public static async Task<bool> SubmitE1Async(List<E1Entry> entries, string tableType)
        {
            if (entries == null || entries.Count == 0)
                return false;

            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                throw new Exception("Λείπουν στοιχεία σύνδεσης με το ΕΡΓΑΝΗ API.");

            string baseUrl = company.UseProductionApi
                ? "https://ergani-api-prod.example.gr/api/e1"
                : "https://ergani-api-test.example.gr/api/e1";

            var payload = new
            {
                afmErgodoti = company.AFM,
                aaParartima = company.BranchId,
                username = company.ErganiUsername,
                password = company.ErganiPassword,
                submissionDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                tableType = tableType, // π.χ. Ετήσιος ή Τροποποιητικός
                production = company.UseProductionApi,
                employees = entries.Select(e => new
                {
                    afm = e.AFM,
                    amka = e.AMKA,
                    firstName = e.FirstName,
                    lastName = e.LastName,
                    fatherName = e.FatherName,
                    specialty = e.Specialty,
                    workHours = e.WorkHours,
                    contractType = e.ContractType,
                    employmentType = e.EmploymentType,
                    hireDate = e.HireDate.ToString("yyyy-MM-dd"),
                    address = e.Address,
                    city = e.City,
                    postalCode = e.PostalCode
                }).ToList()
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", company.ErganiApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                    context.E1Entries.Update(entry);
                }
                await context.SaveChangesAsync();
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine("E1 Submit Error: " + error);
            return false;
        }


        public static async Task<bool> SubmitE2Async(List<E2Entry> entries)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null)
                throw new Exception("Λείπουν τα στοιχεία επιχείρησης.");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company.ErganiApiKey);

            var payload = new
            {
                company = company.AFM,
                entries = entries
            };

            var response = await client.PostAsJsonAsync("https://ergani-api/e2", payload);
            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public static async Task<bool> SubmitE4Async(List<E4Entry> entries)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null)
                throw new Exception("Λείπουν τα στοιχεία της επιχείρησης.");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company.ErganiApiKey);

            var payload = new
            {
                company = company.AFM,
                entries = entries
            };

            var apiUrl = company.UseProductionApi
                ? "https://ergani.gov.gr/api/e4"
                : "https://ergani-test.gov.gr/api/e4";

            var response = await client.PostAsJsonAsync(apiUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        public static async Task<bool> SubmitE5Async(List<E5Entry> entries)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null)
                throw new Exception("Λείπουν τα στοιχεία επιχείρησης.");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company.ErganiApiKey);

            var payload = new
            {
                company = company.AFM,
                entries = entries.Select(e => new
                {
                    afm = e.AFM,
                    fullName = e.FullName,
                    terminationDate = e.TerminationDate.ToString("yyyy-MM-dd"),
                    reason = e.Reason
                }).ToList()
            };

            var url = company.UseProductionApi
                ? "https://ergani-api.gov.gr/api/e5"
                : "https://ergani-api-dev.gov.gr/api/e5";

            var response = await client.PostAsJsonAsync(url, payload);
            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public static async Task<bool> SubmitE6Async(List<E6Entry> entries)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null)
                throw new Exception("Λείπουν τα στοιχεία επιχείρησης.");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company.ErganiApiKey);

            var payload = new
            {
                company = company.AFM,
                entries = entries.Select(e => new
                {
                    afm = e.AFM,
                    fullName = e.FullName,
                    startDate = e.StartDate.ToString("yyyy-MM-dd"),
                    endDate = e.EndDate.ToString("yyyy-MM-dd"),
                    specialty = e.Specialty
                }).ToList()
            };

            var url = company.UseProductionApi
                ? "https://ergani-api.gov.gr/api/e6"
                : "https://ergani-api-dev.gov.gr/api/e6";

            var response = await client.PostAsJsonAsync(url, payload);
            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        public static async Task<bool> SubmitE7Async(List<E7Entry> entries)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null)
                throw new Exception("Λείπουν τα στοιχεία επιχείρησης.");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company.ErganiApiKey);

            var payload = new
            {
                company = company.AFM,
                entries = entries.Select(e => new
                {
                    afm = e.AFM,
                    fullName = e.FullName,
                    date = e.Date.ToString("yyyy-MM-dd"),
                    hours = e.Hours,
                    reason = e.Reason
                }).ToList()
            };

            var url = company.UseProductionApi
                ? "https://ergani-api.gov.gr/api/e7"
                : "https://ergani-api-dev.gov.gr/api/e7";

            var response = await client.PostAsJsonAsync(url, payload);
            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public static async Task<bool> SubmitE9Async(List<E9Entry> entries)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null)
                throw new Exception("Λείπουν τα στοιχεία της επιχείρησης.");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company.ErganiApiKey);

            var payload = new
            {
                company = company.AFM,
                entries = entries.Select(e => new
                {
                    afm = e.AFM,
                    fullName = e.FullName,
                    date = e.Date.ToString("yyyy-MM-dd"),
                    project = e.Project
                }).ToList()
            };

            var url = company.UseProductionApi
                ? "https://ergani-api.gov.gr/api/e9"
                : "https://ergani-api-dev.gov.gr/api/e9";

            var response = await client.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public static async Task<bool> SubmitE10Async(List<E10Entry> entries)
        {
            using var context = new AppDbContext();
            var company = context.CompanyInfos.FirstOrDefault();

            if (company == null)
                throw new Exception("Λείπουν τα στοιχεία επιχείρησης.");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company.ErganiApiKey);

            var payload = new
            {
                company = company.AFM,
                entries = entries.Select(e => new
                {
                    afm = e.AFM,
                    fullName = e.FullName,
                    changeDate = e.ChangeDate.ToString("yyyy-MM-dd"),
                    previousData = e.PreviousData,
                    newData = e.NewData
                }).ToList()
            };

            var url = company.UseProductionApi
                ? "https://ergani-api.gov.gr/api/e10"
                : "https://ergani-api-dev.gov.gr/api/e10";

            var response = await client.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                foreach (var entry in entries)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }


        // ✅ Προστέθηκε η μέθοδος ελέγχου σύνδεσης με το ΕΡΓΑΝΗ API
        public static bool TestConnection()
        {
            try
            {
                using var context = new AppDbContext();
                var company = context.CompanyInfos.FirstOrDefault();

                if (company == null || string.IsNullOrWhiteSpace(company.ErganiApiKey))
                    return false;

                string baseUrl = company.UseProductionApi
                    ? "https://ergani-api-prod.example.gr/api/ping"
                    : "https://ergani-api-test.example.gr/api/ping";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", company.ErganiApiKey);
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = client.GetAsync(baseUrl).Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
