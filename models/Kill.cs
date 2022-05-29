using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillBot.models
{
    public class Kill
    {
        [Key]
        public Guid Id { get; set; }
        public string KillerUsername { get; set; } = "";
        public string TargetUsername { get; set; } = "";
        public DateTime CreatedAt { get; set; }

    }
}
