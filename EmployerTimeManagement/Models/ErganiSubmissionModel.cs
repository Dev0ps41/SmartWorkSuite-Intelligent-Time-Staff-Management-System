namespace EmployerTimeManagement.Models
{
    public class ErganiSubmissionModel
    {
        public string f_afm { get; set; }
        public string f_eponymo { get; set; }
        public string f_onoma { get; set; }
        public int f_type { get; set; }
        public DateTime f_reference_date { get; set; }
        public DateTime f_date { get; set; }
        public string f_aitiologia { get; set; }

        // Εταιρικά πεδία
        public string employer_afm { get; set; }
        public string employer_name { get; set; }
        public string branch_id { get; set; }
    }
}
