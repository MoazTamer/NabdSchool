using NabdSchool.Core.Entities;

namespace NabdSchool.Web.ViewModels
{
    public class AuditLogDetailsViewModel
    {
        public AuditLog AuditLog { get; set; }
        public string OldValuesFormatted { get; set; }
        public string NewValuesFormatted { get; set; }
    }
}
